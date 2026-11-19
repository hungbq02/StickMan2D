"""Phóng to frame art enemy bằng Real-ESRGAN (model anime) để hiển thị nét khi enemy được vẽ to trên màn.

    python -I -B -X utf8 Tools/ArtPipeline/upscale.py <Name> [--factor 2] [--exe PATH]

- Chạy realesrgan-x4plus-anime (×4) rồi thu Lanczos về ×factor; giữ kênh alpha.
- Chỉ đụng frame trong manifest["anims"] (FX/ giữ nguyên, có PPU riêng).
- Manifest: cellWidth / cellHeight / pixelsPerUnit × factor => kích thước trong game không đổi; pivot (tỉ lệ) và
  endShift (unit) không đổi. Ghi "upscale": factor để không phóng 2 lần; from_run.py ghi đè manifest => phải chạy lại.
- Exe: --exe, biến môi trường REALESRGAN_EXE, hoặc mặc định ~/Tools/realesrgan/realesrgan-ncnn-vulkan.exe
  (bản ncnn-vulkan từ github.com/xinntao/Real-ESRGAN/releases, không nằm trong repo).
Sau đó: refresh Unity -> Tools/Enemies/Import Enemy Art.
"""
import glob
import json
import os
import shutil
import subprocess
import sys
import tempfile

from PIL import Image

ART_DIR = os.path.join(os.path.dirname(__file__), '..', '..', 'Assets', '_GamePlay', 'Art', 'Enemies')
DEFAULT_EXE = os.path.join(os.path.expanduser('~'), 'Tools', 'realesrgan', 'realesrgan-ncnn-vulkan.exe')
MODEL = 'realesrgan-x4plus-anime'


def find_exe(explicit=None):
    exe = explicit or os.environ.get('REALESRGAN_EXE') or DEFAULT_EXE
    if not os.path.isfile(exe):
        sys.exit('Không thấy realesrgan-ncnn-vulkan: %s (đặt --exe hoặc REALESRGAN_EXE)' % exe)
    return exe


def run(out_dir, factor=2, exe=None):
    """Phóng frame trong thư mục art của 1 enemy (out_dir chứa *_manifest.json)."""
    exe = find_exe(exe)
    manifests = glob.glob(os.path.join(out_dir, '*_manifest.json'))
    if not manifests:
        sys.exit('Không có manifest trong %s' % out_dir)
    mpath = manifests[0]
    m = json.load(open(mpath, encoding='utf-8'))
    if m.get('upscale'):
        print('  đã phóng ×%s rồi — bỏ qua (%s)' % (m['upscale'], os.path.basename(mpath)))
        return
    files = [f for a in m['anims'] for f in a['frames']]
    cw, ch = m['cellWidth'], m['cellHeight']
    tmp = tempfile.mkdtemp(prefix='upscale_')
    try:
        src_dir, dst_dir = os.path.join(tmp, 'in'), os.path.join(tmp, 'out')
        os.makedirs(src_dir)
        os.makedirs(dst_dir)
        for f in files:
            shutil.copy(os.path.join(out_dir, f), os.path.join(src_dir, f))
        # thư mục vào -> thư mục ra (1 lần nạp model); exe in tiến độ ra stderr
        r = subprocess.run([exe, '-i', src_dir, '-o', dst_dir, '-n', MODEL, '-s', '4', '-f', 'png'],
                           capture_output=True, text=True, cwd=os.path.dirname(exe))
        if r.returncode != 0:
            sys.exit('realesrgan lỗi (%d): %s' % (r.returncode, r.stderr[-500:]))
        size = (cw * factor, ch * factor)
        # kiểm hết output trước rồi mới ghi đè => lỗi giữa chừng không để lại nửa cũ nửa mới
        result = {}
        for f in files:
            up = os.path.join(dst_dir, f)
            if not os.path.isfile(up):
                sys.exit('Thiếu output cho %s — không ghi đè gì' % f)
            im = Image.open(up).convert('RGBA')
            if im.size != (cw * 4, ch * 4):
                sys.exit('%s: output %s, cần %s — không ghi đè gì' % (f, im.size, (cw * 4, ch * 4)))
            if im.getchannel('A').getextrema()[1] == 0:
                sys.exit('%s: output trong suốt hoàn toàn (lỗi GPU?) — không ghi đè gì' % f)
            result[f] = im.resize(size, Image.LANCZOS)
        for f, im in result.items():
            im.save(os.path.join(out_dir, f))
    finally:
        shutil.rmtree(tmp, ignore_errors=True)
    m['cellWidth'], m['cellHeight'] = size
    m['pixelsPerUnit'] = m['pixelsPerUnit'] * factor
    m['upscale'] = factor
    with open(mpath, 'w', encoding='utf-8') as fp:
        json.dump(m, fp, indent=1)
    print('  %d frame -> %dx%d, PPU %s' % (len(files), size[0], size[1], m['pixelsPerUnit']))


def main():
    args = sys.argv[1:]
    opt = {}
    for k in ('--factor', '--exe'):
        if k in args:
            i = args.index(k)
            opt[k] = args[i + 1]
            del args[i:i + 2]
    if len(args) != 1:
        sys.exit(__doc__)
    run(os.path.join(ART_DIR, args[0]), int(opt.get('--factor', 2)), opt.get('--exe'))


if __name__ == '__main__':
    main()
