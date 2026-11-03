"""Dựng sprite enemy từ sprite sheet AI-gen trong Assets/_GamePlay/Art/EnemyConcepts.

    python -I Tools/ArtPipeline/build.py [tên ...] [--preview DIR]

Mỗi nhân vật: tách nền (matte) -> sửa tay (xoá nhãn / cắt chỗ dính) -> cắt frame (slice)
-> cắt hiệu ứng rời (fx). Kết quả: Assets/_GamePlay/Art/Enemies/<Name>/ gồm frame PNG +
<id>_manifest.json (Unity: menu Tools/Enemies/Import Enemy Art đọc file này để set pivot/PPU,
tạo AnimationClip + AnimatorController).

Toạ độ trong cấu hình là pixel của sheet gốc. Ghi chú các lỗi của sheet gốc nằm cạnh cấu hình.
Cần: numpy, scipy, pillow.
"""
import json, os, sys, shutil
import numpy as np
from PIL import Image

HERE = os.path.dirname(os.path.abspath(__file__))
sys.path.insert(0, HERE)
from matte import matte          # noqa: E402
from slice import slice_sheet    # noqa: E402
from fx import cut_fx            # noqa: E402

ROOT = os.path.normpath(os.path.join(HERE, '..', '..'))
SRC_DIR = os.path.join(ROOT, 'Assets', '_GamePlay', 'Art', 'EnemyConcepts')
OUT_DIR = os.path.join(ROOT, 'Assets', '_GamePlay', 'Art', 'Enemies')

# ---------------------------------------------------------------- cấu hình từng sheet

AXEMAN = dict(
    name='Axeman', source='melee_enemy_concept_Spritesheet.png',
    # ô ca rô 178/215, chu kỳ ~23.4px; vạch đất đen dài dưới mỗi hàng
    matte=[dict(tones=[178, 215], period=23.4, kill_lines=[['h', 300]], glow_sigma=5)],
    slice=dict(id='axeman', ppu=115, anims=[
        dict(name='idle', fps=6, loop=True, band=[0, 50, 800, 290], ground=286, anchor='head'),
        dict(name='walk', fps=10, loop=True, band=[820, 50, 1960, 290], ground=286, anchor='head'),
        dict(name='hit', fps=12, band=[1990, 50, 2816, 290], ground=286, anchor='head'),
        # sheet gốc đặt nhãn ngược: hàng "Death" là đòn chém, hàng "Slash" là ngã gục
        dict(name='slash', fps=12, band=[0, 350, 800, 580], ground=577, anchor='head'),
        dict(name='death', fps=10, band=[820, 350, 2816, 580], ground=577, anchor='hip'),
        dict(name='smash', fps=10, band=[0, 660, 985, 930], ground=928, anchor='head'),
        dict(name='spin', fps=14, band=[985, 660, 2816, 930], ground=928, anchor='head'),
        dict(name='charge', fps=14, band=[0, 975, 2816, 1215], ground=1211, anchor='hip'),
        dict(name='tellSmash', fps=1, band=[0, 1290, 240, 1535], ground=1531, anchor='head'),
        dict(name='tellCharge', fps=1, band=[240, 1290, 505, 1535], ground=1531, anchor='head'),
        dict(name='enrage', fps=8, band=[505, 1290, 1600, 1535], ground=1531, anchor='head'),
        # frame 5 bị watermark ngôi sao đè -> bỏ
        dict(name='taunt', fps=8, band=[1600, 1290, 2816, 1535], ground=1531, anchor='head', drop=[5]),
    ]),
)

