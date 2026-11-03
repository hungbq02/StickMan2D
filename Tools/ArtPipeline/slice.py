"""Cắt sheet RGBA (đã tách nền) thành frame PNG cùng kích thước ô, chân trên cùng 1 đường đáy.

cfg (mỗi nhân vật):
  id, ppu, merge, min_body_h, text_filter, text_min_n, text_max_h, min_comp_w, pad,
  align: {ref, chains, no_return, exclude} (căn giữa các clip theo chỗ nối, xem _align_clips; False = tắt)
  anims: [{name, fps, loop, band:[x0,y0,x1,y1], anchor: head|hip|register|bbox,
           ground (y đáy cố định, tuỳ chọn), air:[i] (frame bay -> dùng đáy trung vị),
           seeds:[[x,y]...] (tách frame dính nhau), merge_frames:[[i,j]], drop:[i], exclude:[[x0,y0,x1,y1]],
           flip (lật ngang), extra_reach, min_body_h, face_*, hip_*, core_rows, reg_range}]
Mỗi frame: 1 "body" (cụm cao >= min_body_h) + các cụm nhỏ gần nó (tia lửa, hạt...).
Neo ngang: head = tâm khối mặt sáng; hip = trọng tâm dải hông; register = dịch để
phần đầu-thân chồng khít frame trước; bbox = tâm khung.
"""
import os, numpy as np
from PIL import Image, ImageDraw
from scipy import ndimage as ndi


def _head_x(rgba, an, x0):
    """Toạ độ x tâm mặt: khối sáng, ít bão hoà, lớn nhất ở phần trên frame."""
    rg = rgba[..., :3].astype(int)
    face = (rg.max(2) - rg.min(2) < an.get('face_sat', 30)) & (rg.mean(2) > an.get('face_min', 185)) & (rgba[..., 3] > 200)
    face[int(face.shape[0] * an.get('face_top', 0.6)):] = False
    fl, fn = ndi.label(face)
    if fn:
        sz = ndi.sum(face, fl, range(1, fn + 1))
        k = int(np.argmax(sz)) + 1
        if sz[k - 1] >= an.get('face_min_px', 80):
            return x0 + np.nonzero(fl == k)[1].mean()
    return None  # không thấy mặt (bị tay / vũ khí che) -> căn khớp hình với frame bên cạnh


def _core_mask(f, an):
    al = f['rgba'][..., 3]
    a = al > 128
    if not a.any():
        a = al > 20  # frame tan biến (khói, hạt)
    rows = np.nonzero(a.any(1))[0]
    if len(rows) == 0:
        return a
    t, b = rows[0], rows[-1]
    m = np.zeros_like(a)
    lo_, hi_ = an.get('core_rows', [0.0, 0.65])
    m[int(t + (b - t) * lo_):int(t + (b - t) * hi_) + 1] = True
    return a & m


def _profile(f, an):
    m = _core_mask(f, an)
    xs = np.nonzero(m.any(0))[0]
    if len(xs) == 0:
        return m.sum(0).astype(np.float64), f['x0'] + m.shape[1] / 2
    return m.sum(0).astype(np.float64), f['x0'] + (xs.min() + xs.max()) / 2


ALIGN_ROWS = [0.0, 0.65]  # đầu + thân trên: mốc ổn định nhất (chân bước, vũ khí, áo choàng làm nhiễu)
RAMP_CAP_U = 0.06       # bù trôi tối đa / frame (đơn vị thế giới) — không tự tạo cú giật mới trong clip


def _body_profile(f):
    """Profile cột của đầu + thân trên, bỏ pixel sáng + bão hoà mạnh (lửa, glow, vòng phép) để hiệu ứng lớn
    không kéo lệch phép khớp giữa các clip."""
    m = _core_mask(f, {'core_rows': ALIGN_ROWS})
    rg = f['rgba'][..., :3].astype(int)
    fx = (rg.max(2) > 200) & (rg.max(2) - rg.min(2) > 90)
    body = m & ~fx
    return (body if body.sum() > 0.3 * m.sum() else m).sum(0).astype(np.float64)


