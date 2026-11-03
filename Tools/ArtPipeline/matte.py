"""Tách nền ca rô "vẽ chết" (alpha = 255) / nền phẳng khỏi sprite sheet AI-gen -> RGBA sạch.

B1: vùng nền chắc chắn = pixel ít bão hoà, đúng dải tông nền, nối với vùng nền lớn
    (+ túi nhỏ giữa chân nếu đúng tông). Mảng 1 tông rộng hơn 1 ô mà viền là nét tối
    (vd mặt nhân vật xám đúng tông ô sáng) bị ép là foreground. Đường kẻ dài (vạch đất,
    khung ô) coi là nền.
B2: dựng ảnh nền B(x,y) dưới nhân vật: lấy pixel nền thật lệch đúng bội số chu kỳ ô
    (ưu tiên lệch ngang cùng hàng ô), nền phẳng thì lấy trung vị.
B3: alpha = color-to-alpha(C, B), khử màu nền ở pixel bán trong suốt; vùng đặc lấp lỗ
    -> alpha 1; glow mềm làm mượt tần số thấp để xoá vân ô còn sót.
"""
import sys, json, numpy as np
from PIL import Image
from scipy import ndimage as ndi


def _axis_fit(cls, bgmask, period_guess, axis):
    """Vị trí chuyển tông dọc 1 trục -> (period, phase) có độ tập trung modulo cao nhất."""
    c = cls if axis == 1 else cls.T
    m = bgmask if axis == 1 else bgmask.T
    edge = (c[:, 1:] != c[:, :-1]) & m[:, 1:] & m[:, :-1]
    pos = np.nonzero(edge)[1].astype(np.float64) + 0.5
    best = None
    for p in np.arange(period_guess - 1.5, period_guess + 1.5, 0.01):
        ang = 2 * np.pi * pos / p
        z = np.exp(1j * ang).mean()
        if best is None or abs(z) > best[0]:
            best = (abs(z), p, (np.angle(z) % (2 * np.pi)) / (2 * np.pi) * p)
    return best


