# StickFigure — Hành động người chơi

> Thống kê từ code + `Input/PlayerInputs.inputactions` + asset data (2026-10-15). Số liệu chỉnh ở `Data/PlayerStats`, `DataCombat/*`, `Data/Enemy/*`.

## 1. Phím điều khiển

| Hành động | Bàn phím | Gamepad | Ghi chú |
|---|---|---|---|
| Di chuyển trái / phải | `A` `D` / `←` `→` | Left stick | |
| Leo lên / xuống thang | `W` `S` / `↑` `↓` | Left stick | Chỉ khi đứng cạnh thang |
| Nhảy | `Space` | A (South) | |
| Tấn công | `Enter` | A (South) ⚠️ | |
| Dash | `Shift` | A (South) ⚠️ | |
| Slide | `Ctrl` | A (South) ⚠️ | |
| Đổi vũ khí Fighter / Sword / Gun | `F1` / `F2` / `F3` | — | Cheat — chỉ Editor / Development Build |

- Nút bấm được **buffer 0.1s**: bấm hơi sớm (vd: nhảy ngay trước khi chạm đất) vẫn được tính.
- ⚠️ **Lỗi cấu hình gamepad**: Jump, Attack, Dash, Slide đều gán vào cùng nút **A (South)** → bấm 1 lần kích hoạt cả 4. Cần gán lại (gợi ý: A = Jump, X = Attack, B = Dash, Y hoặc RB = Slide).

## 2. Di chuyển

| Hành động | Điều kiện | Kết quả | Thông số |
|---|---|---|---|
| **Chạy** | Đứng trên đất, ấn trái/phải | Chạy, tự quay mặt theo hướng ấn; buông phím dừng ngay | 5 u/s |
| **Nhảy** | Trên đất, hoặc vừa rời mép trong **0.1s** (coyote time) | Bật lên, vẫn điều khiển ngang được trên không | lực 13 |
| **Rơi** | Rời mép / hết đà nhảy | Điều khiển ngang trên không; camera nhìn trước xuống dưới khi rơi nhanh | |
| **Tiếp đất** | Chạm đất | Animation land 0.1s — **nhảy / đánh / dash / slide huỷ được** ngay | |
| **Dash** | Trên đất: luôn được · Trên không: **1 lần** mỗi lần rời đất | Lao ngang theo hướng mặt, để lại bóng mờ; **tắt collider khi dash** (xuyên qua & không bị trúng đòn) | 20 u/s × 0.15s |
| **Slide** | Đứng trên đất | Trượt theo hướng mặt, collider thấp còn 50% (chui qua khe thấp); dừng khi hết giờ hoặc rời đất | 15 u/s × 0.2s |
| **Bám tường (wall slide)** | Trên không, chạm tường, **ấn về phía tường** | Trượt chậm xuống; buông hướng → rơi | −1 u/s |
| **Nhảy tường (wall jump)** | Đang bám tường + nhảy (kể cả ấn hướng ngược cùng lúc) | Bật chéo ra khỏi tường, khoá điều khiển 0.2s giữ quán tính; **không bám lại ngay bức tường vừa rời** | (14, 14) |
| **Leo thang** | **Đứng trên đất** cạnh thang + ấn lên | Leo lên/xuống theo phím (không trọng lực); nhảy để rời; tự thoát khi chạm đất hoặc ra khỏi thang | 5 u/s |

## 3. Tấn công

| Vũ khí | Trên mặt đất | Trên không |
|---|---|---|
| **Fighter** (tay không) | Combo **3 đòn** — bấm tiếp trong **0.4s** để nối đòn; đứng yên khi đánh | **Đá chéo xuống** (lao 25 u/s) tới khi chạm đất — gây damage **1 lần** lúc bắt đầu |
| **Sword** | Combo **3 đòn** (0.4s nối) | Chém trên không, giữ tư thế tới khi chạm đất |
| **Gun** | Bắn 1 viên / lần bấm (0.2s), đạn ra **đúng đầu nòng cùng frame giật súng** | Bắn ngang, giữ tư thế bắn 0.2s rồi rơi |

**Chỉ số đòn đánh** (crit roll 1 lần / đòn; mọi mục tiêu trúng cùng kết quả):

| Vũ khí | Đòn | Damage | Knockback | Crit (tỉ lệ × hệ số) |
|---|---|---|---|---|
| Fighter | Combo 1 / 2 / 3 | 10 / 10 / 10 | 1 | 50% × 2 |
| Fighter | Đá trên không | 10 | 3 | 40% × 2 |
| Sword | Combo 1 / 2 / 3 | 15 / 15 / 20 | 2 | 20% / 20% / 50% × 2 |
| Sword | Chém trên không | 20 | 2 | 0% |
| Gun | Bắn | 10 | 2 | 5% × 1.5 |
| Gun | Bắn trên không | 10 | 2 | 0% |