def _pair_shift(fa, fb, R=90):
    """(dx, độ khớp): phần thân của fb lệch dx px (dương = sang phải) so với fa, tính quanh điểm neo."""
    ca = _body_profile(fa); cb = _body_profile(fb)
    L = 2400
    a = np.zeros(L); b = np.zeros(L)
    sa = int(round(fa['x0'] - fa['ax'])) + L // 2; sb = int(round(fb['x0'] - fb['ax'])) + L // 2
    a[sa:sa + len(ca)] = ca; b[sb:sb + len(cb)] = cb
    best = (0, -1.0)
    na = np.sqrt((a * a).sum())
    for dx in range(-R, R + 1):
        bs = np.roll(b, -dx)  # b(x + dx)
        sc = (a * bs).sum() / (na * np.sqrt((bs * bs).sum()) + 1e-9)
        if sc > best[1]:
            best = (dx, sc)
    return best


def _align_clips(frames_all, anims_cfg, align, ppu=100):
    """Căn giữa các clip để thân nhân vật không giật ngang ở các chỗ nối thực tế:
      ref (idle) -> frame đầu mỗi clip; frame cuối mỗi clip -> ref;
      chuỗi [tell, attack, ...]: tell -> attack nối với nhau (attack không nối từ ref, tell không quay về ref).
    Mỗi clip dịch o_đầu ở frame đầu, o_cuối ở frame cuối, nội suy tuyến tính ở giữa (bù clip AI vẽ trôi dần);
    clip lặp / clip không quay về ref / 1 frame dùng 1 khoảng dịch. Bình phương tối thiểu, trọng số = độ khớp^4,
    ưu tiên nhẹ khoảng dịch nhỏ và o_cuối ~ o_đầu (giữ chuyển động thật trong clip).
    align: {ref, chains:[[a,b,...]], no_return:[clip], exclude:[clip]}"""
    ref = align.get('ref', 'idle')
    cfg_by_name = {a['name']: a for a in anims_cfg}
    names = [a['name'] for a in anims_cfg if frames_all.get(a['name'])]
    if ref not in names:
        return None
    exclude = set(align.get('exclude', []))
    no_return = set(align.get('no_return', ['death']))
    follows, leads = {}, set()
    for ch in align.get('chains', []):
        for x, y in zip(ch, ch[1:]):
            follows.setdefault(y, []).append(x)
            leads.add(x)
    edges = []
    for c in names:
        if c == ref or c in exclude:
            continue
        for prev in follows.get(c, [ref]):
            if prev in names and prev not in exclude:
                edges.append((prev, c))
        if c not in leads and c not in no_return:
            edges.append((c, ref))
    clips = [c for c in names if c != ref and c not in exclude]
    # ẩn số: o_đầu mỗi clip, + o_cuối cho clip được nội suy
    var = {}
    for c in clips:
        var[(c, 0)] = len(var)
        ramp = (not cfg_by_name[c].get('loop') and c not in no_return and len(frames_all[c]) > 1)
        var[(c, 1)] = len(var) if ramp else var[(c, 0)]
    n = len(var)
    rows, rhs = [], []
    for a, b in edges:
        dx, sc = _pair_shift(frames_all[a][-1], frames_all[b][0])
        w = max(sc, 0.0) ** 4
        row = np.zeros(n)
        if b in clips: row[var[(b, 0)]] += 1   # frame đầu của b
        if a in clips: row[var[(a, 1)]] -= 1   # frame cuối của a
        rows.append(row * w); rhs.append(-dx * w)  # dx + o_b,đầu - o_a,cuối -> 0
    for c in clips:
        row = np.zeros(n); row[var[(c, 0)]] = 0.05
        rows.append(row); rhs.append(0.0)
        if var[(c, 1)] != var[(c, 0)]:
            row = np.zeros(n); row[var[(c, 1)]] = 0.15; row[var[(c, 0)]] = -0.15
            rows.append(row); rhs.append(0.0)
    sol = np.linalg.lstsq(np.array(rows), np.array(rhs), rcond=None)[0]
    moved = []
    cap = RAMP_CAP_U * ppu
    for c in clips:
        o0, o1 = sol[var[(c, 0)]], sol[var[(c, 1)]]
        fs = frames_all[c]
        if len(fs) > 1 and abs(o1 - o0) > cap * (len(fs) - 1):  # phần trôi lớn hơn = chuyển động thật của đòn
            o1 = o0 + np.sign(o1 - o0) * cap * (len(fs) - 1)
        for i, f in enumerate(fs):
            t = i / (len(fs) - 1) if len(fs) > 1 else 0.0
            f['ax'] -= int(round(o0 + (o1 - o0) * t))  # nội dung dịch phải
        if round(o0) or round(o1):
            moved.append('%s%+d' % (c, round(o0)) + ('..%+d' % round(o1) if round(o1) != round(o0) else ''))
    print('  căn giữa clip (px):', ', '.join(moved) if moved else 'không cần')
    return ref, [c for c in clips if c not in leads and c not in no_return]


