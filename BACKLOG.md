# StickFigure — Backlog kỹ thuật

> Mục tiêu: dọn nợ kỹ thuật hiện tại và dựng nền để scale (nhiều vũ khí, nhiều loại enemy, nhiều level) mà không phải viết lại.
> Cập nhật lần cuối: 2026-10-15

**Ký hiệu**
- Ưu tiên: `P0` làm ngay · `P1` trước khi thêm content mới · `P2` khi scale · `P3` nice-to-have
- Effort: `S` < 2h · `M` nửa ngày – 1 ngày · `L` 2–3 ngày
- 🖥️ = cần mở Unity Editor (prefab, animation clip, Inspector, Play mode)
- `→ X` = phụ thuộc task X
- 🟡 = đã làm một phần (phần còn lại ghi ngay dưới)

---

## ✅ Đã xong

- [x] **FIX-01** Bỏ `using UnityEditorInternal` trong `WallJumpState` (fail build player)
- [x] **FIX-02** `return` sau mỗi `ChangeState` — giữ nguyên độ ưu tiên cũ
- [x] **FIX-03** Sửa `PlayerStateType` truyền sai (Dash/Slide → Fall, WallSlide → WallJump)
- [x] **FIX-04** `Hitbox` dedupe theo `IDamageable` — 1 đòn chỉ trúng 1 lần / mục tiêu
- [x] **FIX-05** `Health` reset trong `OnEnable` (hỗ trợ ObjectPool)
- [x] **FIX-06** `ClimbState` khôi phục `gravityScale` gốc thay vì hardcode `3f`
- [ ] **FIX-07** 🟡 🖥️ Play-test toàn bộ thay đổi — xem mục **"Cần play-test"** cuối file
  - Đã làm (2026-10-15, qua MCP): Unity sinh `.meta` cho `PlayerStates.cs`, `GroundedState.cs`, `AirborneState.cs`; compile 0 lỗi; `SampleScene` validate sạch (0 missing script); tham chiếu trên Player đầy đủ; vào Play không có exception, `PlayerController` khởi tạo đủ state.
  - Còn lại: bạn tự chơi thử theo bảng "Cần play-test" (MCP không test được cảm giác chơi — Editor không chạy frame khi mất focus). Kiểm tra `attackComboCooldown` (scene 0.4 vs code cũ 0.5).
  - Khi dùng MCP: luôn `set_active_instance` về **StickFigure** (đang mở song song Editor Meomeo).
---

## Phase 1 — Nền tảng State Machine (P1)

- [x] **SM-01** Mỗi state tự khai báo `StateType` (abstract property), bỏ tham số constructor.
- [x] **SM-02** Cache instance state trong `PlayerStates` (tạo 1 lần ở `PlayerController.Awake`).
- [x] **SM-03** Hierarchical state: `GroundedState` (Idle, Run, Land) và `AirborneState` (Jump, Fall). Đã thống nhất các điều kiện lệch nhau — chi tiết ở mục "Cần play-test".
- [x] **SM-04** `PlayerStateMachine` chặn `ChangeState` lồng trong Enter/Exit + cảnh báo (Editor) khi đổi state nhiều lần trong 1 frame.
- [ ] **SM-05** `P1` `M` 🖥️ Thêm `HurtState` + `StunState` cho player; nối `IAttackHandler.Cancel()`. Cần gắn `Health`/`Damageable` cho player trong Editor. `→ CB-05`
- [ ] **SM-06** `P2` `M` Tách state machine thành generic `StateMachine<TOwner>` để enemy dùng lại. `→ EN-01`
- [x] **SM-07** Input buffer 0.1s (`PlayerInputHandler.inputBufferTime`, nút được `Consume` khi dùng) + coyote time (`PlayerStats.coyoteTime`).

## Phase 2 — Combat pipeline (P1)

- [x] **CB-01** Bỏ `CurrentAttackData` mutable dùng chung. Tham số `int` của animation event = bước đòn của clip (`0` = trên không, `1..3` = combo); vũ khí suy ra từ tên hàm event (`FighterAttack`/`SwordAttack`). `WeaponAttackManager` chỉ còn `GetAttackData(weapon, step)` (stateless) → đổi vũ khí / combo nhanh giữa chừng không đọc nhầm data. Đã gán `intParameter` cho 10 clip (gồm 2 clip Gun, chuẩn bị cho CB-03).
- [x] **CB-02** `WeaponAttackDataSO`: `attackDataList` → `comboSteps` + field riêng `airAttack` (đã migrate 3 asset). Xoá `AttackComboIndex`; `MeleeAttackHandler` lấy số đòn combo từ `comboSteps.Count` thay vì hằng `3`.
- [x] **CB-03** Đạn nhận `AttackData` từ người bắn: `Fire(int attackStep)` tra data qua `WeaponAttackManager`, roll crit lúc bắn, `Bullet.Launch(dir, data)`. Bỏ field `attackData` trên prefab `bullet`. Bắn trên không giờ dùng `GunAirAttackData` (trước đây không được dùng). `Bullet` cũng tự recycle sau `lifeTime`.
- [ ] **CB-04** `P1` `M` 🖥️ Shield chặn damage thật (cần xem layout prefab enemy).
- [ ] **CB-05** 🟡 `P1` `M` Hệ effect thống nhất.
  - Đã làm: `BurnEffect` tự nghe `Damageable.OnDamaged` khi `effectType == Burn` (trước đây không ai gọi); xoá `IEffectReceiver` không dùng.
  - Còn lại: `Freeze` chưa có; `KnockbackEffect` vẫn dựa vào `knockbackForce > 0` thay vì effect; cân nhắc `EffectSO` 🖥️.
