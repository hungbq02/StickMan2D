# Art style — "bóng đen + 1 màu nhấn" (AS-06)

Chốt 2026-11-18. Mục tiêu: nhìn có chất riêng, không lộ "AI vẽ đều khắp", và **nhân vật luôn nổi hơn nền**.
Chuỗi prompt dùng chung: `SpriteGenRuns/as06/style.py` (BASE / ENV / CHAR / FX / UI).

## Quy tắc thứ bậc (quan trọng nhất)

| Lớp | Độ sáng | Bão hoà | Chi tiết |
|---|---|---|---|
| Trời + lớp xa | sáng vừa, gần màu sương | rất thấp | gần như không |
| Lớp giữa | tối hơn 1 bậc | thấp | ít, khối lớn |
| Đất / tường / bục (đi được) | gần đen + 1 đường viền sáng mảnh ở mép trên | thấp | phẳng |
| **Player** | trắng + viền đen (pack que trắng giữ nguyên) | — | — |
| **Enemy / boss** | đen than + trắng xương | **màu nhấn đỏ cam `#e5482b`** (mắt, khăn, lưỡi vũ khí) | vừa |
| FX | đòn player: trắng · nguy hiểm từ enemy: đỏ cam · trạng thái: màu riêng (băng xanh nhạt, cháy cam) | — | khối phẳng 2–3 tông |

- Cấm ở nền: glow, bloom, tia sáng, hạt lấp lánh, trăng/xoáy sáng chói làm tâm điểm, texture noise, gradient trong khối.
- Dải giữa màn hình (chỗ nhân vật đứng) là sương tông vừa, không chi tiết bận.
- Đỏ cam chỉ dành cho enemy / nguy hiểm → người chơi đọc "đỏ = nguy hiểm, trắng = mình".

## Palette từng màn

| Màn | Chủ đề | Palette (tối → sáng) |
|---|---|---|
| L1 Ashen Moor | ngoại ô pháo đài đổ nát lúc chạng vạng | `#2a2f38 #5b6370 #8d96a2 #b9c0c7` xám xanh |
| L2 Fortress Walls | trong tường thành | `#2d2826 #5a514b #8a7f75 #b8ada2` xám tro ấm |
| L3 Void Throne | đảo đá trôi, ngai xa | `#25242f #4a4860 #77738e #a9a5bd` chàm xám |

## Nhân vật

- Thân / giáp: `#1b1b20`, `#3a3a42`; vạch trắng xương `#e8e4da`; 1 màu nhấn `#e5482b`, dùng ít.
- Viền đậm sạch, cel 2 tông, không texture, không tô vẽ dày.
- Giữ nguyên thiết kế, dáng, tỉ lệ, chiều cao của bản EN-13 (chỉ đổi style + màu) → collider / hitbox / `hitTimes` không phải chỉnh lại.

## UI

Mực phẳng: khối đen than, nét trắng xương, nhấn đỏ cam. Không khung sắt đinh tán, không đá quý, không glow.
