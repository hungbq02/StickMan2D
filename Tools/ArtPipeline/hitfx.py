"""Vẽ bằng code hiệu ứng trúng đòn (ART_STYLE: nét phẳng, viền mực, player = trắng, enemy = đỏ cam).

    python -I -B Tools/ArtPipeline/hitfx.py [--preview out.png]

Mỗi loại = N frame liền mạch (cùng một hình biến đổi theo thời gian, có easing) -> mượt hơn các hình rời nhau.
Ghi đè / tạo Assets/_GamePlay/Art/FX/Hit/<kind>_NN.png, canvas 320px (PPU 200 = 1.6u), tâm va chạm ở giữa,
hướng đòn bay tới = +x (HitFx lật theo hướng đòn).
"""
import math, os, sys
import numpy as np
from PIL import Image, ImageDraw

HERE = os.path.dirname(os.path.abspath(__file__))
OUT = os.path.normpath(os.path.join(HERE, '..', '..', 'Assets', '_GamePlay', 'Art', 'FX', 'Hit'))
SIZE, SS = 320, 4
INK = (27, 27, 32)
WHITE = (250, 248, 242)
BONE = (232, 228, 218)
ACC = (229, 72, 43)
ACC_LIGHT = (255, 170, 120)


def ease_out(t, p=3):
    return 1 - (1 - min(max(t, 0), 1)) ** p


def ease_in(t, p=2):
    return min(max(t, 0), 1) ** p


class Frame:
    """Hai lớp: viền mực (vẽ dày hơn) + lớp màu; ghép lại rồi thu nhỏ 4x để khử răng cưa."""

    def __init__(self, w=SIZE, h=SIZE, ox=0.0, oy=0.0, k=1.0):
        """Canvas w x h px; gốc toạ độ lệch (ox, oy) px so với tâm canvas (y lên trên)."""
        self.w, self.h, self.k = w, h, k  # k: phóng mọi toạ độ / bán kính (viền giữ nguyên px)
        self.cx, self.cy = (w / 2 + ox) * SS, (h / 2 - oy) * SS
        self.ink = Image.new('L', (w * SS, h * SS), 0)
        self.col = Image.new('RGBA', (w * SS, h * SS), (0, 0, 0, 0))
        self.di, self.dc = ImageDraw.Draw(self.ink), ImageDraw.Draw(self.col)

    def poly(self, pts, color, outline=10):
        pts = [(self.cx + x * self.k * SS, self.cy - y * self.k * SS) for x, y in pts]  # toạ độ px canvas gốc, y lên trên
        if outline >= 1:
            self.di.polygon(pts, fill=255)
            self.di.line(pts + [pts[0]], fill=255, width=outline * 2, joint='curve')
        self.dc.polygon(pts, fill=color + (255,))

    def ring(self, r, w, color, sx=1.0, sy=1.0, outline=8):
        r, w = r * self.k, w * self.k
        if r <= 0 or w <= 0.3:
            return
        box = lambda rr: (self.cx - rr * sx * SS, self.cy - rr * sy * SS, self.cx + rr * sx * SS, self.cy + rr * sy * SS)
        o = min(outline / SS, w * 0.3)
        if outline and o > 0.2:
            self.di.ellipse(box(r + w / 2 + o), outline=255, width=int((w + 2 * o) * SS))
        self.dc.ellipse(box(r + w / 2), outline=color + (255,), width=max(1, int(w * SS)))

    def disc(self, x, y, r, color, outline=8):
        x, y, r = x * self.k, y * self.k, r * self.k
        if r <= 0.2:
            return
        if outline:
            o = outline / SS
            self.di.ellipse((self.cx + (x - r - o) * SS, self.cy - (y + r + o) * SS, self.cx + (x + r + o) * SS, self.cy - (y - r - o) * SS), fill=255)
        self.dc.ellipse((self.cx + (x - r) * SS, self.cy - (y + r) * SS, self.cx + (x + r) * SS, self.cy - (y - r) * SS), fill=color + (255,))

    def image(self, alpha=1.0):
        ink = Image.new('RGBA', self.ink.size, INK + (0,))
        ink.putalpha(self.ink)
        ink.alpha_composite(self.col)
        im = ink.resize((self.w, self.h), Image.LANCZOS)
        if alpha < 1:
            a = np.array(im)
            a[..., 3] = (a[..., 3] * alpha).astype(np.uint8)
            im = Image.fromarray(a)
        return im