- [ ] **CB-06** `P1` `S` 🖥️ Thống nhất kiểu damage (`int`/`float`). Đổi kiểu field serialize cần kiểm tra lại asset trong Editor.
- [x] **CB-07** Crit roll 1 lần lúc tạo `AttackData` (`isCritical`, `FinalDamage`) — mọi mục tiêu trúng cùng kết quả; sửa lỗi crit chance 0 vẫn có thể crit.
- [ ] **CB-08** `P2` `S` Faction/team — làm khi enemy bắt đầu tấn công (hiện đã tách bằng layer).
- [x] **CB-09** `Hitbox` dùng `OverlapCircle`/`CircleCast` với `ContactFilter2D` + List tái sử dụng; gizmo CircleCast vẽ theo hướng mặt.
- [ ] **CB-10** `P2` `S` I-frame sau khi bị đánh (player). `→ SM-05`

## Phase 3 — Decoupling & dependency (P1)

- [x] **DC-01** `WeaponManager.OnWeaponChanged` (event cục bộ, có payload) thay cho `ObserverService` global. Đổi sang cùng vũ khí không bắn event.
- [ ] **DC-02** `P1` `M` 🖥️ Tách popup damage khỏi `Damageable` → `DamagePopupSpawner`. Cần gắn component mới vào prefab enemy.
- [ ] **DC-03** `P1` `M` 🖥️ `PrefabRegistrySO` thay cho prefab trong `Bootstrap`. Cần tạo asset + gán Inspector.
- [x] **DC-04** Dọn `Bootstrap`: bỏ `using UnityEngine.WSA`, `curLanguage`, `levelColorOffset`, `levelColorMap`.
  - Giữ lại `SetResolution(720)` và các `AnimationCurve` (framework `PopupBase` đang dùng `bounceCurve`) — quyết định khi làm GS-01.
- [ ] **DC-05** 🟡 `P1` `M` Giảm trách nhiệm `PlayerController`.
  - Đã làm: cheat F1–F3 bọc `UNITY_EDITOR || DEVELOPMENT_BUILD` và dùng Input System; `canDash`/handler chuyển thành property + method (`UseDash`, `ResetDash`, `MeleeHandler`, `GunHandler`); đổi tên `weaponManager` → `WeaponManager`, `weaponAttackManager` → `WeaponAttackManager`; logic thu nhỏ collider gom vào `ShrinkColliderHeight`.
  - Còn lại: tách `DebugCheatPanel` thành component riêng 🖥️; tách `PlayerMotor` nếu `PlayerController` tiếp tục phình.
- [x] **DC-06** `GhostSpriteSpawner` cache trong `PlayerController.GhostSpawner`.

## Phase 4 — Animation (P2)

- [x] **AN-01** Cache hash theo key `(state, weapon)` / `(combo, weapon)`, không ghép string mỗi lần Play; log lỗi khi Animator thiếu state.
- [x] **AN-02** `IsDone()` chỉ tính khi animator đã thực sự vào state vừa Play (state không tồn tại → coi như xong, không treo).
- [x] **AN-03** `AttackState` null-guard `attackHandler`.
- [ ] **AN-04** `P3` `L` 🖥️ Animator Override Controller theo vũ khí thay vì đặt tên clip `{state}_{weapon}`.

## Phase 5 — Enemy & content scale (P2)

- [ ] **EN-01** `P2` `L` 🖥️ Enemy cơ bản: `EnemyController` + state machine (Patrol / Chase / Attack / Hurt / Dead). `→ SM-06, CB-05`
- [ ] **EN-02** `P2` `M` 🖥️ `EnemyDataSO` (máu, tốc độ, tầm phát hiện, attack data).
- [ ] **EN-03** `P2` `M` `Death` hỗ trợ pool (`Recycle` thay vì `Destroy`) — làm cùng EN-01 khi có spawn enemy.
- [ ] **EN-04** `P2` `M` 🖥️ `WeaponDataSO` gộp (type, attack data, animation set, hitbox, projectile). `→ CB-02, AN-01`
- [ ] **EN-05** 🟡 `P2` `S` `PlayerStats`.
  - Đã làm: sửa tooltip copy-paste sai; xoá `criticalDamageMultiplier` (không dùng — hệ số crit nằm ở `AttackDataSO`); combo window đọc từ `attackComboCooldown` thay vì hardcode `0.5f`.
  - Còn lại: chuyển thành `PlayerStatsSO` 🖥️.

