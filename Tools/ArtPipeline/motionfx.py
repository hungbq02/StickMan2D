"""Vẽ bằng code hiệu ứng chuyển động của player (vệt đòn, loé nòng, bụi) — cùng nét với hitfx.py.

    python -I -B Tools/ArtPipeline/motionfx.py [--preview out.png]

Ghi đè đúng tên file cũ (giữ GUID): Art/FX/Warm/{sword_slash1,2,3, sword_air, fist_swoosh, muzzle_flash}_NN.png (384x384, gốc giữa)
và Art/FX/Dust/{jump,land,run,dash,slide}_dust / dash_burst / wall_sparks (384x256). PPU 100.
Hướng tới trước = +x (HitFx lật theo hướng mặt). Bụi: gốc = chân (cách đáy 40px) -> pivot sprite đặt ở đó khi import.
"""
import math, os, sys
import numpy as np
from PIL import Image

sys.path.insert(0, os.path.dirname(os.path.abspath(__file__)))
from hitfx import Frame, crescent, streak, star, ease_out, ease_in, WHITE  # noqa: E402

ART = os.path.normpath(os.path.join(os.path.dirname(os.path.abspath(__file__)), '..', '..', 'Assets', '_GamePlay', 'Art', 'FX'))
DUST = (176, 183, 191)
DUST_HI = (214, 219, 224)
SMOKE = (150, 154, 162)
FOOT_Y = 40  # px từ đáy canvas bụi
DW, DH = 384, 256


def fade(t, start=0.55, keep=0.15):
    return 1.0 if t < start else 1 - (t - start) / (1 - start) * (1 - keep)


def dust_frame(k=2.0):
    return Frame(DW, DH, 0, -(DH / 2 - FOOT_Y), k)


def puff(f, x, y, r, outline=6):
    """Cụm bụi phẳng: đĩa xám + đốm sáng lệch trên-trái."""
    if r < 1:
        return
    f.disc(x, y, r, DUST, outline=int(min(outline, r * 4 * 0.3)))
    if r > 5:
        f.disc(x - r * 0.25, y + r * 0.3, r * 0.5, DUST_HI, outline=0)


# ------------------------------------------------------------------ vệt đòn (384x384)

def sword(a0, a1, r=125, sy=0.6, w=34, n=8, double=False, sparks=True):
    out = []
    for i in range(n):
        t = i / (n - 1)
        f = Frame(384, 384)
        head = a0 + (a1 - a0) * ease_out(0.2 + t / 0.45, 2)
        tail = a0 + (a1 - a0) * ease_in(max(0, (t - 0.05) / 0.95), 1.5)
        th = w * (1 - ease_in(t, 1.4)) + 2
        crescent(f, tail, head, r, th, WHITE, sy=sy)
        if double:  # vệt trong mảnh đi sau — nặng tay hơn
            crescent(f, tail, a0 + (a1 - a0) * ease_out(t / 0.5, 2), r * 0.72, th * 0.45, WHITE, sy=sy)
        if sparks:
            for k in range(3):
                tt = t - 0.1 * k
                if 0.15 < tt < 0.95:
                    a = math.radians(a1 + (8 + 7 * k) * (1 if a1 < a0 else -1))
                    d = r + 28 * ease_out((tt - 0.15) / 0.8)
                    f.disc(math.cos(a) * d, math.sin(a) * d * sy - 20 * tt * tt, 5 * (1 - tt), WHITE, outline=6)
        out.append(f.image(fade(t, 0.5)))
    return out


def sword_air(n=9):
    """Chém xoay trên không: vòng cung đuổi theo đầu lưỡi quay gần hết một vòng."""
    out = []
    for i in range(n):
        t = i / (n - 1)
        f = Frame(384, 384)
        head = 90 - 380 * ease_out(0.1 + t / 0.6, 2)
        tail = head + 40 + 220 * (1 - ease_in(t, 1.3)) * min(1, t / 0.25 + 0.3)
        crescent(f, head, tail, 110, 30 * (1 - ease_in(t, 1.4)) + 2, WHITE, sy=0.85)
        out.append(f.image(fade(t, 0.5)))
    return out