END_SHIFT_MIN_U = 0.15  # lệch nhỏ hơn thì để nguyên (không đáng dời vị trí nhân vật)


def _end_shifts(frames_all, ref, returning, ppu):
    """Clip kết thúc với thân lệch khỏi idle (đòn bước tới / lùi): khoảng lệch (đơn vị thế giới, + = phía trước mặt).
    EnemyAnimator dời nhân vật đúng khoảng này khi clip chạy hết => idle hiện ra đúng chỗ thân đang đứng."""
    out = {}
    for c in returning:
        dx, sc = _pair_shift(frames_all[ref][0], frames_all[c][-1])
        if sc >= 0.7 and abs(dx) / ppu >= END_SHIFT_MIN_U:
            out[c] = round(dx / ppu, 3)
    if out:
        print('  root shift cuối clip (u):', ', '.join('%s%+.2f' % kv for kv in out.items()))
    return out


def _align_to(f, ref, an):
    """Điểm neo của f sao cho profile cột phần lõi khớp nhất với frame ref (đã có neo)."""
    R = an.get('reg_range', 60)
    col, guess = _profile(f, an)
    pc, _ = _profile(ref, an)
    best = None
    L = 1200
    for dx in range(-R, R + 1):
        ax = guess + dx
        a1 = np.zeros(L); a2 = np.zeros(L)
        s1 = int(round(f['x0'] - ax)) + L // 2; s2 = int(round(ref['x0'] - ref['ax'])) + L // 2
        a1[s1:s1 + len(col)] = col; a2[s2:s2 + len(pc)] = pc
        sc = (a1 * a2).sum() / (np.sqrt((a1 ** 2).sum() * (a2 ** 2).sum()) + 1e-9)
        if best is None or sc > best[0]:
            best = (sc, ax)
    return best[1]


def _register(frames, an):
    """Neo ngang cho frame chưa có neo: căn khớp hình với frame đã có neo gần nhất phía trước
    (frame đầu chưa có neo thì lấy tâm bbox phần lõi). Mode register: mọi frame chưa có neo."""
    for i, f in enumerate(frames):
        if f['ax'] is not None:
            continue
        ref = next((frames[j] for j in range(i - 1, -1, -1) if frames[j]['ax'] is not None), None)
        if ref is None:
            ref = next((g for g in frames[i + 1:] if g['ax'] is not None), None)
        f['ax'] = _align_to(f, ref, an) if ref is not None else _profile(f, an)[1]


def _seed_frames(im, lab, an, inside):
    """Tách các frame dính nhau: loang đồng thời từ seed (tâm thân) bên trong mặt nạ alpha."""
    A = im[..., 3]
    ids = [c['id'] for c in inside]
    x0 = min(c['x0'] for c in inside); x1 = max(c['x1'] for c in inside)
    y0 = min(c['y0'] for c in inside); y1 = max(c['y1'] for c in inside)
    m = np.isin(lab[y0:y1, x0:x1], ids) & (A[y0:y1, x0:x1] > 0)
    seg = np.zeros(m.shape, np.int32)
    near_idx = ndi.distance_transform_edt(~m, return_distances=False, return_indices=True)
    for k, (sx, sy) in enumerate(an['seeds']):
        yy, xx = near_idx[0][sy - y0, sx - x0], near_idx[1][sy - y0, sx - x0]  # bám pixel có hình gần nhất
        seg[max(yy - 4, 0):yy + 5, max(xx - 4, 0):xx + 5] = k + 1
    seg[~m] = 0
    st = np.ones((3, 3), bool)
    while True:
        grown = ndi.grey_dilation(seg, footprint=st)
        new = (seg == 0) & m & (grown > 0)
        if not new.any():
            break
        seg[new] = grown[new]
    frames = []
    for k in range(len(an['seeds'])):
        fm = seg == k + 1
        ys, xs = np.nonzero(fm)
        fy0, fy1, fx0, fx1 = ys.min(), ys.max() + 1, xs.min(), xs.max() + 1
        rgba = im[y0 + fy0:y0 + fy1, x0 + fx0:x0 + fx1].copy()
        rgba[..., 3] = np.where(fm[fy0:fy1, fx0:fx1], rgba[..., 3], 0)
        frames.append(dict(rgba=rgba, x0=x0 + fx0, y0=y0 + fy0, ax=None, ground=y0 + fy1))
    if an.get('anchor') == 'head':
        for f in frames:
            f['ax'] = _head_x(f['rgba'], an, f['x0'])
    _register(frames, an)
    return frames


