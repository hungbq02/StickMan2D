"""Cắt sprite hiệu ứng / đạn vẽ đứng riêng trong sheet (mũi tên bay, tia beam, cột lửa...).

items: [{name, rects:[[x0,y0,x1,y1], ...], align: center|bottom, fps, loop}]
Mỗi item = 1 chuỗi frame (1 rect = 1 frame) cùng kích thước ô; căn giữa (đạn) hoặc đáy-giữa (cột lửa).
"""
import os, numpy as np
from PIL import Image
from scipy import ndimage as ndi


def _clean(c, floor=8, min_mass=40):
    a = np.where(c[..., 3] > floor, c[..., 3], 0)
    lab, n = ndi.label(a > 0, structure=np.ones((3, 3)))
    if n:
        mass = ndi.sum(a, lab, range(1, n + 1)) / 255.0
        a[np.isin(lab, np.nonzero(mass < min_mass)[0] + 1)] = 0  # bỏ chấm nhiễu lẻ
    c = c.copy(); c[..., 3] = a
    ys, xs = np.nonzero(a)
    return c[ys.min():ys.max() + 1, xs.min():xs.max() + 1]


def cut_fx(im, items, out_dir, ppu):
    os.makedirs(out_dir, exist_ok=True)
    out = []
    for it in items:
        crops = [_clean(im[y0:y1, x0:x1], min_mass=it.get('min_mass', 40)) for (x0, y0, x1, y1) in it['rects']]
        pad = 2
        W = max(c.shape[1] for c in crops) + 2 * pad
        H = max(c.shape[0] for c in crops) + 2 * pad
        W += W % 2; H += H % 2
        bottom = it.get('align', 'center') == 'bottom'
        names = []
        for k, c in enumerate(crops):
            cell = Image.new('RGBA', (W, H), (0, 0, 0, 0))
            py = H - pad - c.shape[0] if bottom else (H - c.shape[0]) // 2
            cell.alpha_composite(Image.fromarray(c, 'RGBA'), ((W - c.shape[1]) // 2, py))
            fn = '%s_%02d.png' % (it['name'], k)
            cell.save(os.path.join(out_dir, fn))
            names.append(fn)
        out.append(dict(name=it['name'], fps=it.get('fps', 12), loop=it.get('loop', False), frames=names,
                        pivotX=0.5, pivotY=pad / H if bottom else 0.5, pixelsPerUnit=ppu))
        print('  fx %-20s %d frame, cell %dx%d' % (it['name'], len(names), W, H))
    return out
