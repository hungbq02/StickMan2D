"""Đưa frame do sprite-gen sinh (run dir GPT image rows) vào Assets/_GamePlay/Art/Enemies/<Name>/.

    python -I -B Tools/ArtPipeline/from_run.py <Name> <run-dir> --height <u> [--chains tellA:a,tellB:b] [--preview DIR] [--out DIR] [--scale state=s,...] [--upscale N]

Mỗi ô của sprite-gen = 1 frame (đã tách nền, chân trên đáy ô). Neo ngang: register (khớp hình frame trước),
căn chỗ nối giữa các clip + root shift cuối đòn dùng chung slice.compose_cells (như build.py).
--height: chiều cao thế giới (u) của thân ở idle frame 0 -> pixelsPerUnit.
Giữ thư mục FX/ và danh sách fx trong manifest cũ (hiệu ứng rời không sinh lại ở đây).
--upscale N: sau cùng phóng frame ×N bằng Real-ESRGAN (upscale.py, PPU × N) — boss vẽ to (Demon / Unmaker dùng 2).
"""
import json, os, sys
import numpy as np
from PIL import Image

HERE = os.path.dirname(os.path.abspath(__file__))
sys.path.insert(0, HERE)
from slice import compose_cells, _register  # noqa: E402

ROOT = os.path.normpath(os.path.join(HERE, '..', '..'))
OUT_DIR = os.path.join(ROOT, 'Assets', '_GamePlay', 'Art', 'Enemies')


def load_run(run_dir):
    req = json.load(open(os.path.join(run_dir, 'sprite-request.json'), encoding='utf-8'))
    fm = json.load(open(os.path.join(run_dir, 'frames', 'frames-manifest.json'), encoding='utf-8'))
    rows = {r['state']: r for r in fm['rows']} if isinstance(fm.get('rows'), list) else fm['rows']
    # sprite-gen đặt chân trên đường cách đáy ô safe_margin_y px (không phải đáy ô)
    return req, rows, int((fm.get('cell') or {}).get('safe_margin_y', 0))