def fist_swoosh(n=6):
    """Đấm: ba vệt gió lao tới trước + cung nhỏ ở đầu nắm tay."""
    out = []
    for i in range(n):
        t = i / (n - 1)
        f = Frame(384, 384)
        f.k = 1.6
        for k, y in enumerate((-18, 2, 20)):
            head = 20 + 55 * ease_out(t + 0.15, 2) - 10 * abs(k - 1)
            L = 95 * (1 - ease_in(t, 1.2)) * (0.8 if k != 1 else 1)
            if L > 3:
                f.poly([(head - L, y), (head - L * 0.4, y + 4 * (1 - t) + 0.6), (head, y), (head - L * 0.4, y - 4 * (1 - t) - 0.6)], WHITE,
                       int(min(8, (8 * (1 - t) + 1.2) * 4 * 0.3)))
        crescent(f, 65, -65, 30 + 10 * ease_out(t), 14 * (1 - ease_in(t, 1.3)) + 1, WHITE, sy=1.0)
        out.append(f.image(fade(t, 0.45)))
    return out


def muzzle_flash(n=5):
    """Loé nòng: sao trắng dài về phía trước (gốc = đầu nòng), khói xám nhỏ trôi lên."""
    out = []
    for i in range(n):
        t = i / (n - 1)
        f = Frame(384, 384, 0, 0, 1.3)
        k = 1 - ease_in(t, 1.2)
        if k > 0.08:
            L, S, B = 95 * k * (1.1 - 0.3 * t), 30 * k, 16 * k
            f.poly([(0, 0), (L * 0.3, S * 0.35), (L * 0.35, S), (L * 0.55, S * 0.3), (L, 0), (L * 0.55, -S * 0.3), (L * 0.35, -S),
                    (L * 0.3, -S * 0.35), (0, 0), (-B, B * 0.5), (-B * 0.2, 0), (-B, -B * 0.5)], WHITE, 8)
        for j, (dx, dy, r) in enumerate(((20, 8, 12), (38, 16, 9), (8, 18, 8))):
            tt = t - 0.1 * j
            if tt > 0:
                rr = r * (0.6 + 0.8 * ease_out(tt)) * (1 - ease_in(tt, 2))
                if rr > 1:
                    f.disc(dx + 25 * tt, dy + 30 * tt, rr, SMOKE, outline=int(min(6, rr * 1.2)))
        out.append(f.image(fade(t, 0.5)))
    return out


# ------------------------------------------------------------------ bụi (384x256, gốc = chân)

def jump_dust(n=7):
    out = []
    for i in range(n):
        t = i / (n - 1)
        f = dust_frame()
        for s in (-1, 1):
            for k, (x0, x1, r) in enumerate(((10, 55, 15), (22, 90, 11))):
                e = ease_out(t, 2.5)
                rr = r * (0.5 + 0.9 * e) * (1 - ease_in(t, 1.6))
                puff(f, s * (x0 + (x1 - x0) * e), 4 + 10 * e - 2 * k, rr)
        for x in (-14, 0, 14):  # vệt đạp xuống
            if t < 0.5:
                L = 30 * (1 - t / 0.5)
                f.poly([(x - 3, 2), (x, 2 + L), (x + 3, 2)], DUST_HI, int(min(6, (1 - t / 0.5) * 6)))
        out.append(f.image(fade(t, 0.5)))
    return out


def land_dust(n=8):
    out = []
    for i in range(n):
        t = i / (n - 1)
        f = dust_frame()
        e = ease_out(t, 2.6)
        for s in (-1, 1):
            for k, (x1, r) in enumerate(((35, 15), (62, 12), (86, 9))):
                rr = r * (0.5 + 0.8 * e) * (1 - ease_in(t, 1.5))
                puff(f, s * (8 + (x1 - 8) * e), 3 + 6 * e * (1 - 0.3 * k), rr)
            for a in (25, 50):  # tia toé chéo lên
                ang = 90 - s * (90 - a)
                d = 15 + 55 * e
                L = 22 * (1 - ease_in(t, 1.2))
                streak(f, ang, d - L, d, 6 * (1 - t) + 1, DUST_HI, outline=6)
        out.append(f.image(fade(t, 0.5)))
    return out


def run_dust(n=6):
    out = []
    for i in range(n):
        t = i / (n - 1)
        f = dust_frame()
        e = ease_out(t, 2)
        puff(f, -12 - 40 * e, 3 + 16 * e, 10 * (0.6 + 0.7 * e) * (1 - ease_in(t, 1.8)))
        puff(f, -28 - 30 * e, 2 + 8 * e, 7 * (0.6 + 0.6 * e) * (1 - ease_in(t, 1.5)))
        out.append(f.image(fade(t, 0.45)))
    return out