DEMON = dict(
    name='Demon', source='mini_boss_concept_SpriteSheey.png',
    # nền trắng phẳng; xoá nhãn, mũi tên "->" giữa các frame combo, chữ THUD / ROAR!, hình người que làm thước
    matte=[dict(flat=True, tones=[247, 255], tone_tol=8, sat_max=12, min_bg_comp=400, min_pocket=30, pocket_near=0.6,
                erase_rects=[[0, 0, 2816, 48], [0, 200, 90, 250], [0, 400, 170, 448], [0, 578, 240, 622], [0, 770, 240, 815],
                             [1520, 770, 1720, 815], [0, 958, 240, 1000], [1600, 958, 1800, 1000], [0, 1142, 260, 1186],
                             [1060, 1145, 1330, 1185], [1790, 1145, 1880, 1185], [0, 1330, 1100, 1396], [1100, 1330, 1230, 1370],
                             [376, 1046, 434, 1082], [782, 1046, 840, 1082], [1026, 1046, 1084, 1082], [1254, 1046, 1306, 1082],
                             [1502, 1046, 1560, 1082], [1710, 1046, 1772, 1082],
                             [896, 838, 950, 866], [2198, 1182, 2230, 1258], [2332, 1182, 2432, 1268], [20, 60, 90, 200]])],
    slice=dict(id='demon', ppu=58, anims=[
        dict(name='idle', fps=6, loop=True, band=[100, 50, 905, 205]),
        dict(name='walk', fps=10, loop=True, band=[905, 50, 2160, 205]),
        dict(name='hit', fps=12, band=[2160, 50, 2816, 205]),
        dict(name='run', fps=14, loop=True, band=[0, 220, 2816, 400]),
        dict(name='death', fps=10, band=[0, 420, 2816, 590]),
        dict(name='swing', fps=14, band=[0, 600, 2816, 780]),
        dict(name='smash', fps=12, band=[0, 790, 1520, 960], air=[2, 3]),
        dict(name='bash', fps=12, band=[1520, 790, 2816, 960]),
        dict(name='combo', fps=12, band=[0, 975, 1740, 1145]),
        dict(name='shockwave', fps=10, band=[1740, 975, 2816, 1145]),
        dict(name='shield', fps=10, band=[0, 1180, 1010, 1340]),
        # hàng "Braced sound attack" bị AI vẽ bóng mờ chồng nhau -> không dùng
        dict(name='roar', fps=8, band=[1790, 1180, 2816, 1340]),
        dict(name='tellSmash', fps=6, band=[0, 1380, 580, 1536]),
        dict(name='tellCharge', fps=6, band=[580, 1380, 1100, 1536]),
        dict(name='enrage', fps=10, band=[1100, 1370, 2816, 1536], drop=[8]),  # frame cuối dính watermark
    ]),
)

KNIGHT = dict(
    name='Knight', source='elite_enemy_concept_spritesheet.png',
    # 2 vùng nền: trên = ca rô 202/218 chu kỳ ~34px, dưới (Rage) = xám phẳng ~218
    matte=[dict(rect=[0, 0, 2048, 1712], tones=[202, 218], period=34, tone_tol=8, blob_tone_tol=6, pocket_tone_tol=5, glow_sigma=5),
           dict(rect=[0, 1712, 2048, 2048], flat=True, tones=[212, 224], tone_tol=6, sat_max=10, min_bg_comp=400, min_pocket=30,
                pocket_near=0.6, glow_sigma=5)],
    post=[dict(op='clear', rects=[[0, 1709, 2048, 1715],                                   # tách 2 vùng
                                  [1255, 498, 1314, 530], [1500, 498, 1562, 530], [1376, 740, 1460, 800],  # THUD, CRASH
                                  [20, 8, 235, 46], [28, 46, 98, 78], [262, 46, 352, 80], [1066, 46, 1302, 84], [18, 303, 322, 341],
                                  [28, 341, 88, 377], [428, 303, 558, 342], [1068, 303, 1172, 342], [28, 564, 262, 603],
                                  [28, 603, 222, 640], [733, 567, 988, 606], [1485, 595, 1598, 635], [28, 862, 262, 901],
                                  [28, 901, 262, 941], [28, 1138, 238, 1183], [28, 1180, 222, 1216], [1075, 1168, 1302, 1213],
                                  [28, 1428, 188, 1471], [28, 1467, 192, 1503], [468, 1434, 698, 1472], [813, 1435, 1018, 1473],
                                  [32, 1714, 222, 1759], [1235, 1714, 1370, 1759], [1558, 1714, 1708, 1759]])],
    slice=dict(id='knight', ppu=88, text_filter=True, min_body_h=75, anims=[
        dict(name='idle', fps=1, loop=True, band=[0, 60, 240, 295], anchor='head', face_min=200, face_top=0.45),
        dict(name='walk', fps=8, loop=True, band=[240, 60, 1040, 295], anchor='head', face_min=200, face_top=0.45),
        dict(name='stance', fps=8, band=[1040, 60, 2048, 295], anchor='head', face_min=200, face_top=0.45),
        dict(name='hit', fps=10, band=[0, 350, 420, 565], anchor='head', face_min=200, face_top=0.45),
        dict(name='stagger', fps=8, band=[420, 350, 1040, 565], anchor='head', face_min=200, face_top=0.45),
        dict(name='taunt', fps=6, band=[1040, 350, 2048, 565], anchor='head', face_min=200, face_top=0.45),
        dict(name='slash', fps=12, band=[0, 640, 700, 860], anchor='head', face_min=200, face_top=0.45),
        # vệt chém + CRASH vẽ tách khỏi người -> gộp vào frame chém
        dict(name='overhead', fps=10, band=[700, 560, 1470, 860], anchor='head', face_min=200, face_top=0.45, merge_frames=[[2, 3]]),
        dict(name='cleave', fps=10, band=[1470, 640, 2048, 860], anchor='head', face_min=200, face_top=0.45),
        # kiếm frame 2-3 và 5-6 đè chéo nhau -> tách bằng seed
        dict(name='combo', fps=12, band=[0, 860, 1400, 1145], anchor='register',
             seeds=[[120, 1040], [360, 1040], [640, 1040], [860, 1040], [1060, 1040], [1290, 1040]]),
        dict(name='cinder', fps=10, band=[0, 1150, 1050, 1430], anchor='head', face_min=200, face_top=0.45),
        dict(name='rupture', fps=1, band=[1050, 1150, 1480, 1430], anchor='head', face_min=200, face_top=0.45),
        dict(name='tellSlash', fps=6, band=[0, 1430, 470, 1711], anchor='head', face_min=200, face_top=0.45),
        dict(name='tellExecute', fps=1, band=[470, 1430, 800, 1711], anchor='head', face_min=200, face_top=0.45),
        dict(name='tellRupture', fps=1, band=[800, 1430, 1250, 1711], anchor='head', face_min=200, face_top=0.45),
        dict(name='enrage', fps=6, band=[0, 1712, 1180, 2048], anchor='register'),
        # "Rage Walk" chỉ còn 1 frame sạch (frame 2 dính watermark) -> dùng walk + tô màu trong game
        dict(name='rageIdle', fps=1, loop=True, band=[1180, 1712, 1515, 2048], anchor='register'),
    ]),
    fx=[dict(name='knight_ruptureFlame', align='bottom', fps=12,
             rects=[[1470, 1150, 1584, 1430], [1586, 1150, 1730, 1430], [1730, 1150, 1870, 1430], [1870, 1150, 2030, 1430]])],
    fx_ppu=88,
)