- Đạn: 15 u/s, tự biến mất sau 5s; trúng enemy hoặc hết thời gian thì trả về pool.
- Mỗi đòn cận chiến chỉ trúng **1 lần / mục tiêu** dù chạm nhiều collider.
- Đòn đánh mang **phe** (Player / Enemy): không gây damage cho đồng đội.

## 4. Nhận sát thương & hồi sinh

| Tình huống | Kết quả | Thông số |
|---|---|---|
| **Bị đánh** | Huỷ đòn đang đánh, khựng (animation hit), bị đẩy lùi rồi hãm lại trên đất | Khựng 0.3s · lùi ~0.26u (knockback 5) |
| **Sau khi bị đánh** | **Bất tử + nhấp nháy** | 0.8s |
| **Bị choáng** (đòn có Stun) | Không điều khiển được tới hết thời gian choáng | Theo `effectDuration` của đòn |
| **Hết máu** | Animation death, khoá điều khiển, không nhận thêm damage → **hồi sinh** tại điểm xuất phát, đầy máu, bất tử 0.8s | Chờ 2s · Máu 100 |
| **Đòn bị khiên chặn** | Không mất máu, không knockback / effect | Khiên chặn theo hướng |

## 5. Đối thủ (MeleeEnemy)

| Hành vi | Mô tả | Thông số |
|---|---|---|
| Tuần tra | Đi qua lại quanh điểm xuất phát, nghỉ ở 2 đầu; không rơi khỏi mép / đâm tường | 1.5 u/s · ±3u |
| Đuổi | Thấy player cùng tầng trong tầm; bỏ đuổi khi player ra xa | Thấy 6u · bỏ 9u · 3 u/s |
| Đánh | Dừng lại, **sprite tô đỏ báo hiệu** rồi chém | Vung 0.45s · 10 dmg · knockback 5 · hồi chiêu 1.2s |
| Bị đánh | Khựng, quay về phía bị đánh, đuổi lại | Máu 50 |
| Player chết | Bỏ mục tiêu, quay về tuần tra | |

3 **bia tập** (hình người đỏ không có AI) đứng yên để thử đòn: 100 máu.

## 6. Giao diện

- **HUD** (góc trên trái): thanh máu (chạy mượt, đổi màu đỏ → vàng → xanh theo %) + số máu `hiện/tối đa` + tên vũ khí đang cầm.
- **Popup số damage** bay lên khi player / enemy mất máu.

## 7. Độ phủ animation

| Animation | Fighter | Sword | Gun | Dùng ở |
|---|---|---|---|---|
| idle, run, jump, fall, land | ✅ | ✅ | ✅ | Di chuyển (wall jump dùng `jump`) |
| dash, slide, climb, wallSlide | ✅ | ✅ | ✅ | Di chuyển |
| attack1–3 / attack | ✅ | ✅ | ✅ (`attack_Gun`) | Tấn công mặt đất |
| airAttack | ✅ | ✅ | ✅ | Tấn công trên không |
| hit | ✅ | ✅ | ✅ | Bị đánh + bị choáng |
| death | ✅ | ✅ | ✅ | Hết máu |
| **walk** | ❌ | ❌ | ❌ | **Chưa dùng** — game không có trạng thái đi bộ |

## 8. Hạn chế / ghi chú hiện tại

| Mục | Hiện trạng | Gợi ý |
|---|---|---|
| Gamepad | 4 hành động chung 1 nút A | Gán lại binding (mục 1) |
| Hiệu ứng Stun / Burn / Freeze | Hệ thống có sẵn nhưng **chưa vũ khí nào gây hiệu ứng** | Gán `attackEffect` + `effectDuration` cho `AttackDataSO` muốn dùng |
| Dash trên đất | Không giới hạn, không cooldown (trên không 1 lần) | Thêm cooldown nếu muốn hạn chế spam |
| Leo thang | Chỉ bám được khi **đứng trên đất** — không bám thang khi đang nhảy / rơi | Thêm điều kiện bám thang trong `AirborneState` nếu cần |
| Walk | 3 clip chưa dùng | Dùng khi joystick nghiêng nhẹ (mobile) hoặc xoá |
| Màn chơi | Chưa có thắng màn / giới hạn mạng / checkpoint (GS-02) | — |