def _components(im, cfg):
    A = im[..., 3]
    vis = A > 25
    merge = cfg.get('merge', 3)
    lab, _ = ndi.label(ndi.binary_dilation(vis, iterations=merge), structure=np.ones((3, 3)))
    comps = []
    for i, sl in enumerate(ndi.find_objects(lab)):
        y0, y1, x0, x1 = sl[0].start, sl[0].stop, sl[1].start, sl[1].stop
        mask = (lab[y0:y1, x0:x1] == i + 1) & vis[y0:y1, x0:x1]
        if mask.sum() < 30 or (x1 - x0) < cfg.get('min_comp_w', 0):
            continue
        g_ = im[y0:y1, x0:x1, :3].mean(2)[mask]
        # chữ nhãn / số khung: thấp, đặc, gần như toàn pixel đen
        is_text = (y1 - y0) < cfg.get('text_max_h', 75) and mask.sum() > cfg.get('text_min_n', 250) and (g_ < 60).mean() > 0.8
        if cfg.get('text_filter') and is_text:
            continue
        comps.append(dict(id=i + 1, x0=x0, y0=y0, x1=x1, y1=y1, cx=(x0 + x1) / 2, cy=(y0 + y1) / 2, h=y1 - y0))
    return lab, comps


def _anim_frames(im, lab, comps, cfg, an):
    A = im[..., 3]
    vis = A > 25
    merge = cfg.get('merge', 3)
    min_h = an.get('min_body_h', cfg.get('min_body_h', 60))
    bx0, by0, bx1, by1 = an['band']
    inside = [c for c in comps if bx0 <= c['cx'] < bx1 and by0 <= c['cy'] < by1
              and not any(r[0] <= c['cx'] < r[2] and r[1] <= c['cy'] < r[3] for r in an.get('exclude', []))]
    if an.get('seeds'):
        return _seed_frames(im, lab, an, inside)
    bodies = sorted([c for c in inside if c['h'] >= min_h], key=lambda c: c['cx'])
    groups = [[b] for b in bodies]
    for e in (c for c in inside if c['h'] < min_h):
        if not bodies:
            break
        dist = [max(0, b['x0'] - e['x1'], e['x0'] - b['x1']) for b in bodies]
        k = int(np.argmin(dist))
        if dist[k] <= an.get('extra_reach', 40):
            groups[k].append(e)
    for (i, j) in an.get('merge_frames', []):  # vd vệt chém vẽ tách khỏi người
        groups[i] += groups[j]; groups[j] = []
    groups = [g for g in groups if g]
    groups = [g for k, g in enumerate(groups) if k not in an.get('drop', [])]
    frames = []
    mode = an.get('anchor', 'register')
    for g in groups:
        x0 = min(c['x0'] for c in g); x1 = max(c['x1'] for c in g)
        y0 = min(c['y0'] for c in g); y1 = max(c['y1'] for c in g)
        m = np.isin(lab[y0:y1, x0:x1], [c['id'] for c in g]) & vis[y0:y1, x0:x1]
        m = ndi.binary_dilation(m, iterations=merge) & (A[y0:y1, x0:x1] > 0)
        rgba = im[y0:y1, x0:x1].copy()
        rgba[..., 3] = np.where(m, rgba[..., 3], 0)
        ground = an.get('ground', y1)
        if 'ground' not in an and len(frames) in an.get('air', []):
            ground = None  # frame bay: điền sau = trung vị đáy các frame đứng
        if mode == 'head':
            ax = _head_x(rgba, an, x0)
        elif mode == 'hip':
            body_m = np.isin(lab[y0:y1, x0:x1], [g[0]['id']]) & (A[y0:y1, x0:x1] > 128)
            if not body_m.any():
                body_m = rgba[..., 3] > 20
            rows = np.nonzero(body_m.any(1))[0]
            t, b = rows[0], rows[-1]
            band = body_m[int(t + (b - t) * an.get('hip_lo', 0.45)):int(t + (b - t) * an.get('hip_hi', 0.65)) + 1]
            ax = x0 + np.nonzero(band)[1].mean()
        elif mode == 'bbox':
            ax = (x0 + x1) / 2
        elif mode == 'register':
            ax = None
        else:
            raise ValueError(mode)
        frames.append(dict(rgba=rgba, x0=x0, y0=y0, ax=ax, ground=ground))
    grounds = [f['ground'] for f in frames if f['ground'] is not None]
    for f in frames:
        if f['ground'] is None:
            f['ground'] = float(np.median(grounds))
    _register(frames, an)  # mode register, hoặc frame head không thấy mặt
    if an.get('flip'):  # lật ngang quanh điểm neo (AI vẽ đòn đánh về phía sau lưng)
        for f in frames:
            w = f['rgba'].shape[1]
            f['ax'] = f['x0'] + (w - (f['ax'] - f['x0']))
            f['rgba'] = f['rgba'][:, ::-1].copy()
    return frames