_STALKER_BOX_COLS = [30, 197, 366, 537, 706, 876, 1045, 1216, 1429, 1598, 1768, 1939, 2110, 2280, 2449, 2617]
_STALKER_BOX_TOPS = [120, 329, 538, 741, 933, 940, 1141, 1342]
STALKER = dict(
    name='Stalker', source='ranged_enemy_concept_spriteSheet.png',
    # ca rô 118/137 chu kỳ ~24px, mỗi frame trong 1 khung ô đen có số thứ tự
    matte=[dict(tones=[118, 137], period=24, tone_tol=8, blob_tone_tol=6, pocket_tone_tol=5,
                kill_lines=[['h', 250], ['v', 120]], line_dark=105, line_grow=1,
                grid_lines=dict(dark=125, sat=30, min_count=300, grow=2), erase_rects=[[0, 0, 2816, 58]], glow_sigma=4)],
    post=[dict(op='clear', rects=[[1420, 1099, 1640, 1116],   # cung của Rain f1 chạm Piercing f1
                                  [2439, 530, 2448, 700],     # Charged f7 dính mũi tên f9
                                  [1428, 1100, 1632, 1137]]), # nhãn "Rain of Arrows"
          dict(op='box_numbers', cols=_STALKER_BOX_COLS, tops=_STALKER_BOX_TOPS, size=[33, 37], max_gray=95)],
    slice=dict(id='stalker', ppu=90, text_filter=True, text_min_n=60, min_comp_w=12, anims=[
        dict(name='idle', fps=8, loop=True, band=[30, 115, 1040, 290], anchor='head'),
        dict(name='walk', fps=10, loop=True, band=[30, 320, 1390, 495], anchor='head'),
        dict(name='run', fps=12, loop=True, band=[30, 530, 1390, 700], anchor='head'),
        dict(name='hit', fps=12, band=[30, 735, 700, 905]),
        dict(name='death', fps=10, band=[30, 935, 1390, 1105], anchor='hip', min_body_h=30),
        # AI vẽ lệch khung: vệt gió ở ô 2 là đuôi của người ở ô 3 -> chỉ có 5 frame
        dict(name='dash', fps=12, band=[30, 1135, 1060, 1305], anchor='head',
             seeds=[[163, 1250], [395, 1230], [585, 1230], [770, 1235], [960, 1240]]),
        # hàng "Stealth Enter" thứ 2 là biến thể thiếu frame -> không dùng
        dict(name='stealthIn', fps=10, band=[30, 1335, 880, 1510], anchor='hip', min_body_h=20, extra_reach=8),
        dict(name='aim', fps=10, band=[1425, 115, 2300, 290], anchor='head'),
        # các frame chỉ có mũi tên / tia bay (không có người) bị loại nhờ min_body_h, xuất riêng ở fx
        dict(name='quick', fps=14, band=[1425, 320, 2460, 495], anchor='head'),
        dict(name='charged', fps=10, band=[1425, 530, 2800, 700], anchor='head', extra_reach=5, min_body_h=90,
             exclude=[[2446, 530, 2617, 700]]),
        dict(name='triple', fps=12, band=[1425, 735, 2460, 905], anchor='head'),
        dict(name='piercing', fps=12, band=[1425, 935, 2800, 1100], anchor='head', extra_reach=5, min_body_h=90),
        dict(name='rain', fps=10, band=[1425, 1116, 2640, 1305], anchor='head', extra_reach=5),
        dict(name='stealthOut', fps=10, band=[1940, 1335, 2800, 1510], anchor='hip', min_body_h=10, drop=[4]),  # f6 dính watermark
    ]),
    fx=[dict(name='stalker_arrow', rects=[[1772, 333, 1936, 487]]),
        dict(name='stalker_arrowCharged', rects=[[2447, 541, 2615, 694]]),
        dict(name='stalker_pierceBeam', rects=[[2284, 944, 2447, 1095]]),
        dict(name='stalker_arrowPierce', rects=[[2452, 944, 2615, 1095]]),
        dict(name='stalker_arrowRain', rects=[[2621, 1144, 2782, 1298]])],
    fx_ppu=90,
)