## Phase 6 — Game structure (P2)

- [ ] **GS-01** `P2` `M` Scene flow: Bootstrap scene → Menu → Level, service lifetime rõ ràng.
- [ ] **GS-02** `P2` `M` Game flow: level start / win / fail / respawn.
- [ ] **GS-03** `P2` `M` HUD: thanh máu player, vũ khí hiện tại.
- [ ] **GS-04** `P2` `M` Audio service (SFX đánh/trúng/nhảy, BGM).
- [ ] **GS-05** `P3` `M` Save system (tiến độ level, vũ khí mở khoá).
- [ ] **GS-06** `P3` `S` Camera: SmoothDamp/Cinemachine + dead zone + biên level.

## Phase 7 — Tổ chức project & chất lượng (P1–P2)

- [x] **OR-01** Sắp xếp thư mục qua Editor (GUID giữ nguyên): `PlayerInputHandle.cs` → `PlayerInputHandler.cs`; `Player/Ef/{Burn,Knockback,Stun}Effect` → `Combat/Effect/`; `GhostSpriteSpawner` → `Player/`; `GroundCheck` → `Player/`; `DamagePopup`, `AttackComboIndex` → `Combat/`; `Scripts/Manager` → `Scripts/Health`.
- [x] **OR-02** Dọn asset: xoá `Combat/image.png` (không ai dùng), `blue.png`/`red.png`, file rác `fighter_Idle_0002.png~`.
  - `Assets/fighter_Idle_0002.png` **không phải rác** (3 SpriteRenderer trong `SampleScene` dùng, khác nội dung bản trong Art) → chuyển sang `_GamePlay/Art/`.
- [x] **OR-03** Xoá `Weapon.cs`, `IEffectReceiver.cs` (không ai dùng) và code comment-out trong các state. Giữ `Enemy.cs` vì đang gắn trong `SampleScene`.
- [x] **OR-04** Pin `com.coplaydev.unity-mcp` vào commit `85c101f5` (đúng bản đang dùng trong `packages-lock.json`).
- [ ] **OR-05** `P2` `M` 🖥️ Namespace + asmdef. DOTween/ACTk nằm trong `Plugins` (firstpass) nên cần tạo asmdef cho chúng trong Editor trước. `→ OR-01`
- [ ] **OR-06** `P2` `M` EditMode test cho logic thuần. `→ OR-05`
- [ ] **OR-07** `P2` `S` `CLAUDE.md` / README: kiến trúc, quy ước tên clip animation, cách thêm vũ khí / enemy.
- [ ] **OR-08** `P3` `M` CI build (GameCI).

---

## Cần play-test (FIX-07)

Các thay đổi hành vi có chủ đích — kiểm tra cảm giác chơi, chỉnh lại nếu không đúng ý:

| Thay đổi | Trước | Sau |
|---|---|---|
| Input buffer | Nút chỉ có hiệu lực trong frame bấm | Giữ 0.1s cho tới khi được dùng (vd: bấm nhảy hơi sớm trước khi chạm đất) |
| Coyote time | Rời mép là mất nhảy | Vẫn nhảy được trong 0.1s sau khi rời mép |
| Combo window | Hardcode 0.5s | `stats.attackComboCooldown` — scene đang là **0.4s** (combo hơi gắt hơn, chỉnh lại trong Inspector nếu cần) |
| Idle khi mất mặt đất | Kẹt animation Idle giữa không trung | Chuyển sang Fall |
| Run trên dốc xuống | `velocity.y < 0` là Fall → có thể nháy Fall/Land | Chỉ Fall khi không chạm đất |
| Land | Khoá 0.1s, không làm gì được | Jump/attack/dash/slide huỷ được animation tiếp đất |
| Run + phím lên ở thang | Không leo | Leo (giống Idle) |
| Dash khi đang chạy | Không check chạm đất | Phải chạm đất (trên không đi theo luật dash trên không) |
| `canDash` | Chỉ hồi khi qua Land | Hồi ở mọi state mặt đất (vd: rời thang về Idle) |
| Wall slide + bấm hướng ngược & nhảy cùng lúc | Rơi, mất cú nhảy | Ra wall jump |
| Đổi sang đúng vũ khí đang cầm | Huỷ attack + phát lại animation | Không làm gì |
| Crit | Roll riêng cho từng mục tiêu | Roll 1 lần / đòn |
| Bắn súng trên không | Dùng `GunAttackData` (crit 5%) | Dùng `GunAirAttackData` (dmg 10, **crit 0**) — chỉnh asset nếu muốn giống bắn thường |

---

## Thứ tự đề xuất (phần còn lại)

```
FIX-07 (play-test thủ công)
  → CB-01 → CB-02 → CB-03 → CB-04 → CB-05 → SM-05 → CB-10
  → DC-02, DC-03
  → OR-05 → OR-06
  → EN-04 → EN-01..03 → Phase 6
```