def slice_sheet(im, cfg, out_dir, preview=None):
    """im: RGBA đã tách nền. Ghi frame PNG vào out_dir, trả manifest (dict, dạng JsonUtility đọc được)."""
    lab, comps = _components(im, cfg)
    frames_all = {}
    for an in cfg['anims']:
        frames_all[an['name']] = _anim_frames(im, lab, comps, cfg, an)
        print('  %-12s %2d frame' % (an['name'], len(frames_all[an['name']])))
    end_shift = {}
    if cfg.get('align') is not False:
        res = _align_clips(frames_all, cfg['anims'], cfg.get('align') or {}, cfg['ppu'])
        if res:
            end_shift = _end_shifts(frames_all, res[0], res[1], cfg['ppu'])

    # ô chung cho cả nhân vật: neo ở giữa đáy
    allf = [f for fs in frames_all.values() for f in fs]
    pad = cfg.get('pad', 6)
    half = int(np.ceil(max(max(f['ax'] - f['x0'], f['x0'] + f['rgba'].shape[1] - f['ax']) for f in allf))) + pad
    up = max(f['ground'] - f['y0'] for f in allf)
    down = max(0, max(f['y0'] + f['rgba'].shape[0] - f['ground'] for f in allf))
    W = 2 * half
    H = int(np.ceil(up + down)) + 2 * pad
    H += H % 2
    base = pad + int(np.ceil(down))  # pixel từ đáy ô tới đường chân
    os.makedirs(out_dir, exist_ok=True)
    manifest = dict(id=cfg['id'], cellWidth=W, cellHeight=H, pivotX=0.5, pivotY=base / H,
                    pixelsPerUnit=cfg['ppu'], anims=[])
    strips = []
    for an in cfg['anims']:
        names, cells = [], []
        for k, f in enumerate(frames_all[an['name']]):
            cell = Image.new('RGBA', (W, H), (0, 0, 0, 0))
            px = int(round(half - (f['ax'] - f['x0'])))
            py = int(round(H - base - (f['ground'] - f['y0'])))
            cell.alpha_composite(Image.fromarray(f['rgba'], 'RGBA'), (px, py))
            fn = '%s_%s_%02d.png' % (cfg['id'], an['name'], k)
            cell.save(os.path.join(out_dir, fn))
            names.append(fn); cells.append(cell)
        manifest['anims'].append(dict(name=an['name'], fps=an.get('fps', 10), loop=an.get('loop', False), frames=names,
                                      endShift=end_shift.get(an['name'], 0.0)))
        strips.append((an['name'], cells))
    print('  cell %dx%d pivotY %.3f' % (W, H, base / H))
    if preview:
        _preview(strips, W, H, base, preview)
    return manifest


def _preview(strips, W, H, base, path, sc=0.5):
    """Mỗi anim 1 hàng + cột cuối là onion skin (chồng các frame) để soi giật."""
    cw, ch = int(W * sc), int(H * sc)
    maxn = max(len(c) for _, c in strips)
    sheet = Image.new('RGB', ((maxn + 1) * cw, len(strips) * ch), (60, 70, 90))
    d = ImageDraw.Draw(sheet)
    for r, (name, cells) in enumerate(strips):
        onion = Image.new('RGBA', (W, H), (0, 0, 0, 0))
        for k, c in enumerate(cells):
            small = c.resize((cw, ch))
            sheet.paste(small, (k * cw, r * ch), small)
            o = c.copy(); o.putalpha(o.getchannel('A').point(lambda v: v * 0.35))
            onion.alpha_composite(o)
        small = onion.resize((cw, ch))
        sheet.paste(small, (maxn * cw, r * ch), small)
        d.text((4, r * ch + 2), name, fill=(255, 255, 0))
        y = r * ch + ch - int(base * sc)
        d.line([(0, y), (sheet.width, y)], fill=(0, 255, 0))
        for k in range(maxn + 1):
            d.line([(k * cw + cw // 2, r * ch), (k * cw + cw // 2, r * ch + ch)], fill=(255, 80, 80))
    sheet.save(path)
