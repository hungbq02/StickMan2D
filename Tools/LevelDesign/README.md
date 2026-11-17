# Level Design — 3 level side-scroll

`levels.json` là dữ liệu bố cục của 3 level (format v1, builder C# đọc). `validate_levels.py` kiểm tra file:

```
python -I -B Tools/LevelDesign/validate_levels.py
```

Toạ độ: đơn vị world, +x phải, +y lên, mặt trên đất chính = 0. Ground dày 4 (đáy y = -4); ground nâng cao (bậc, tường thành) vẫn đáy -4, chỉ tăng `h`. Rơi dưới y = -8 là chết.

## Tầm di chuyển (đã đo)

| Nguồn | Giá trị |
|---|---|
| `Data/PlayerStats.asset` | `moveSpeed` 5, `jumpForce` 13, `wallJumpForceX/Y` 14/14, `wallJumpDuration` 0.2, `dashSpeed` 20, `dashDuration` 0.15, `climbSpeed` 5, `coyoteTime` 0.1 |
| Rigidbody2D Player (SampleScene / BossArena) | `gravityScale` 4, CapsuleCollider2D 0.72 × 1.75 |
| `ProjectSettings/Physics2DSettings.asset` | gravity y = -9.81 |

- g = 9.81 × 4 = **39.24 u/s²**. Nhảy đặt `velocity.y = jumpForce` (JumpState), không có cắt nhảy khi thả nút → độ cao cố định.
- **Độ cao tối đa** H = v²/2g = 13² / 78.48 = **2.15u** → 75% = **1.62u** (bậc cao nhất dùng: 1.3).
- **Khoảng nhảy ngang tối đa** (cùng độ cao): thời gian bay 2v/g = 0.663s × `moveSpeed` 5 = **3.31u** → 75% = **2.48u** (gap lớn nhất dùng: 2.2). Không tính bề ngang collider (+0.72) và coyote time (+0.5u) — là biên an toàn thêm.
- Nhảy lên mặt cao dy: t = (v + √(v² − 2g·dy)) / g, tầm = 5t. Validator dùng 75% của tầm này và chặn dy > 1.62. Ví dụ dy +1.0 → 75% = 2.15 (dùng gap 2.0); dy +1.3 → 2.02.
- **Wall jump**: `velocity = (±14, 14)` khoá 0.2s → đi ngang 2.8u, lên ~2.0u, rồi còn ~0.5u quán tính. Không bám lại được bức tường vừa nhảy ra (`LastWallJumpDirection`) → **leo tường cần giếng 2 tường đối diện**. Lòng giếng dùng 2.0–2.5u.
- **Dash**: 20 × 0.15 = 3u ngang, không rơi (velocity.y = 0), 1 lần trên không. Không cần dash để qua bất kỳ chỗ nào.
- **Thang**: chỉ vào được từ dưới đất bấm lên (`GroundedState`); trên mép không có cách bấm xuống để leo xuống. Leo hết thang → Idle; ClimbState cho nhảy ra khỏi thang.

## Level01 — "Phế tích ngoại thành" (120u, 4 section × 30)

Mục tiêu: dạy di chuyển trước, đánh sau; không boss.
- S0 (0–30): 2 hố dạy nhảy — hố 2.0 lên bậc +1.0, rồi hố 2.2 xuống lại. Chưa có enemy (enemy đầu ở x 36, cách spawn 33).
- S1 (30–60): nhóm 3 MeleeEnemy trên nền phẳng. Thang (x 50.8, cao 4) lên lối đi treo y 4 vắt qua hố rộng 8u — chỉ qua được bằng thang; cuối lối đi nhảy xuống (đây là phần "xuống", vì thang không leo xuống từ trên được).
- S2 (60–90): nhóm 3 MeleeEnemy, nghỉ, rồi giếng wall-jump thấp (2 tường, lòng 2.0, cao 6). Tường trái treo từ y 2.2 nên player đi lọt vào giếng; tường phải cao 6 chắn đường.
- S3 (90–120): MeleeEnemy + Axeman (giới thiệu đòn lao), hố 2.0, 2 Axeman sát cổng, cổng ra x 117.
- Tổng 10 enemy (7 MeleeEnemy, 3 Axeman).

## Level02 — "Tường thành nguyền" (150u, 5 section × 30)

Mục tiêu: chiều dọc + kẻ địch tầm xa, kết bằng mini-boss.
- S0: 2 MeleeEnemy dưới đất, Stalker trên bậc +1.2 (bậc rộng 12).
- S1: hố 2.0 xuống nền 0; nhóm Knight + 2 MeleeEnemy ngay chân thang; thang cao 4.6 lên gờ, gờ nối liền mặt tường thành.
- S2: tường thành y 4.6 (MeleeEnemy + Stalker), nhảy xuống sân trong: Knight dưới đất, MeleeEnemy + Stalker trên bậc +1.3 (rộng 7).
- S3: **giếng wall-jump cao 11** (lòng 2.5) → lối đi trên cao y 11 có Knight + Stalker → nhảy xuống (rơi 11u, không có fall damage trong code) — đoạn thở trước arena.
- S4: arena Demon phẳng 122–144 (22u), trigger 124, Demon x 135. Cổng ra x 147 sau arena.
- Tổng 13 enemy thường + Demon = 14 (6 MeleeEnemy, 3 Knight, 4 Stalker).

Stalker đặt trên gờ chỉ cao hơn nền player 1.2–1.3: `StalkerData.maxHeightDifference` = 1.5, gờ cao hơn thì Stalker không nhận ra player dưới đất.

## Level03 — "Ngai Hư Không" (104u, 4 section 25/25/27/27)

Mục tiêu: chặng ngắn dồn dập rồi boss cuối.
- S0: 2 MeleeEnemy + Knight.
- S1: hố 2.0, nền phẳng (MeleeEnemy) rồi bậc +1.3 (MeleeEnemy + Stalker).
- S2: 2 platform nổi qua hố (gap 2.0 mỗi bước, +1.0 rồi −1.0) — đoạn thở, rồi Knight + MeleeEnemy, bậc +1.3 có MeleeEnemy + Stalker.
- S3: arena Unmaker phẳng 80–103 (23u), không vật gì phía trên (camera maxY 15), trigger 82. Không có exit — boss chết là hết game.
- Tổng 10 enemy thường + Unmaker (6 MeleeEnemy, 2 Knight, 2 Stalker).

## Điểm cần builder / play-test lưu ý

- Mỗi level có tường chặn đầu (x 0–1) và tường cuối (L1 119, L2 149, L3 103) để player không đi ra khỏi map; L3 tường cuối chính là mép arena.
- Tường arena boss (`BossEncounter.arenaWalls`) không có trong JSON — builder tự dựng tại arenaMinX / arenaMaxX.
- **Dash tắt collider** (`DashState` → `Collider.enabled = false`): player có thể dash xuyên tường dày 1u (3u dash) → bỏ qua được giếng wall-jump ở L1 / L2. Chưa xử lý trong dữ liệu (làm tường dày > 3u sẽ khiến dash kết thúc trong tường).
- Đỉnh thang = mặt gờ, thang đặt sát mép gờ (không nằm dưới gờ). Chưa kiểm cảm giác leo ra khỏi đỉnh thang; nếu bị rơi lại, nâng đỉnh thang thêm ~1u.
- Unmaker là boss bay nhưng `y` ghi theo mặt sàn arena (0); controller tự lo độ cao bay.