def dash_burst(n=7):
    out = []
    for i in range(n):
        t = i / (n - 1)
        f = dust_frame(1.4)
        e = ease_out(t, 2.2)
        for k, y in enumerate((14, 26, 39, 50)):
            off = (0, 18, 6, 26)[k]
            tail = -20 - off - (75 + 15 * (k % 2)) * ease_out(min(1, t * 1.4 + 0.25), 2)
            head = -10 - off - 35 * ease_in(t, 1.3)
            if head - tail > 4:
                w = 7 * (1 - t) + 1
                f.poly([(tail, y), ((tail + head) / 2, y + w / 2), (head, y), ((tail + head) / 2, y - w / 2)], WHITE, int(min(8, w * 4 * 0.3)))
        for k in range(3):
            puff(f, -15 - (30 + 25 * k) * e, 3 + 8 * e, (13 - 3 * k) * (0.5 + 0.8 * e) * (1 - ease_in(t, 1.5)))
        out.append(f.image(fade(t, 0.5)))
    return out


def slide_dust(n=6):
    out = []
    for i in range(n):
        t = i / (n - 1)
        f = dust_frame()
        e = ease_out(t, 2)
        for k in range(4):
            tt = max(0, t - 0.06 * k)
            ee = ease_out(tt, 2)
            puff(f, -10 - (25 + 28 * k) * ee, 2 + (5 + 3 * k) * ee, (12 - 2 * k) * (0.5 + 0.7 * ee) * (1 - ease_in(t, 1.5)))
        for a in (150, 165):
            d = 20 + 70 * e
            streak(f, a, d - 24 * (1 - t), d, 5 * (1 - t) + 1, DUST_HI, outline=6)
        out.append(f.image(fade(t, 0.45)))
    return out


def wall_sparks(n=6):
    """Trượt tường: gốc = điểm chạm tường (tường ở +x) — tia toé ngược ra (-x) và lên, bụi nhỏ."""
    rng = np.random.default_rng(21)
    sp = [(rng.uniform(100, 170), rng.uniform(0.7, 1.2)) for _ in range(4)]
    out = []
    for i in range(n):
        t = i / (n - 1)
        f = Frame(DW, DH, 0, 0, 1.8)
        e = ease_out(t, 2.2)
        for a, s in sp:
            d = 8 + 60 * s * e
            L = 20 * (1 - ease_in(t, 1.2)) * s
            streak(f, a, d - L, d, 5 * (1 - t) + 1, WHITE, outline=6)
        puff(f, -8 - 14 * e, -4 + 10 * e, 8 * (0.6 + 0.6 * e) * (1 - ease_in(t, 1.6)))
        out.append(f.image(fade(t, 0.45)))
    return out


KINDS = {
    'Warm/sword_slash1': lambda: sword(150, -30),
    'Warm/sword_slash2': lambda: sword(-150, 30),
    'Warm/sword_slash3': lambda: sword(172, -8, r=140, sy=0.38, w=44, double=True),
    'Warm/sword_air': sword_air,
    'Warm/fist_swoosh': fist_swoosh,
    'Warm/muzzle_flash': muzzle_flash,
    'Dust/jump_dust': jump_dust,
    'Dust/land_dust': land_dust,
    'Dust/run_dust': run_dust,
    'Dust/dash_burst': dash_burst,
    'Dust/slide_dust': slide_dust,
    'Dust/wall_sparks': wall_sparks,
}


def main():
    prev = sys.argv[sys.argv.index('--preview') + 1] if '--preview' in sys.argv else None
    rows = {}
    for k, fn in KINDS.items():
        frames = fn()
        rows[k] = frames
        d, name = os.path.split(os.path.join(ART, k))
        old = [f for f in os.listdir(d) if f.startswith(name + '_') and f[len(name) + 1:-4].isdigit() and f.endswith('.png')]
        for i, im in enumerate(frames):
            im.save(os.path.join(d, f'{name}_{i:02d}.png'))
        for f in old:
            if int(f[len(name) + 1:-4]) >= len(frames):
                os.remove(os.path.join(d, f))
                if os.path.exists(os.path.join(d, f + '.meta')):
                    os.remove(os.path.join(d, f + '.meta'))
        print(k, len(frames))
    if prev:
        W = 110
        n = max(len(v) for v in rows.values())
        sheet = Image.new('RGBA', (W * n, W * len(rows)), (0, 0, 0, 255))
        for r, (k, frames) in enumerate(rows.items()):
            for c, im in enumerate(frames):
                cell = Image.new('RGBA', (W, W), (58, 63, 74, 255) if r % 2 == 0 else (120, 128, 140, 255))
                th = im.copy(); th.thumbnail((W, W))
                cell.alpha_composite(th, ((W - th.width) // 2, (W - th.height) // 2))
                sheet.paste(cell, (c * W, r * W))
        sheet.save(prev)


if __name__ == '__main__':
    main()