def matte(rgb, cfg):
    """rgb: mảng HxWx3 -> mảng RGBA uint8."""
    im = np.asarray(rgb)[..., :3].astype(np.float32)
    h, w, _ = im.shape
    gray = im.mean(2)
    sat = im.max(2) - im.min(2)
    lo, hi = cfg['tones']
    tol = cfg.get('tone_tol', 14)
    cand = (sat < cfg.get('sat_max', 14)) & (gray > lo - tol) & (gray < hi + tol)
    for (a, b) in cfg.get('extra_bg_ranges', []):  # vd viền trắng "sticker"
        cand |= (sat < cfg.get('sat_max', 14)) & (gray >= a) & (gray <= b)
    # đường kẻ dài (vạch đất, khung ô) -> coi là nền; tính trước để không làm nhiễu phép xét viền
    lines_all = np.zeros((h, w), bool)
    for (kind, lo_len) in cfg.get('kill_lines', []):
        dark = gray < cfg.get('line_dark', 90)
        if kind == 'h':
            run = ndi.uniform_filter1d(dark.astype(np.float32), lo_len, axis=1) > 0.97
            line = ndi.binary_dilation(run, structure=np.ones((1, lo_len))) & dark
        else:
            run = ndi.uniform_filter1d(dark.astype(np.float32), lo_len, axis=0) > 0.97
            line = ndi.binary_dilation(run, structure=np.ones((lo_len, 1))) & dark
        line = ndi.binary_dilation(line, iterations=cfg.get('line_grow', 1))
        lines_all |= line
    # khung ô thẳng hàng: cột/hàng nào có nhiều pixel đường kẻ -> xoá mọi pixel tối ít bão hoà dọc cột/hàng đó
    if cfg.get('grid_lines'):
        gl = cfg['grid_lines']
        weak = (gray < gl.get('dark', 125)) & (sat < gl.get('sat', 30))
        for axis in (0, 1):
            score = lines_all.sum(axis)
            idx = np.nonzero(score > gl.get('min_count', 300))[0]
            for i in idx:
                for d_ in range(-gl.get('grow', 2), gl.get('grow', 2) + 1):
                    j = min(max(i + d_, 0), (w if axis == 0 else h) - 1)
                    if axis == 0:
                        lines_all[:, j] |= weak[:, j]
                    else:
                        lines_all[j, :] |= weak[j, :]
    cand |= lines_all
    # mảng 1 tông rộng hơn 1 ô ca rô mà viền quanh là nét đen (không phải ô tông kia)
    # -> là foreground (vd mặt nhân vật xám đúng tông ô sáng)
    forced_fg = np.zeros((h, w), bool)
    if not cfg.get('flat'):
        p2 = cfg['period'] ** 2
        lowsat = sat < cfg.get('sat_max', 14)
        tt = cfg.get('blob_tone_tol', 11)
        four = ndi.generate_binary_structure(2, 1)
        near_t = {t: lowsat & (np.abs(gray - t) < tt) for t in (lo, hi)}
        for tone, other in ((lo, hi), (hi, lo)):
            m = near_t[tone]
            l2, n2 = ndi.label(m, structure=four)
            if not n2:
                continue
            sz = ndi.sum(m, l2, range(1, n2 + 1))
            objs = ndi.find_objects(l2)
            for i in np.nonzero(sz > cfg.get('big_tone_k', 1.6) * p2)[0]:
                sl = objs[i]
                y0, y1 = max(0, sl[0].start - 4), min(h, sl[0].stop + 4)
                x0, x1 = max(0, sl[1].start - 4), min(w, sl[1].stop + 4)
                blob = l2[y0:y1, x0:x1] == i + 1
                ring = ndi.binary_dilation(blob, iterations=3) & ~ndi.binary_dilation(blob, iterations=1)
                ring &= ~ndi.binary_dilation(lines_all[y0:y1, x0:x1], iterations=2)
                other_frac = near_t[other][y0:y1, x0:x1][ring].mean() if ring.any() else 1.0
                dark_frac = (gray[y0:y1, x0:x1][ring] < cfg.get('ring_dark', 90)).mean() if ring.any() else 0
                if other_frac < cfg.get('fg_ring_other', 0.25) and dark_frac > cfg.get('fg_ring_dark', 0.4):
                    filled_blob = ndi.binary_fill_holes(blob)
                    forced_fg[y0:y1, x0:x1] |= ndi.binary_dilation(filled_blob, iterations=1)
        cand &= ~forced_fg
    # vùng nền = thành phần liên thông lớn + "túi" nhỏ (khe giữa chân...) nếu đúng tông nền
    lab, n = ndi.label(cand)
    idxs = np.arange(1, n + 1)
    sizes = ndi.sum(cand, lab, idxs)
    near = (np.minimum(np.abs(gray - lo), np.abs(gray - hi)) < cfg.get('pocket_tone_tol', 7)) | lines_all
    near_frac = ndi.mean(near, lab, idxs)
    keep = np.zeros(n + 1, bool)
    keep[1:] = (sizes > cfg.get('min_bg_comp', 3000)) | (
        (sizes > cfg.get('min_pocket', 60)) & (near_frac > cfg.get('pocket_near', 0.75)))
    bg = keep[lab]
    bg |= lines_all
    for (x0, y0, x1, y1) in cfg.get('erase_rects', []):  # nhãn chữ, số khung...
        bg[y0:y1, x0:x1] = True
    hard_bg = bg.copy()

    # dựng ảnh nền B
    if cfg.get('flat'):
        B = np.empty_like(im)
        B[:] = np.median(im[cand & bg], axis=0)
    else:
        cls = gray > (lo + hi) / 2
        truebg = bg & cand & ~lines_all
        px = _axis_fit(cls, truebg, cfg['period'], 1)[1]
        py = _axis_fit(cls, truebg, cfg['period'], 0)[1]
        B = np.zeros_like(im)
        known = truebg.copy()
        B[known] = im[known]
        need = ~known
        R = cfg.get('shift_r', 6)
        # ưu tiên lệch ngang cùng hàng ô (hàng ô AI-gen cao thấp không đều, cột ô đều hơn)
        RH = cfg.get('shift_rh', 14)
        shifts = sorted([(a, 0) for a in range(-RH, RH + 1) if a % 2 == 0 and a], key=lambda t: abs(t[0]))
        rest = [(a, b) for a in range(-R, R + 1) for b in range(-R, R + 1) if (a + b) % 2 == 0 and b]
        rest.sort(key=lambda t: t[0] ** 2 + 2 * t[1] ** 2)
        shifts += rest
        for a, b in shifts:
            dx, dy = int(round(a * px)), int(round(b * py))
            ys = slice(max(0, -dy), min(h, h - dy)); yd = slice(max(0, dy), min(h, h + dy))
            xs = slice(max(0, -dx), min(w, w - dx)); xd = slice(max(0, dx), min(w, w + dx))
            take = need[ys, xs] & truebg[yd, xd]
            if take.any():
                tgt = B[ys, xs]
                tgt[take] = im[yd, xd][take]
                need[ys, xs] &= ~take
            if not need.any():
                break
        if need.any():
            idx = ndi.distance_transform_edt(~truebg, return_distances=False, return_indices=True)
            B[need] = im[idx[0], idx[1]][need]
    # color-to-alpha so với B
    C = im
    up = np.where(C > B, (C - B) / np.maximum(255 - B, 1), 0)
    dn = np.where(C < B, (B - C) / np.maximum(B, 1), 0)
    alpha = np.clip(np.maximum(up, dn).max(2), 0, 1)
    alpha[hard_bg] = 0
    a0 = cfg.get('alpha_floor', 0.06)
    alpha = np.clip((alpha - a0) / (1 - a0), 0, 1)

    # vùng đặc: alpha cao -> lấp lỗ (lỗ nhỏ hơn max_hole) -> alpha = 1
    solid = alpha > cfg.get('solid_t', 0.55)
    filled = ndi.binary_fill_holes(solid)
    holes = filled & ~solid & ~hard_bg
    hl, hn = ndi.label(holes)
    if hn:
        hs = ndi.sum(holes, hl, range(1, hn + 1))
        big = np.zeros(hn + 1, bool); big[1:] = hs > cfg.get('max_hole', 4000)
        holes &= ~big[hl]
    core = ndi.binary_erosion(solid, iterations=1) | holes | forced_fg
    alpha_out = np.where(core, 1.0, alpha)
    # màu: khử màu nền ở pixel bán trong suốt
    a3 = np.maximum(alpha_out, 1e-3)[..., None]
    F = np.where(core[..., None], C, (C - (1 - a3) * B) / a3)
    F = np.clip(F, 0, 255)

    # vùng glow mềm (xa thân > 2px, alpha thấp): làm mượt tần số thấp để xoá vân ô còn sót
    gs = cfg.get('glow_sigma', 0)
    if gs:
        body = ndi.binary_dilation(alpha_out > cfg.get('glow_body_t', 0.9), iterations=2)
        soft = (alpha_out > 0) & ~body
        wgt = soft.astype(np.float32)
        P = F * alpha_out[..., None] * wgt[..., None]
        Ab = ndi.gaussian_filter(alpha_out * wgt, gs)
        Pb = ndi.gaussian_filter(P, (gs, gs, 0))
        Wb = np.maximum(ndi.gaussian_filter(wgt, gs), 1e-4)
        a_s = Ab / Wb
        F_s = Pb / np.maximum(Ab, 1e-4)[..., None]
        alpha_out = np.where(soft, a_s, alpha_out)
        F = np.where(soft[..., None], np.clip(F_s, 0, 255), F)

    # bỏ hạt nhỏ
    vis = alpha_out > 0.05
    lab, n = ndi.label(vis, structure=np.ones((3, 3)))
    if n:
        s = ndi.sum(alpha_out, lab, range(1, n + 1))
        tiny = np.zeros(n + 1, bool); tiny[1:] = s < cfg.get('min_speck', 12)
        alpha_out[tiny[lab]] = 0
    return np.dstack([F, alpha_out * 255]).astype(np.uint8)


if __name__ == '__main__':
    # python matte.py src.png dst.png '{"tones":[178,215],"period":23.4}'
    src, dst, cfgjson = sys.argv[1], sys.argv[2], sys.argv[3]
    Image.fromarray(matte(Image.open(src).convert('RGB'), json.loads(cfgjson)), 'RGBA').save(dst)