def frames_of(run_dir, row, margin=0):
    out = []
    for rel in row['files']:
        p = rel if os.path.isabs(rel) else os.path.join(run_dir, rel)
        if not os.path.exists(p):
            p = os.path.join(run_dir, 'frames', rel)
        im = np.asarray(Image.open(p).convert('RGBA'))
        ys, xs = np.nonzero(im[..., 3] > 8)
        if len(ys) == 0:  # frame trống (vd teleport biến mất): giữ 1 pixel trong suốt ở giữa đáy
            h, w = im.shape[:2]
            out.append(dict(rgba=np.zeros((1, 1, 4), np.uint8), x0=w // 2, y0=h - 1, ax=w / 2, ground=h - margin))
            continue
        y0, y1, x0, x1 = ys.min(), ys.max() + 1, xs.min(), xs.max() + 1
        # ground = đường chân chung của run (đáy ô - lề an toàn) -> giữ nguyên chuyển động dọc trong clip
        out.append(dict(rgba=im[y0:y1, x0:x1].copy(), x0=int(x0), y0=int(y0), ax=None, ground=im.shape[0] - margin))
    return out


def _mask(f, k=4):
    """Mặt nạ thu nhỏ 1/k của frame, đáy = đường chân (ground)."""
    a = f['rgba'][..., 3] > 128
    h = int(f['ground'] - f['y0'])
    m = np.zeros((max(h, a.shape[0]), a.shape[1]), bool)
    m[:a.shape[0]] = a
    im = Image.fromarray((m * 255).astype(np.uint8)).resize((max(1, m.shape[1] // k), max(1, m.shape[0] // k)))
    return np.asarray(im) > 127


def _iou_at(ref, m, s):
    """IoU tốt nhất (dò lệch ngang) khi phóng m theo hệ số s, hai mặt nạ cùng đáy."""
    h, w = max(1, int(round(m.shape[0] * s))), max(1, int(round(m.shape[1] * s)))
    ms = np.asarray(Image.fromarray((m * 255).astype(np.uint8)).resize((w, h))) > 127
    H = max(ref.shape[0], h); W = ref.shape[1] + 2 * w
    A = np.zeros((H, W), bool); A[H - ref.shape[0]:, w:w + ref.shape[1]] = ref
    best = 0.0
    for x in range(0, W - w + 1, 2):
        B = np.zeros((H, W), bool); B[H - h:, x:x + w] = ms
        inter = (A & B).sum()
        if inter:
            best = max(best, inter / (A | B).sum())
    return best


def row_scale(idle0, frames):
    """Hệ số đưa row về cùng cỡ idle: khớp hình frame đầu / cuối (đòn bắt đầu hoặc kết thúc ở idle) với idle."""
    ref = _mask(idle0)
    best = (0.0, 1.0)
    for f in (frames[0], frames[-1]):
        m = _mask(f)
        for s in np.arange(0.7, 1.45, 0.025):
            sc = _iou_at(ref, m, s)
            if sc > best[0]:
                best = (sc, float(s))
    return best[1], best[0]


def rescale(frames, s, cell_w):
    for f in frames:
        h, w = f['rgba'].shape[:2]
        nh, nw = max(1, int(round(h * s))), max(1, int(round(w * s)))
        f['rgba'] = np.asarray(Image.fromarray(f['rgba'], 'RGBA').resize((nw, nh), Image.LANCZOS)).copy()
        cx = cell_w / 2
        f['x0'] = int(round(cx - (cx - f['x0']) * s))
        f['y0'] = int(round(f['ground'] - (f['ground'] - f['y0']) * s))


def main():
    args = sys.argv[1:]
    opt = {}
    for k in ('--height', '--chains', '--preview', '--id', '--out', '--scale', '--upscale'):
        if k in args:
            i = args.index(k); opt[k] = args[i + 1]; del args[i:i + 2]
    name, run_dir = args
    cid = opt.get('--id', name.lower())
    req, rows, margin = load_run(run_dir)
    anims = []
    frames_all = {}
    for state, st in req['states'].items():
        row = rows.get(state)
        if not row or not row.get('ok', True) or not row.get('files'):
            print('  BỎ QUA %s: chưa extract được' % state)
            continue
        an = dict(name=state, fps=st['fps'], loop=st['loop'])
        frames_all[state] = frames_of(run_dir, row, margin)
        anims.append(an)
    # sprite-gen fit từng row vào ô riêng (tư thế vươn rộng bị thu nhỏ) -> đưa mọi row về cùng cỡ idle
    forced = dict(kv.split('=') for kv in opt['--scale'].split(',')) if opt.get('--scale') else {}
    idle0 = frames_all['idle'][0]
    for an in anims:
        state, fr = an['name'], frames_all[an['name']]
        if state in forced:
            s, sc = float(forced[state]), None
        elif state == 'idle':
            s, sc = 1.0, 1.0
        else:
            s, sc = row_scale(idle0, fr)
        if abs(s - 1) >= 0.04:
            rescale(fr, s, req['cell']['width'])
        else:
            s = 1.0
        _register(fr, an)
        print('  %-12s %2d frame @%d%s  scale %.3f%s' % (state, len(fr), an['fps'], ' loop' if an['loop'] else '',
                                                       s, ' (đặt tay)' if sc is None else ' (khớp %.2f)' % sc))
    idle = frames_all['idle'][0]['rgba'][..., 3] > 20
    body_h = int(np.ptp(np.nonzero(idle.any(1))[0])) + 1
    ppu = max(1, int(round(body_h / float(opt['--height']))))
    chains = [c.split(':') for c in opt['--chains'].split(',')] if opt.get('--chains') else []
    cfg = dict(id=cid, ppu=ppu, anims=anims, align=dict(chains=chains, no_return=['death', 'teleport', 'stealthIn']))
    out = os.path.join(opt.get('--out', OUT_DIR), name)
    old_fx = []
    mpath = os.path.join(out, '%s_manifest.json' % cid)
    if os.path.exists(mpath):
        old_fx = json.load(open(mpath, encoding='utf-8')).get('fx', [])
    if os.path.isdir(out):  # xoá frame cũ ở thư mục gốc (giữ FX/ và .meta trùng tên)
        for f in os.listdir(out):
            if f.endswith('.png') or f.endswith('_manifest.json'):
                os.remove(os.path.join(out, f))
    preview = os.path.join(opt['--preview'], name + '.png') if opt.get('--preview') else None
    if preview:
        os.makedirs(opt['--preview'], exist_ok=True)
    manifest = compose_cells(frames_all, cfg, out, preview)
    manifest['fx'] = old_fx
    with open(mpath, 'w', encoding='utf-8') as f:
        json.dump(manifest, f, indent=1)
    for f in os.listdir(out):  # .meta mồ côi
        if f.endswith('.png.meta') and not os.path.exists(os.path.join(out, f[:-5])):
            os.remove(os.path.join(out, f))
    print('  thân idle %d px / %.2f u -> PPU %d' % (body_h, float(opt['--height']), ppu))
    if opt.get('--upscale'):
        import upscale
        upscale.run(out, int(opt['--upscale']))


if __name__ == '__main__':
    main()