_UNMAKER_LABELS = [[18, 10, 56, 34], [510, 10, 562, 34], [18, 181, 50, 204], [408, 196, 465, 222], [18, 356, 68, 380],
                   [544, 371, 596, 395], [18, 508, 122, 534], [544, 508, 628, 534], [18, 638, 92, 664], [534, 638, 644, 664],
                   [18, 761, 142, 788], [16, 888, 98, 914], [536, 888, 592, 914]]
UNMAKER = dict(
    name='Unmaker', source='boss_concept_spritesheet.png',
    # sheet 1024px (frame ~110px, mờ khi phóng to); ca rô 185/228 chu kỳ ~20.5px; viền trắng "sticker" quanh nhân vật
    matte=[dict(tones=[185, 228], period=20.4, tone_tol=9, blob_tone_tol=7, pocket_tone_tol=6, min_bg_comp=800, min_pocket=25,
                glow_sigma=3, extra_bg_ranges=[[236, 256]], sat_max=14)],
    post=[dict(op='clear', rects=[[0, 0, 1024, 34],            # tiêu đề
                                  [821, 405, 828, 503]]),      # tách tia beam khỏi đầu trượng
          dict(op='clear_lowsat', rects=_UNMAKER_LABELS, grow=2, max_sat=45)],
    slice=dict(id='unmaker', ppu=35, text_filter=True, text_min_n=60, text_max_h=40, min_body_h=80, merge=2, pad=4, anims=[
        dict(name='idle', fps=6, loop=True, band=[0, 35, 500, 165], anchor='head', face_min_px=20),
        dict(name='move', fps=8, loop=True, band=[500, 35, 1024, 165], anchor='head', face_min_px=20),
        dict(name='hit', fps=10, band=[0, 200, 395, 340], anchor='head', face_min_px=20, extra_reach=30),
        dict(name='death', fps=6, band=[395, 200, 1024, 365], anchor='hip'),
        dict(name='slash', fps=12, band=[0, 380, 500, 510], anchor='head', face_min_px=20,
             seeds=[[62, 460], [156, 460], [262, 465], [368, 455], [452, 465]]),
        dict(name='beam', fps=8, band=[500, 380, 822, 510], anchor='head', face_min_px=20, extra_reach=4),
        dict(name='wingBurst', fps=8, band=[0, 520, 530, 640], anchor='head', face_min_px=20),
        dict(name='summon', fps=8, band=[530, 520, 925, 640], anchor='head', face_min_px=20, extra_reach=4),
        dict(name='teleport', fps=6, band=[0, 640, 520, 770], anchor='bbox', min_body_h=60),
        dict(name='meteor', fps=8, band=[520, 640, 1024, 770], anchor='head', face_min_px=20,
             seeds=[[585, 725], [690, 725], [785, 725], [875, 725], [968, 725]]),
        dict(name='phase', fps=10, band=[0, 770, 1024, 895], anchor='head', face_min_px=20),
        dict(name='rageIdle', fps=6, loop=True, band=[0, 895, 530, 1024], anchor='head', face_min_px=20),
        dict(name='taunt', fps=6, band=[530, 895, 1024, 1024], anchor='head', face_min_px=20, drop=[4]),  # f5 dính watermark
    ]),
    fx=[dict(name='unmaker_beam', rects=[[828, 400, 1024, 505]]),
        dict(name='unmaker_imp', align='bottom', fps=8, loop=True, rects=[[918, 588, 956, 630], [962, 574, 996, 616]], min_mass=10),
        dict(name='unmaker_meteor', rects=[[534, 707, 568, 748]], min_mass=10)],
    fx_ppu=35,
)