def streak(f, ang, r0, r1, w, color, outline=8):
    """Vệt nhọn hai đầu dọc theo tia góc ang (độ), từ bán kính r0 tới r1, dày nhất w."""
    if r1 - r0 < 1 or w < 0.4:
        return
    outline = int(min(outline, w * SS * 0.3))
    a = math.radians(ang)
    ux, uy, nx, ny = math.cos(a), math.sin(a), -math.sin(a), math.cos(a)
    m = r0 + (r1 - r0) * 0.35
    pts = [(ux * r0, uy * r0), (ux * m + nx * w / 2, uy * m + ny * w / 2), (ux * r1, uy * r1), (ux * m - nx * w / 2, uy * m - ny * w / 2)]
    f.poly(pts, color, outline)


def star(f, n, r_out, r_in, color, rot=0.0, outline=10, jitter=None):
    pts = []
    for i in range(n * 2):
        r = r_out if i % 2 == 0 else r_in
        if jitter is not None and i % 2 == 0:
            r *= jitter[i // 2]
        a = math.radians(rot + 180.0 * i / n)
        pts.append((math.cos(a) * r, math.sin(a) * r))
    f.poly(pts, color, outline)


def crescent(f, a0, a1, r, w, color, sy=1.0, outline=10, steps=48):
    """Lưỡi liềm từ góc a0 tới a1 (độ), dày w ở giữa, nhọn hai đầu; sy < 1 = dẹt (nhát chém nghiêng)."""
    if abs(a1 - a0) < 2 or w < 0.5:
        return
    outline = int(min(outline, w * SS * 0.3))
    outer, inner = [], []
    for i in range(steps + 1):
        t = i / steps
        a = math.radians(a0 + (a1 - a0) * t)
        th = w * math.sin(math.pi * t) ** 0.8
        outer.append((math.cos(a) * (r + th / 2), math.sin(a) * (r + th / 2) * sy))
        inner.append((math.cos(a) * (r - th / 2), math.sin(a) * (r - th / 2) * sy))
    f.poly(outer + inner[::-1], color, outline)


# ---------------------------------------------------------------- các loại

def fist(n=10):
    """Fighter: cú đấm — bùng trắng răng cưa nén theo hướng đấm, vòng chấn động dày nở ra, vệt tốc độ bắn tung."""
    rng = np.random.default_rng(3)
    angs = [-70, -40, -12, 14, 42, 72, 150, 205]
    jit = list(rng.uniform(0.75, 1.15, 9))
    out = []
    for i in range(n):
        t = i / (n - 1)
        f = Frame()
        e = ease_out(t)
        f.ring(26 + 80 * e, 26 * (1 - t) ** 1.6, WHITE, sx=0.72, sy=1.0)
        for k, a in enumerate(angs):
            sp = 0.8 + 0.4 * ((k * 37) % 10) / 10
            head = 40 + 100 * ease_out(t, 2.2) * sp
            length = 52 * (1 - ease_in(t, 1.2)) * sp
            streak(f, a, head - length, head, 14 * (1 - t) ** 1.2 + 0.5, WHITE)
        if t < 0.5:  # lõi bùng: to nhanh, co lại
            core = 78 * math.sin(math.pi * min(1, 0.35 + t / 0.5 * 0.65))
            star(f, 9, core, core * 0.55, WHITE, rot=10 + 30 * t, jitter=jit)
        out.append(f.image(1.0 if t < 0.55 else 1 - (t - 0.55) / 0.45 * 0.85))
    return out


def slash(n=10):
    """Sword: nhát chém lưỡi liềm quét qua (đầu đi trước, đuôi đuổi theo), đường cắt sáng ngang thân, tia nhỏ văng theo."""
    a_start, a_end = 145, -35
    out = []
    for i in range(n):
        t = i / (n - 1)
        f = Frame()
        head = a_start + (a_end - a_start) * ease_out(0.25 + t / 0.4, 2)
        tail = a_start + (a_end - a_start) * ease_in(max(0, (t - 0.1) / 0.9), 1.6)
        crescent(f, tail, head, 108, 42 * (1 - ease_in(t, 1.5)) + 2, WHITE, sy=0.5)
        # đường cắt mảnh xuyên tâm (loé 3 frame đầu)
        if t < 0.35:
            L = 135 * ease_out(0.3 + t / 0.35)
            w = 14 * (1 - t / 0.35) + 1
            streak(f, -18, -L, L, w, WHITE, outline=6)
        # tia văng theo tiếp tuyến ở đầu lưỡi
        for k in range(4):
            tt = t - 0.08 * k
            if 0.1 < tt < 0.9:
                a = math.radians(a_end + 10 + k * 9)
                d = 92 + 80 * ease_out((tt - 0.1) / 0.8)
                f.disc(math.cos(a) * d, math.sin(a) * d * 0.55 - 30 * tt ** 2, 6 * (1 - tt), WHITE, outline=6)
        out.append(f.image(1.0 if t < 0.6 else 1 - (t - 0.6) / 0.4 * 0.85))
    return out


def bullet(n=8):
    """Gun: đạn găm — loé 4 cánh nhỏ, tia lửa văng NGƯỢC về phía người bắn (-x) có trọng lực, vòng nhỏ."""
    rng = np.random.default_rng(11)
    sparks = [(180 + rng.uniform(-55, 55), rng.uniform(0.7, 1.15)) for _ in range(7)]
    out = []
    for i in range(n):
        t = i / (n - 1)
        f = Frame()
        f.ring(8 + 38 * ease_out(t), 7 * (1 - t), WHITE, outline=6)
        for a, sp in sparks:
            d = 18 + 95 * sp * ease_out(t, 2.5)
            L = 34 * (1 - ease_in(t, 1.2)) * sp
            ar = math.radians(a)
            hx, hy = math.cos(ar) * d, math.sin(ar) * d - 40 * t ** 2  # rơi xuống
            ang = math.degrees(math.atan2(hy - math.sin(ar) * (d - L), hx - math.cos(ar) * (d - L)))
            # vệt từ (đuôi) tới (đầu) theo hướng bay hiện tại
            tx, ty = hx - math.cos(math.radians(ang)) * L, hy - math.sin(math.radians(ang)) * L
            pts_w = 6 * (1 - t) + 1
            nx, ny = -math.sin(math.radians(ang)), math.cos(math.radians(ang))
            mx, my = tx + (hx - tx) * 0.6, ty + (hy - ty) * 0.6
            if L > 2:
                f.poly([(tx, ty), (mx + nx * pts_w / 2, my + ny * pts_w / 2), (hx, hy), (mx - nx * pts_w / 2, my - ny * pts_w / 2)], WHITE, 6)
        core = 34 * (1 - t / 0.4) if t < 0.4 else 0
        if core > 1:
            star(f, 4, core, core * 0.28, WHITE, rot=0, outline=8)
        out.append(f.image())
    return out


def enemy_hit(n=10):
    """Đòn enemy trúng player (mặc định): ba vệt cào đỏ cam lõi trắng xương + mảnh vỡ — báo nguy hiểm."""
    out = []
    for i in range(n):
        t = i / (n - 1)
        f = Frame()
        for k, off in enumerate((-28, 0, 28)):
            tt = t + 0.04 - 0.05 * k
            if tt <= 0:
                continue
            L = 115 * ease_out(min(1, 0.3 + tt / 0.35), 2)
            w = 26 * (1 - ease_in(tt, 1.3)) + 1
            a = math.radians(-38)
            cx, cy = -math.sin(a) * off, math.cos(a) * off
            p0 = (cx - math.cos(a) * L, cy - math.sin(a) * L)
            p1 = (cx + math.cos(a) * L, cy + math.sin(a) * L)
            nx, ny = -math.sin(a) * w / 2, math.cos(a) * w / 2
            f.poly([p0, (cx + nx, cy + ny), p1, (cx - nx, cy - ny)], ACC, int(min(8, w * SS * 0.3)))
            if w > 6:
                f.poly([p0, (cx + nx * .35, cy + ny * .35), p1, (cx - nx * .35, cy - ny * .35)], BONE, 0)
        for k in range(6):
            a = math.radians(30 + k * 60)
            d = 40 + 90 * ease_out(t, 2)
            f.disc(math.cos(a) * d, math.sin(a) * d - 25 * t * t, 7 * (1 - t), ACC, outline=6)
        out.append(f.image(1.0 if t < 0.55 else 1 - (t - 0.55) / 0.45 * 0.85))
    return out


def crit(n=10):
    """Chí mạng (phủ thêm lên hiệu ứng vũ khí): loé 4 cánh dài mảnh lệch góc, một vòng trắng dày nở, tàn đỏ cam văng."""
    rng = np.random.default_rng(5)
    embers = [(rng.uniform(0, 360), rng.uniform(0.7, 1.2)) for _ in range(8)]
    out = []
    for i in range(n):
        t = i / (n - 1)
        f = Frame()
        f.ring(30 + 100 * ease_out(t), 22 * (1 - t) ** 1.5, WHITE)
        for a, sp in embers:
            d = 40 + 110 * sp * ease_out(t, 2)
            ar = math.radians(a)
            f.disc(math.cos(ar) * d, math.sin(ar) * d - 30 * t * t, 8 * (1 - t) * sp, ACC, outline=6)
        if t < 0.55:
            s_ = 140 * math.sin(math.pi * min(1, 0.3 + t / 0.55 * 0.7))
            star(f, 4, s_, s_ * 0.1, WHITE, rot=20 + 15 * t)
        out.append(f.image(1.0 if t < 0.55 else 1 - (t - 0.55) / 0.45 * 0.85))
    return out


def block(n=8):
    """Đỡ đòn: cung khiên sáng phía đòn đến (+x là hướng đòn đi -> khiên ở -x của hình sau khi lật) + tia toé."""
    out = []
    for i in range(n):
        t = i / (n - 1)
        f = Frame()
        span = 70 * ease_out(0.35 + t / 0.3) if t < 0.3 else 70
        crescent(f, 180 - span, 180 + span, 70 + 25 * ease_out(t), 22 * (1 - ease_in(t, 1.3)) + 2, BONE, sy=1.0)
        for k, a in enumerate((140, 160, 200, 220, 180)):
            d = 75 + 80 * ease_out(t, 2)
            L = 30 * (1 - t)
            streak(f, a, d - L, d, 7 * (1 - t) + 1, WHITE, outline=6)
        out.append(f.image(1.0 if t < 0.55 else 1 - (t - 0.55) / 0.45 * 0.85))
    return out


KINDS = {'fist': fist, 'slash': slash, 'bullet': bullet, 'hit': enemy_hit, 'crit': crit, 'block': block}


def main():
    prev = sys.argv[sys.argv.index('--preview') + 1] if '--preview' in sys.argv else None
    rows = {}
    for k, fn in KINDS.items():
        frames = fn()
        rows[k] = frames
        old = [f for f in os.listdir(OUT) if f.startswith(k + '_') and f.endswith('.png')]
        for i, im in enumerate(frames):
            im.save(os.path.join(OUT, f'{k}_{i:02d}.png'))
        for f in old:  # frame thừa của bản cũ (ít frame hơn thì không có)
            idx = int(f[len(k) + 1:-4])
            if idx >= len(frames):
                os.remove(os.path.join(OUT, f))
                if os.path.exists(os.path.join(OUT, f + '.meta')):
                    os.remove(os.path.join(OUT, f + '.meta'))
        print(k, len(frames))
    if prev:
        W = 128
        n = max(len(v) for v in rows.values())
        sheet = Image.new('RGBA', (W * n, W * len(rows) * 2), (0, 0, 0, 255))
        for r, (k, frames) in enumerate(rows.items()):
            for c, im in enumerate(frames):
                for band, bg in ((0, (58, 63, 74, 255)), (1, (185, 192, 199, 255))):  # nền tối / nền sáng
                    cell = Image.new('RGBA', (W, W), bg)
                    cell.alpha_composite(im.resize((W, W), Image.LANCZOS))
                    sheet.paste(cell, (c * W, (r * 2 + band) * W))
        sheet.save(prev)


if __name__ == '__main__':
    main()