SHEETS = [AXEMAN, STALKER, KNIGHT, DEMON, UNMAKER]

# ---------------------------------------------------------------- pipeline


def _post(rgba, src_rgb, ops):
    g = src_rgb.astype(int).mean(2)
    rg = rgba[..., :3].astype(int)
    sat = rg.max(2) - rg.min(2)
    for op in ops:
        if op['op'] == 'clear':
            for (x0, y0, x1, y1) in op['rects']:
                rgba[y0:y1, x0:x1, 3] = 0
        elif op['op'] == 'clear_lowsat':  # nhãn chữ: xoá pixel ít bão hoà (giữ hiệu ứng tím đè lên)
            k = op.get('grow', 0)
            for (x0, y0, x1, y1) in op['rects']:
                sl = (slice(y0 - k, y1 + k), slice(x0 - k, x1 + k))
                a = rgba[sl][..., 3]
                a[sat[sl] < op.get('max_sat', 45)] = 0
                rgba[sl + (3,)] = a
        elif op['op'] == 'box_numbers':  # số thứ tự đen ở góc trên-trái mỗi khung ô
            w, h = op['size']
            for x0 in op['cols']:
                for t in op['tops']:
                    sl = (slice(t + 3, t + 3 + h), slice(x0 + 3, x0 + 3 + w))
                    a = rgba[sl][..., 3]
                    a[g[sl] < op['max_gray']] = 0
                    rgba[sl + (3,)] = a
    return rgba


def build(sheet, preview_dir=None):
    print('== %s (%s)' % (sheet['name'], sheet['source']))
    src = np.asarray(Image.open(os.path.join(SRC_DIR, sheet['source'])).convert('RGB'))
    rgba = np.zeros(src.shape[:2] + (4,), np.uint8)
    for m in sheet['matte']:
        x0, y0, x1, y1 = m.get('rect', [0, 0, src.shape[1], src.shape[0]])
        rgba[y0:y1, x0:x1] = matte(src[y0:y1, x0:x1], m)
    rgba = _post(rgba, src, sheet.get('post', []))

    out = os.path.join(OUT_DIR, sheet['name'])
    # xoá frame cũ (giữ .meta để GUID / import setting không đổi khi tên trùng)
    if os.path.isdir(out):
        for root, _, files in os.walk(out):
            for f in files:
                if f.endswith('.png') or f.endswith('_manifest.json'):
                    os.remove(os.path.join(root, f))
    preview = os.path.join(preview_dir, sheet['name'] + '.png') if preview_dir else None
    manifest = slice_sheet(rgba, sheet['slice'], out, preview)
    manifest['fx'] = cut_fx(rgba, sheet['fx'], os.path.join(out, 'FX'), sheet['fx_ppu']) if sheet.get('fx') else []
    with open(os.path.join(out, '%s_manifest.json' % sheet['slice']['id']), 'w', encoding='utf-8') as f:
        json.dump(manifest, f, indent=1)
    # .meta mồ côi (frame không còn sinh ra) -> xoá để Unity không báo lỗi
    for root, _, files in os.walk(out):
        for f in files:
            if f.endswith('.png.meta') and not os.path.exists(os.path.join(root, f[:-5])):
                os.remove(os.path.join(root, f))


if __name__ == '__main__':
    args = sys.argv[1:]
    preview_dir = None
    if '--preview' in args:
        i = args.index('--preview'); preview_dir = args[i + 1]; del args[i:i + 2]
        os.makedirs(preview_dir, exist_ok=True)
    names = set(a.lower() for a in args)
    for s in SHEETS:
        if not names or s['name'].lower() in names:
            build(s, preview_dir)
