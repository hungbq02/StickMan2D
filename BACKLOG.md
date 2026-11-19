# StickFigure — Backlog kỹ thuật

> Mục tiêu: dọn nợ kỹ thuật hiện tại và dựng nền để scale (nhiều vũ khí, nhiều loại enemy, nhiều level) mà không phải viết lại.
> Cập nhật lần cuối: 2026-11-03 (Phase 8 enemy roster & boss)

**Ký hiệu**
- Ưu tiên: `P0` làm ngay · `P1` trước khi thêm content mới · `P2` khi scale · `P3` nice-to-have
- Effort: `S` < 2h · `M` nửa ngày – 1 ngày · `L` 2–3 ngày
- 🖥️ = cần mở Unity Editor (prefab, animation clip, Inspector, Play mode)
- `→ X` = phụ thuộc task X
- 🟡 = đã làm một phần (phần còn lại ghi ngay dưới)






## 📊 Tiến độ

**63 xong · 3 làm một phần · 5 còn lại** (tổng 71)

| Phase | Xong | Một phần 🟡 | Còn lại |
|---|---|---|---|
| Sửa lỗi | 11 | 07 | — |
| 1. State machine ✅ | 7 | — | — |
| 2. Combat ✅ | 10 | — | — |
| 3. Decoupling ✅ | 6 | — | — |
| 4. Animation | 3 | — | 04 |
| 5. Enemy ✅ | 5 | — | — |
| 6. Game structure | 2 | 02 | 01, 04, 05 |
| 7. Tổ chức project | 6 | 05 | 08 |
| 8. Enemy roster & boss ✅ | 9 | — | — |
| 9. Asset & visual | 3 | 04, 05, 06 | — |
| 10. Cảm giác chơi (mới) | IN-01, AS-07, CB-11, CB-12 | — | EQ-02, MAP-02, AS-08 |

---

## ✅ Đã xong

- [x] **FIX-01** Bỏ `using UnityEditorInternal` trong `WallJumpState` (fail build player)
- [x] **FIX-02** `return` sau mỗi `ChangeState` — giữ nguyên độ ưu tiên cũ
- [x] **FIX-03** Sửa `PlayerStateType` truyền sai (Dash/Slide → Fall, WallSlide → WallJump)
- [x] **FIX-04** `Hitbox` dedupe theo `IDamageable` — 1 đòn chỉ trúng 1 lần / mục tiêu
- [x] **FIX-05** `Health` reset trong `OnEnable` (hỗ trợ ObjectPool)
- [x] **FIX-06** `ClimbState` khôi phục `gravityScale` gốc thay vì hardcode `3f`
- [x] **FIX-08** Rung khi di chuyển / rơi: Rigidbody2D của Player + bia tập `interpolation = None` (physics 50Hz) trong khi camera SmoothDamp mỗi frame trong `LateUpdate` => nhân vật giật theo bước physics. Bật `Interpolate` (prefab `MeleeEnemy` đã bật sẵn).
- [x] **FIX-09** Súng: animation và đạn không khớp.
  - Prefab `bullet` lưu ở trạng thái **inactive** (từ commit đầu) + nhánh `Instantiate` của `ObjectPool.Spawn` không `SetActive(true)` => chỉ viên lấy từ pool hiện ra, bắn khi viên trước còn bay thì đạn **vô hình, không damage**. Sửa cả prefab và `ObjectPool`.
  - Điểm sinh đạn lệch đầu nòng (0.16 trước / 0.13 dưới) => đo đầu nòng từ sprite frame bắn: `SpawnPointBullet` (1.31, −0.25) cho `pistol_shot_0064`, thêm `AirSpawnPointBullet` (1.37, −0.38) cho `pistol_air_attack_0062`; đuôi đạn trùng đầu nòng.
  - Đạn sinh từ code cùng frame `Play` (bỏ event `Fire` ở t=0 — không chắc bắn lại khi phát lại cùng state); animation đòn đánh luôn phát lại từ đầu; `IsDone` không báo xong ở frame vừa `Play`.
  - Bắn trên không giữ tư thế hết clip (0.2s) rồi mới Fall (trước: đổi Fall ngay khi rơi => tư thế bắn chỉ 1 frame; đang bay lên thì kẹt tư thế tới đỉnh).
  - Hướng đạn tính theo world (`Space.World`), bỏ kiểu nhân hướng 2 lần.
- [x] **FIX-10** Camera không theo kịp khi rơi (dead zone Y 0.75 + trễ 0.12s, player rơi tới ~40 unit/s). `CameraFollow`: rơi càng nhanh càng nhìn trước xuống dưới (tối đa 2u) và trục Y bám nhanh hơn (0.12s → 0.04s); thêm safe frame — player luôn cách mép khung ≥ 20% nửa khung, vượt thì camera bắt kịp ngay. Test rơi 21u: player luôn ở 40–53% chiều cao màn hình.
- [x] **FIX-11** Player trượt dài trên đất khi bị enemy đánh: collider `NoFriction` + Idle / Land / Hurt / Stun không hãm vận tốc ngang => knockback 5 u/s giữ mãi. Thêm `PlayerStatsSO.groundDeceleration` (40 u/s²) + `PlayerController.BrakeOnGround()` cho 4 state trên => bị đẩy lùi ~0.26u rồi dừng.
- [x] **FIX-12** Rà animation: 47 clip đều đã gắn state. Thống nhất loop — `jump` Sword/Gun, `land`/`fall` Gun, `death` ×3 chuyển sang phát 1 lần (trước: jump Gun/Sword lặp 3 frame khi bay lên => giật); xoá state rỗng `New State`, default = `idle_Fighter`. Cú đá Fighter (`fighterAirHitbox` r=0.5 tại (0.66, −1.11)): phạm vi ôm bàn chân + ống chân ở frame `fighter_air_attack_0062`, damage 1 lần — đã test, giữ nguyên.
  - Chưa dùng: `walk_*` (không có state đi bộ — chỉ cần nếu muốn đi chậm theo joystick), `death_*` → đã dùng ở GS-02 (DeadState), `IdleVFX` (demo của asset pack Pixel Art).
- [ ] **FIX-07** 🟡 🖥️ Play-test toàn bộ thay đổi — xem mục **"Cần play-test"** cuối file
  - Đã làm (2026-10-15, qua MCP): Unity sinh `.meta` cho `PlayerStates.cs`, `GroundedState.cs`, `AirborneState.cs`; compile 0 lỗi; `SampleScene` validate sạch (0 missing script); tham chiếu trên Player đầy đủ; vào Play không có exception, `PlayerController` khởi tạo đủ state.
  - Còn lại: bạn tự chơi thử theo bảng "Cần play-test" (MCP không test được cảm giác chơi — Editor không chạy frame khi mất focus). Kiểm tra `attackComboCooldown` (scene 0.4 vs code cũ 0.5).
  - Khi dùng MCP: luôn `set_active_instance` về **StickFigure** (đang mở song song Editor Meomeo).
---

## Phase 1 — Nền tảng State Machine (P1) ✅

- [x] **SM-01** Mỗi state tự khai báo `StateType` (abstract property), bỏ tham số constructor.
- [x] **SM-02** Cache instance state trong `PlayerStates` (tạo 1 lần ở `PlayerController.Awake`).
- [x] **SM-03** Hierarchical state: `GroundedState` (Idle, Run, Land) và `AirborneState` (Jump, Fall). Đã thống nhất các điều kiện lệch nhau — chi tiết ở mục "Cần play-test".
- [x] **SM-04** `PlayerStateMachine` chặn `ChangeState` lồng trong Enter/Exit + cảnh báo (Editor) khi đổi state nhiều lần trong 1 frame.
- [x] **SM-05** `HurtState` + `StunState` cho player. `PlayerController` nghe `Damageable.OnDamaged` → Hurt (khoá điều khiển `stats.hurtDuration` = 0.3s, knockback vẫn tác dụng) → Stun nếu đòn có choáng (tới khi `StunEffect` hết) → Idle/Fall. `AttackState.Exit` huỷ attack handler khi bị ngắt. Player được gắn `StunEffect` + `KnockbackEffect`; dùng clip `hit_{weapon}` có sẵn.
- [x] **SM-06** Lõi state machine tách thành `StateMachine<TState>` + `IState` (`Scripts/StateMachine/`, class thuần, owner tự gọi Update/FixedUpdate; giữ guard đổi state lồng + cảnh báo nhiều lần / frame). `PlayerStateMachine` chỉ còn bọc `StateMachine<State>`, API không đổi.
- [x] **SM-07** Input buffer 0.1s (`PlayerInputHandler.inputBufferTime`, nút được `Consume` khi dùng) + coyote time (`PlayerStats.coyoteTime`).

## Phase 2 — Combat pipeline (P1) ✅

- [x] **CB-01** Bỏ `CurrentAttackData` mutable dùng chung. Tham số `int` của animation event = bước đòn của clip (`0` = trên không, `1..3` = combo); vũ khí suy ra từ tên hàm event (`FighterAttack`/`SwordAttack`). `WeaponAttackManager` chỉ còn `GetAttackData(weapon, step)` (stateless) → đổi vũ khí / combo nhanh giữa chừng không đọc nhầm data. Đã gán `intParameter` cho 10 clip (gồm 2 clip Gun, chuẩn bị cho CB-03).
- [x] **CB-02** `WeaponAttackDataSO`: `attackDataList` → `comboSteps` + field riêng `airAttack` (đã migrate 3 asset). Xoá `AttackComboIndex`; `MeleeAttackHandler` lấy số đòn combo từ `comboSteps.Count` thay vì hằng `3`.
- [x] **CB-03** Đạn nhận `AttackData` từ người bắn: `Fire(int attackStep)` tra data qua `WeaponAttackManager`, roll crit lúc bắn, `Bullet.Launch(dir, data)`. Bỏ field `attackData` trên prefab `bullet`. Bắn trên không giờ dùng `GunAirAttackData` (trước đây không được dùng). `Bullet` cũng tự recycle sau `lifeTime`.
- [x] **CB-04** Khiên chặn damage thật: `Shield` không còn là `IDamageable` riêng (trước đây hitbox trúng cả khiên lẫn thân => trừ máu cả 2) mà là `IDamageBlocker` đăng ký với `Damageable` của chủ. Đòn từ phía có khiên (hoặc mọi phía nếu `blockAllDirections`) bị khiên hấp thụ trọn — chủ không mất máu / knockback / effect; khiên trừ `FinalDamage`, vỡ thì tắt. `Damageable.OnBlocked` cho VFX/SFX.
  - Chưa có enemy nào gắn khiên — gắn: child object + `Shield` (tự thêm `Health`), đặt lệch về phía cần chặn. Khiên vỡ tự hồi khi chủ được Spawn lại từ pool (EN-03).
- [x] **CB-05** Hệ effect thống nhất: base `StatusEffect` (nghe `Damageable.OnDamaged`, làm mới thời gian khi trúng lại, luôn dọn trạng thái khi object tắt — trước đây `IsStunned` có thể kẹt `true`). `StunEffect`, `BurnEffect` kế thừa; thêm `FreezeEffect` (khoá vị trí, dừng animator, tô màu băng). Bỏ `AttackEffect.Knockback` (không ai xử lý; knockback luôn theo `knockbackForce`) — `FighterAirAttackData` chuyển về `None`, giá trị enum giữ cố định. Gắn Stun/Burn/Freeze cho 3 enemy trong `SampleScene`.
  - Để sau: `EffectSO` (tham số effect theo asset) khi có nhiều biến thể; burn tick hiện cố định 5 dmg/1s trên component.
- [x] **CB-06** Damage/máu thống nhất `int`: `Health` (`maxHealth`, `Current`, `TakeDamage`, `Heal`, `OnHealthChanged`) và `BurnEffect.damagePerTick` chuyển từ `float` sang `int`. Giá trị serialize cũ (100, 5) đọc lại đúng, không cần sửa scene.
- [x] **CB-07** Crit roll 1 lần lúc tạo `AttackData` (`isCritical`, `FinalDamage`) — mọi mục tiêu trúng cùng kết quả; sửa lỗi crit chance 0 vẫn có thể crit.
- [x] **CB-08** Faction: enum `Team` (Neutral / Player / Enemy) trên `Damageable`; `AttackData.sourceTeam` do người đánh gán (player: `PlayerAnimationAttack`, đạn mang theo `AttackData`). `Damageable` bỏ qua đòn cùng phe; Neutral nhận/gây damage cho mọi phe. Scene: Player = `Player`, 3 enemy = `Enemy`. Layer vẫn giữ để lọc va chạm.
- [x] **CB-09** `Hitbox` dùng `OverlapCircle`/`CircleCast` với `ContactFilter2D` + List tái sử dụng; gizmo CircleCast vẽ theo hướng mặt.
- [x] **CB-10** I-frame: `Damageable.SetInvulnerable(duration)` / `IsInvulnerable` (bỏ qua mọi đòn, reset khi object bật lại từ pool). Player bật `stats.invincibilityDuration` = 0.8s mỗi lần trúng đòn; `InvulnerabilityBlink` nhấp nháy sprite trong lúc bất tử. DoT (Burn) trừ thẳng `Health` nên vẫn tác dụng.

## Phase 3 — Decoupling & dependency (P1) ✅

- [x] **DC-01** `WeaponManager.OnWeaponChanged` (event cục bộ, có payload) thay cho `ObserverService` global. Đổi sang cùng vũ khí không bắn event.
- [x] **DC-02** Popup damage tách khỏi `Damageable` → component `DamagePopupSpawner` (nghe `OnDamaged`, có `offset`). `Damageable` không còn phụ thuộc `Bootstrap`. Đã gắn cho Player + 3 enemy (giữ hành vi cũ).
- [x] **DC-03** `PrefabRegistrySO` (`Assets/_GamePlay/Data/PrefabRegistry.asset`: damagePopup, ghostSprite, bullet). `Bootstrap` chỉ giữ tham chiếu registry và `Service.Set` nó; `DamagePopupSpawner`, `GhostSpriteSpawner`, `PlayerAnimationAttack` dùng `Service.Get<PrefabRegistrySO>()`. Sửa luôn: prefab `Bootstrap` có ref popup hỏng (chỉ chạy được nhờ override trong scene) — override cũ đã gỡ.
- [x] **DC-04** Dọn `Bootstrap`: bỏ `using UnityEngine.WSA`, `curLanguage`, `levelColorOffset`, `levelColorMap`.
  - Giữ lại `SetResolution(720)` và các `AnimationCurve` (framework `PopupBase` đang dùng `bounceCurve`) — quyết định khi làm GS-01.
- [x] **DC-05** Giảm trách nhiệm `PlayerController`.
  - Cheat F1–F3 tách thành component `DebugCheatPanel` (`Scripts/Debug/`, GameObject riêng trong scene; logic bọc `UNITY_EDITOR || DEVELOPMENT_BUILD`, class giữ lại để scene không "missing script" ở release). `PlayerController` không còn phụ thuộc Input System.
  - Trước đó: `canDash`/handler thành property + method, đổi tên field, gom logic thu collider.
  - `PlayerMotor`: chưa tách — `PlayerController` hiện ~170 dòng, chỉ tách khi phình tiếp.
- [x] **DC-06** `GhostSpriteSpawner` cache trong `PlayerController.GhostSpawner`.

## Phase 4 — Animation (P2)

- [x] **AN-01** Cache hash theo key `(state, weapon)` / `(combo, weapon)`, không ghép string mỗi lần Play; log lỗi khi Animator thiếu state.
- [x] **AN-02** `IsDone()` chỉ tính khi animator đã thực sự vào state vừa Play (state không tồn tại → coi như xong, không treo).
- [x] **AN-03** `AttackState` null-guard `attackHandler`.
- [ ] **AN-04** `P3` `L` 🖥️ Animator Override Controller theo vũ khí thay vì đặt tên clip `{state}_{weapon}`.

## Phase 5 — Enemy & content scale (P2) ✅

- [x] **EN-01** Enemy cơ bản `EnemyController` + `StateMachine<EnemyState>`: Patrol (qua lại quanh điểm xuất phát, nghỉ ở 2 đầu) → Chase (thấy player trong `detectRange`, cùng tầng) → Attack (đứng lại, vung đòn có tô đỏ báo hiệu, damage qua `Hitbox`, team Enemy) → Hurt khi trúng đòn (quay mặt về phía bị đánh, ưu tiên đuổi lại). Choáng / đóng băng => đứng yên. Dò mép vực + tường để không rơi / đâm tường. Prefab `Prefabs/Enemy/MeleeEnemy` (đặt 1 con ở x=19 trong `SampleScene`; 3 bia tập cũ giữ nguyên).
  - Chưa có art: dùng sprite fighter tô hồng; `Dead` state do `Death` xử lý (tắt / về pool). Target tìm bằng `FindObjectOfType<PlayerController>` — đổi sang service khi làm GS-02.
- [x] **EN-02** `EnemyDataSO` (máu, tốc độ tuần tra/đuổi, tầm phát hiện/bỏ đuổi, chênh độ cao, `AttackDataSO` + tầm/vung/hồi/cooldown, hurt). `Health.SetMaxHealth`. Asset: `Data/Enemy/MeleeEnemyData`, `DataCombat/Enemy/EnemyMeleeAttackData` (10 dmg, knockback 5).
- [x] **EN-03** `Death`: object spawn từ `ObjectPool` thì `Recycle` về pool (Health / Damageable / StatusEffect tự reset khi Spawn lại); object đặt sẵn trong scene giữ hành vi cũ. `Damageable.OnRespawned` (bắn ở `OnEnable`) → `Shield` đã vỡ tự bật lại đầy máu.
- [x] **EN-04** `WeaponAttackDataSO` → `WeaponDataSO` (GUID giữ nguyên, asset đổi tên `*WeaponData`): thêm `attackStyle` (Melee / Ranged) + `projectile`. `AttackState` chọn handler theo `attackStyle` thay vì `switch WeaponType`; `Fire` lấy đạn từ `projectile` (fallback registry). `WeaponAttackManager.GetWeaponData(type)`.
  - Chưa gộp: animation set (→ AN-04), hitbox (là child object trong scene, `PlayerAnimationAttack` giữ ref). Thêm vũ khí mới vẫn cần thêm giá trị `WeaponType` + clip `{state}_{weapon}`.
- [x] **EN-05** `PlayerStats` → `PlayerStatsSO` (asset `Data/PlayerStats`, giá trị copy nguyên từ scene). `PlayerController.stats` là property trỏ vào asset nên code gọi `player.stats.X` không đổi. Trước đó: sửa tooltip sai, xoá `criticalDamageMultiplier`, combo window đọc từ `attackComboCooldown`.
  - Chỉnh chỉ số player giờ sửa asset `PlayerStats` (thay đổi trong Play mode được giữ lại sau khi thoát Play).

## Phase 6 — Game structure (P2)

- [ ] **GS-01** `P2` `M` Scene flow: Bootstrap scene → Menu → Level, service lifetime rõ ràng.
- [ ] **GS-02** 🟡 `P2` `M` Game flow: level start / win / fail / respawn.
  - Đã làm (tạm thời): player hết máu → `DeadState` (phát `death_{weapon}`, khoá điều khiển, không nhận thêm damage) → sau `PlayerStatsSO.respawnDelay` (2s) hồi sinh tại điểm xuất phát (`PlayerController.Respawn`: đầy máu, gỡ status effect, xoá input buffer, i-frame). Player không còn bị tắt khi chết; enemy bỏ qua target đã chết. `SetSpawnPoint` sẵn cho checkpoint.
  - Còn lại: điều kiện thắng màn, giới hạn mạng / màn thua, checkpoint.
- [x] **GS-03** HUD (prefab `Prefabs/UI/HUD`, theo skill `ui-canvas-layout` cho màn ngang): Canvas Overlay sortingOrder 0, `CanvasScaler` 1920×1080 + `CanvasScalerMatchHelper`, `SafeAreaPanel`; góc trên trái thanh máu (fill chạy mượt, màu đỏ→vàng→xanh theo %, chữ `hiện/tối đa`) + tên vũ khí. `PlayerHUD` nghe `Health.OnHealthChanged` / `WeaponManager.OnWeaponChanged`; `Health.OnEnable` cũng bắn `OnHealthChanged` (respawn / pool).
- [ ] **GS-04** `P2` `M` Audio service (SFX đánh/trúng/nhảy, BGM).
- [ ] **GS-05** `P3` `M` Save system (tiến độ level, vũ khí mở khoá).
- [x] **GS-06** `CameraFollow`: `SmoothDamp` (độc lập FPS, `smoothTime` 0.12s, 0 = bám cứng) thay `Lerp` theo frame; dead zone (0.5 × 0.75); giới hạn trong biên level (`bounds`, tính theo kích thước camera ortho, level nhỏ hơn khung hình thì căn giữa); `SnapToTarget()` cho lúc bắt đầu / respawn. Biên mặc định khớp `SampleScene` (x −7.7…24.5, y −2.7…25.3).

## Phase 7 — Tổ chức project & chất lượng (P1–P2)

- [x] **OR-01** Sắp xếp thư mục qua Editor (GUID giữ nguyên): `PlayerInputHandle.cs` → `PlayerInputHandler.cs`; `Player/Ef/{Burn,Knockback,Stun}Effect` → `Combat/Effect/`; `GhostSpriteSpawner` → `Player/`; `GroundCheck` → `Player/`; `DamagePopup`, `AttackComboIndex` → `Combat/`; `Scripts/Manager` → `Scripts/Health`.
- [x] **OR-02** Dọn asset: xoá `Combat/image.png` (không ai dùng), `blue.png`/`red.png`, file rác `fighter_Idle_0002.png~`.
  - `Assets/fighter_Idle_0002.png` **không phải rác** (3 SpriteRenderer trong `SampleScene` dùng, khác nội dung bản trong Art) → chuyển sang `_GamePlay/Art/`.
- [x] **OR-03** Xoá `Weapon.cs`, `IEffectReceiver.cs` (không ai dùng) và code comment-out trong các state. Giữ `Enemy.cs` vì đang gắn trong `SampleScene`.
- [x] **OR-04** Pin `com.coplaydev.unity-mcp` vào commit `85c101f5` (đúng bản đang dùng trong `packages-lock.json`).
- [ ] **OR-05** 🟡 `P2` `M` Namespace + asmdef.
  - Đã làm: asmdef `StickFigure.Gameplay` cho toàn bộ `Assets/_GamePlay` (ref Input System, TMP, UGUI, `ACTk.Runtime`; DOTween dll tự ref — code chỉ dùng API core, không cần module). Đã kiểm tra compile Editor + **player Android** (`CompilePlayerScripts`), scene không mất script.
  - Còn lại: namespace — hoãn (C# 9 không có file-scoped namespace => phải bọc + thụt lề ~60 file, lợi ích thấp với 1 game). Bật `FIREBASE` / `PURCHASE` sau này cần thêm reference SDK vào asmdef.
- [x] **OR-06** EditMode test (`Assets/Tests/EditMode`, asmdef `StickFigure.Tests.EditMode`): 19 test — `StateMachine` (thứ tự Enter/Exit, chặn đổi state lồng, null, Stop), `AttackData` (crit, FinalDamage, team mặc định, copy từ SO), `Health` / `Damageable` / `Shield` (clamp, chết 1 lần, heal, team, i-frame, khiên theo hướng), `WeaponAttackManager` (bước combo, đòn trên không, ngoài phạm vi). Chạy: Test Runner → EditMode, hoặc MCP `run_tests`.
- [x] **OR-07** `CLAUDE.md` ở root: stack, cấu trúc thư mục, kiến trúc (player state machine, combat pipeline, damage, effect, enemy, service/prefab), quy ước (tên clip, tham số animation event, team/layer, cheat), cách thêm combo / vũ khí / enemy / effect, quy trình MCP + commit theo task, skill name map.
- [ ] **OR-08** `P3` `M` CI build (GameCI).

## Phase 8 — Enemy roster & boss (P2) ✅

Art: 5 sprite sheet AI-gen của user trong `Art/EnemyConcepts` (không commit) → lính rìu **Axeman** (melee), cung thủ **Stalker** (ranged), hiệp sĩ đen **Knight** (elite), quỷ đỏ **Demon** (mini-boss), **Unmaker** (boss cuối).

- [x] **EN-06** 🖥️ Pipeline art + import:
  - `Tools/ArtPipeline/build.py` (Python: numpy, scipy, pillow) dựng lại toàn bộ frame từ sheet gốc: tách nền ca rô "vẽ chết" (alpha = 255 toàn ảnh) bằng dựng lại nền theo chu kỳ ô + color-to-alpha (giữ viền mượt, glow / vệt chém bán trong suốt), xoá nhãn / số khung / khung ô / mũi tên / chữ THUD-ROAR, tách frame dính nhau, căn mọi frame về cùng ô + chân trên 1 đường đáy (neo theo mặt / hông / khớp hình). Ra `Art/Enemies/<Name>/` 361 PNG + `<id>_manifest.json`.
  - Menu **Tools/Enemies/Import Enemy Art** (`Editor/EnemyArtImporter`): set sprite (PPU theo nhân vật, pivot đáy-giữa, bilinear, không mipmap), tạo clip `{anim}_{Name}` + `Anim/Enemies/<Name>/Animator<Name>.controller` (71 clip). Chạy lại an toàn (giữ GUID).
  - Lỗi của sheet gốc đã xử lý (ghi trong `build.py`): Axeman nhãn "Death"/"Slash" bị đặt ngược; mỗi sheet có watermark ngôi sao đè frame cuối 1 animation → bỏ frame đó (Axeman taunt, Stalker stealthOut, Knight rageWalk, Demon enrage, Unmaker taunt); Demon "Braced sound attack" và Stalker "Stealth Enter" thứ 2 vẽ hỏng → không dùng; Knight chỉ có 1 frame idle / rageIdle.
  - Giới hạn: frame AI vẽ không đồng nhất giữa các frame (chi tiết giáp, trượng biến mất...) nên vẫn có nháy nhẹ; Unmaker sheet chỉ 1024px (frame ~110px) → PPU 35, phóng to sẽ mờ. Bản đẹp hơn cần gen lại sheet theo spec (lưới cố định, PNG trong suốt thật, không chữ).
- [x] **EN-07** Enemy có animation + đòn đánh theo data:
  - `EnemyAnimator` (child Visual, cạnh Animator + SpriteRenderer): `Play("walk")` → state `walk_{animSet}`, thiếu state thì bỏ qua (enemy không có art vẫn chạy bằng timer), `GetLength`, `IsDone`.
  - `EnemyAttackSO` (`Data/Enemy Attack`): clip báo hiệu / đòn, `telegraphTime` (+ tô màu), `hitTimes[]` (nhiều mốc trúng / đòn), `activeDuration` (0 = theo clip), `recoveryTime`, `cooldown` riêng, tầm `min/maxRange`, `weight`, `min/maxPhase`, damage + tên hitbox, di chuyển trong đòn (Lunge / Retreat / Teleport). `EnemyAttackSelector` chọn ngẫu nhiên theo weight trong các đòn hợp lệ.
  - `EnemyDataSO`: `attacks` thay 5 field đòn cũ, `attackInterval`, `keepAwayDistance` (đánh xa: lùi khi bị áp sát), `flinch` (Always / NotWhileAttacking / Never), `corpseLinger`, `phases` (ngưỡng máu, animation chuyển phase, bất tử lúc chuyển, idle thay thế, hệ số tốc độ / nhịp đánh, màu), tên animation idle / walk / run / hit / death / taunt.
  - State mới: `EnemyDeadState` (tắt va chạm, phát `death`, mờ dần; `Death.DespawnDelay` chờ hết rồi mới về pool), `EnemyPhaseChangeState`, `EnemyTauntState` (hạ được player). Attack = Telegraph → Active → Recovery; Hurt phát `hit`. `IEnemyAttackHandler` cho component khác nhận mốc trúng (đạn, vùng nguy hiểm...).
  - `Death`: thêm `despawnDelay` (mặc định 0 = như cũ). `MeleeEnemy`: tạo `Data/Enemy/Attacks/MeleeEnemySwing` từ giá trị cũ (vung 0.45s tô đỏ, 10 dmg, hồi 0.35s, cooldown 1.2s, tầm 1.2).
  - Test: +7 EditMode (`EnemyAttackSelectorTests`: tầm, phase, cooldown, weight, tầm tiếp cận, phase theo máu, độ dài pha ra đòn) — 26/26 pass.
- [x] **EN-08** 🖥️ Axeman + Knight:
  - Prefab `Prefabs/Enemy/Axeman`, `Knight` (copy từ `MeleeEnemy`, gốc toạ độ ở chân, Animator + `EnemyAnimator` cùng object với SpriteRenderer, mỗi đòn 1 hitbox con: `SmashHitbox`, `SpinHitbox`, `ChargeHitbox` / `OverheadHitbox`, `CleaveHitbox`, `CinderHitbox`). Đặt trong `SampleScene`: Axeman x=9, Knight x=14.5 (MeleeEnemy x=19 giữ nguyên).
  - **Axeman** (90 máu, khựng mọi đòn): Slash 10 · Smash (tư thế báo hiệu 0.6s) 18 + choáng 0.4s · Spin 2 nhát × 8 · Charge (báo hiệu 0.7s, lao 9u/s, tầm 2.5–4.2) 14. Dưới 40% máu → enrage (bất tử lúc chuyển, nhanh ×1.25, nhịp ×0.75, ửng đỏ).
  - **Knight** (220 máu, không bị ngắt khi đang ra đòn): Slash 14 · Overhead (báo hiệu 0.7s) 24 + choáng 0.5s · Cleave 14 · Combo 3 nhát × 9 · Cinder Wave 16 + cháy 2s (chỉ phase 2). Dưới 50% → enrage, idle đổi `rageIdle`. Sheet không có animation chết → dùng `stagger` rồi mờ dần.
  - Pipeline: thêm `flip` (lật ngang: Smash của Axeman AI vẽ bổ về sau lưng); frame không thấy mặt (bị tay che) căn khớp hình với frame bên cạnh thay vì tâm khung.
  - Chase lên kế hoạch 1 đòn (random theo weight trong các đòn hết cooldown) rồi tiến tới tầm của đòn đó — trước đây chỉ tiến tới tầm xa nhất nên đòn tầm gần ít khi được dùng. Lunge dừng khi đã áp sát mục tiêu (`lungeStopDistance`), không lao xuyên người.
  - Đã chạy Play mode (`EditorApplication.Step`): Axeman ra đủ Charge → Slash → Smash → Spin, trúng đúng damage; enrage bất tử khi chuyển (đòn trong lúc đó bị bỏ qua); chết phát animation + mờ dần, tắt sau 1.6s. Knight 30s: Slash ×7, Overhead ×4, Cleave ×4, Combo ×3. EditMode 27/27.
- [x] **EN-09** 🖥️ Đạn enemy + Stalker:
  - `EnemyProjectile` (bay thẳng, `pierce` = xuyên mục tiêu mỗi mục tiêu 1 lần, gặp đất / tường thì mất, về pool; cần Rigidbody2D Kinematic + trigger). `EnemyShooter` (`IEnemyAttackHandler`): mỗi mốc trúng của đòn có `projectile` bắn `projectileCount` viên từ `muzzle` theo hướng mặt, tỏa đều `spreadAngle`, hoặc `AboveTarget` rơi xuống quanh mục tiêu (`rainHeight`, `rainWidth`). Damage đạn = `projectileDamage` (tách khỏi `damage` chém).
  - Prefab đạn `Prefabs/Enemy/Projectiles/StalkerArrow` (13u/s), `StalkerArrowCharged` (17u/s, xuyên), `StalkerArrowPierce` (22u/s, xuyên), `StalkerArrowRain` (rơi 9u/s). Sprite từ `Art/Enemies/Stalker/FX`.
  - **Stalker** (70 máu, `keepAwayDistance` 3.5): Quick 8 · Charged 16 (xuyên) · Triple 3 × 7 · Piercing 12 (xuyên) · Rain 3 cụm tên rơi × 10 · Dash lùi khi player áp sát < 2.2u. Báo hiệu bằng tư thế `aim`. Đặt trong `SampleScene` x=23.
  - Chưa dùng: `stealthIn` / `stealthOut` (đòn Teleport hiện ra sau lưng player không hợp với cung thủ — cần kiểu "biến mất rồi hiện ở xa").
  - Đã chạy Play mode với 1 Stalker: bắn đúng damage từng loại (8 / 16 / 12 / 7), đứng yên khi bắn; player áp sát 1.2u → dash lùi 3u rồi bắn tiếp.
- [x] **EN-10** 🖥️ Khung boss:
  - `BossEncounter` (object có trigger, tham chiếu boss trong scene): boss ngủ (EnemyController tắt, đứng idle) tới khi player bước vào → bật boss, bật `arenaWalls`, event `Started`. Boss chết → mở tường, `Ended`. Player chết giữa trận → `Ended`, sau `resetDelay` boss về chỗ cũ, đầy máu, phase 0, ngủ lại.
  - `BossHealthBar` trong `Prefabs/UI/HUD` (giữa trên, ẩn mặc định): tên boss, fill đỏ nhảy ngay + vệt sáng chạy theo lượng máu vừa mất. Nghe `BossEncounter.Started/Ended` + `Health.OnHealthChanged`.
  - `GroundHazard` (báo trước: frame đầu mờ nhấp nháy `warnTime` → active: damage 1 lần / mục tiêu trong hộp `size` → về pool; bám mặt đất). `EnemyHazardCaster`: sinh hazard ở mốc trúng — `AtTarget` (± scatter) / `InFront` (hàng lan dần) / `AroundSelf` (2 bên). `SpriteSequence`: phát chuỗi sprite không cần Animator.
  - `EnemySummoner` (triệu hồi qua pool, xen kẽ 2 bên, giới hạn `maxSummons` còn sống, `DespawnAll` khi trận kết thúc). `EnemyGuard` (`IDamageBlocker`: chặn đòn phía trước trong pha ra đòn có `guardFront`).
  - Knight thêm **Ground Rupture** (phase 2): tư thế báo hiệu 0.6s → 4 cột lửa trồi lần lượt phía trước (prefab `Prefabs/Enemy/Hazards/KnightRuptureFlame`), 15 + cháy.
  - Đã chạy Play mode (Knight làm boss thử): ngủ → vào trigger thì thức, thanh máu hiện đúng tên; mất 125 máu → fill 0.43 ngay, vệt sáng chạy dần; phase 2 → Ground Rupture 4 cột lửa, trúng 15; player chết → thanh máu ẩn, 1s sau boss về x gốc đầy máu ngủ lại, vào lại thì trận mới phase 0. Summoner / Guard chạy thử ở EN-11.
- [x] **EN-11** 🖥️ Demon (mini-boss) + Unmaker (boss cuối) — scene thử `Assets/Scenes/BossArena` (copy SampleScene, bỏ enemy thường; 2 đấu trường liền nhau, mỗi cái 1 `BossEncounter` + 2 tường đỏ mờ layer Wall):
  - **Demon "Crimson Brute"** (450 máu, không bị ngắt khi đang ra đòn; đấu trường x 2.5–13.5): Swing 2 nhát × 16 · Smash (báo hiệu 0.6s) 24 + choáng + 2 cột lửa mỗi bên · Shield Bash 10 đẩy văng 12 · Shield Guard (chặn mọi đòn phía trước 1.5s, `EnemyGuard`) · Charge húc 14. < 60% gầm (`roar`) → thêm Combo, Shockwave (nổ quanh thân + cột lửa); < 30% enrage (đỏ, nhịp ×0.7).
  - **Unmaker "The Unmaker"** (900 máu, không bao giờ khựng; đấu trường x 14.5–24.3): Slash 18 · Beam (tia xuyên 16u/s) 20 · Wing Burst (vòng 2.3u) 16 văng 11 · Teleport ra sau lưng player. < 66% → thêm triệu hồi 2 **Imp** (tối đa 4, prefab `Prefabs/Enemy/Imp`: 12 máu, cắn lao 6) + Meteor Rain (2 đợt × 3 thiên thạch rơi) 14; < 33% → `rageIdle`, nhanh ×1.3, nhịp ×0.7.
  - Hazard cột lửa đổi tên `KnightRuptureFlame` → `FlamePillar` (dùng chung Knight + Demon, GUID giữ nguyên). Đạn mới `UnmakerBeam`, `UnmakerMeteor`.
  - Sửa chung cho mọi enemy: (1) lên kế hoạch đòn bỏ các đòn có tầm tối thiểu > khoảng cách hiện tại — trước đó Demon đứng im ~3s khi player ở 2.4–3u (Charge quá gần, Swing quá xa); (2) `IsBlocked` quét cả hình collider theo hướng đi thay vì 1 tia ở tâm thân (Unmaker cao 3u vướng mép trên "Wall (1)" mà tia không thấy) + Chase bị chặn đường thì dùng đòn đang trong tầm thay vì đứng chờ.
  - Đã chạy Play mode: Demon 20s — 6 loại đòn, khoảng nghỉ dài nhất giữa 2 đòn 0.92s, chuyển phase ở 260 (roar) / 120 (enrage) có bất tử; khiên chặn 30/30 đòn phía trước, đòn sau lưng vẫn trúng; cột lửa tối đa 4 cùng lúc. Unmaker 22s — Slash / Wing Burst / Beam / Teleport (hiện sau lưng player 2.2u) / Summon (4 Imp, Imp cắn trúng) / Meteor trúng, không lần nào khựng. `BossArena`: 2 boss ngủ → vào trigger Demon: tường bật, thanh máu "Crimson Brute" → hạ Demon: tường mở, thanh máu ẩn → vào Unmaker: "The Unmaker". `SampleScene` hồi quy 18s: 4 enemy tuần tra / đuổi bình thường, console sạch. EditMode 27/27.

- [x] **EN-12** 🖥️ Độ mượt chuyển animation enemy / boss:
  - Đo (script tạm, mọi chỗ nối clip thực tế idle↔walk/run, idle→báo hiệu→đòn→idle, hit, death, phase): trước sửa ~75 chỗ nối thân giật ngang > 0.12u (Knight tới 0.9u, Demon 0.7u) do pipeline chỉ căn từng clip riêng lẻ.
  - `Tools/ArtPipeline/slice.py`: `_align_clips` căn các clip với nhau theo chuỗi chuyển thật (`align.chains` trong `build.py`, vd `tellExecute → overhead`), bù trôi tuyến tính trong clip có giới hạn (`RAMP_CAP_U`), khớp theo đầu + thân trên và bỏ pixel lửa / glow. Sau sửa: Axeman, Stalker, Demon 0 chỗ nối lệch; Knight còn tellSlash→slash 0.28u, cleave→idle 0.14u.
  - Root shift: đòn kết thúc với thân lệch chỗ (Knight Cinder −1.02u, Slash −0.24; Unmaker Slash −0.57, Summon −0.20) → manifest `endShift` → importer ghi `EnemyAnimator.endShifts` vào prefab; chuyển clip sau khi clip chạy ≥ 85% thì `EnemyController.ShiftForward` dời nhân vật (dò tường, mép vực, thân khác). Đo Play mode: Knight Cinder → idle thân giật 1.03u → 0.01u; sát vật cản thì dời được tới đâu hay tới đó (không còn bị physics đẩy bật ngược).
  - Chase chống nháy: vùng đệm đứng / đi (dừng 85%, đi tiếp khi ra hẳn tầm) + giữ tối thiểu 0.35s; quay mặt có vùng chết 0.3u; cung thủ lùi có hysteresis +1u + cờ dồn góc (trước: bị dồn vào tường thì lật mặt mỗi frame); không dùng đòn lùi khi sau lưng là tường / vực. Đo Play mode: player nhích ±0.15u qua rìa tầm 6 lần/s → Axeman đổi idle/walk 6 lần/s → 2 lần/s (±0.06u: 0); player đứng trên đầu lệch ±0.2u → 0 lần lật; Stalker dồn góc 5s → 2 lần lật.
  - Chưa xử lý: giật trong clip do AI vẽ (Axeman charge, Stalker death / rain, Knight slash, Unmaker hit) — cần gen lại art; trượt chân (tốc độ đi vs nhịp bước) chưa đo; Stalker bị áp sát < 1.5u ở góc tường thì đứng chờ (mọi phát bắn có tầm tối thiểu) — chuyện gameplay, chưa đổi.

- [x] **EN-13** 🖥️ Thiết kế lại + gen lại toàn bộ art animation 5 enemy / boss (sprite-gen, GPT image rows):
  - Base mới phản diện hơn (user chọn 5/5 phương án B, concept chỉ làm cảm hứng); chiều cao so với player 1.74u: Axeman 2.1u (1.2×), Stalker 1.9u (1.1×), Knight 2.5u (1.45×), **Demon 3.5u (2×), Unmaker 4.3u (2.5×)** — boss to hơn hẳn trước (1.48× / 1.86×).
  - Giữ đúng bộ anim cũ (Knight thêm `death`), 72 row, 4–6 frame / row, thời lượng ≈ cũ. Mọi đòn bắt đầu / kết thúc ở idle; tư thế báo hiệu dừng đúng pose đầu của đòn. 16 row phải gen lại (lệch nhận dạng, tách nền hỏng), 3 row thêm lần nữa.
  - `Tools/ArtPipeline/from_run.py` (mới): frame sprite-gen → `Art/Enemies/<Name>` + manifest; chuẩn hoá cỡ giữa các row (sprite-gen fit từng row riêng, lệch 5–30%) bằng khớp mặt nạ frame đầu / cuối với idle; căn chỗ nối + root shift dùng chung `slice.compose_cells` (tách từ `slice_sheet`). Giữ `FX/` cũ.
  - `hitTimes` chỉnh theo frame va chạm mới: Axeman Smash/Spin, Stalker Quick/Charged/Piercing/Rain, Knight Combo, Demon Swing/Combo, Unmaker Beam/WingBurst/Meteor. Collider + vị trí / bán kính / tầm hitbox, muzzle × hệ số tăng chiều cao mỗi prefab.
  - Đã kiểm: Import Enemy Art không lỗi (72 clip); EditMode 27/27; Play mode (Step) BossArena: Demon run→swing→shield, Unmaker move→beam→wingBurst→slash; Axeman tellCharge→charge→tellSmash→smash→spin; Stalker aim→triple/quick/charged; Knight stance→cleave — đòn trúng player, console 0 lỗi.
  - Chưa: FX rời (tên, beam, thiên thạch, lửa, Imp) vẫn art cũ; Unmaker muzzle ở 1.6u còn quả cầu phép vẽ ở ~3.2u (giữ thấp để tia trúng player); Unmaker `phase` kết thúc to hơn `rageIdle` chút; Axeman `hit`, Unmaker `taunt` chuyển động nhẹ.

- [x] **EN-14** 🖥️ Remake art cũ lệch style (sau EN-13):
  - 8 FX vẽ lại bằng sprite-gen cùng nét với art mới, giữ tên file (GUID) + kích thước thế giới bbox cũ, PPU 128 (`SpriteGenRuns/install_fx.py`): Stalker tên thường / tụ lực / xuyên / tia xuyên / mưa tên, Unmaker tia / thiên thạch, Knight lửa cột 4 frame (bùng → cột → đỉnh → tàn).
  - Imp (minion Unmaker) + MeleeEnemy (lính thường, art "Grunt") thiết kế mới có animation (Imp 6, Grunt 7 clip; cao 1.3u / 1.9u); prefab thêm `Animator` + `EnemyAnimator` (bỏ `SpriteSequence` của Imp), MeleeEnemy đổi pivot về chân ⇒ capsule (0.75×1.75, off 0.9), hitbox (0.85, 0.85) r 0.65; Imp collider / hitbox ×1.3. `ImpBite` → tellBite / bite, hit 0.12 · 0.25; `MeleeEnemySwing` → tellSwing / swing, hit 0.12.
  - Sửa kèm: state `death_Knight` (EN-13) chỉ nằm trong bộ nhớ Editor, file controller commit thiếu — import lại đã ghi. Controller mới của Imp / Grunt cũng bị lệch bộ nhớ ↔ đĩa sau 1 lần import bị MCP timeout, chạy lại importer thì đúng (nguyên nhân gốc chưa xác định).
  - Đã kiểm: EditMode 27/27; Play mode (Step): Grunt tellSwing→swing trúng player, Imp idle→bay→tellBite→bite, Stalker bắn tên tụ lực sprite mới 1.79×0.59u; prefab đạn / hazard đủ sprite mới. Console 0 lỗi.

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
| Đòn trên không Fighter | Đặt effect `Knockback` (không có tác dụng) | Effect `None`, knockback vẫn theo `knockbackForce` = 3 — không đổi cảm giác |
| Player bị trúng đòn | Không phản ứng (không có state) | Mất điều khiển 0.3s + knockback + anim `hit`; đòn choáng giữ tới hết `effectDuration` |
| I-frame player | Không có | Bất tử 0.8s + nhấp nháy sau mỗi lần trúng — chỉnh `invincibilityDuration` |
| Enemy AI (`MeleeEnemy` ở x=19) | Không có enemy đánh trả | Tuần tra, đuổi khi thấy player (6u), vung đòn 0.45s (tô đỏ) rồi đánh 10 dmg, cooldown 1.2s — chỉnh trong `MeleeEnemyData` |
| Camera | Bám cứng tâm player (thấy cả khoảng trống dưới đất) | Mượt 0.12s, vùng chết nhỏ, không nhìn ra ngoài biên level — player đứng đất nằm ở ~1/3 dưới màn hình |
| Súng | Bắn liên tục: viên thứ 2+ vô hình; đạn hiện lệch phía trước nòng; bắn trên không tư thế chớp 1 frame | Mọi phát đều ra đạn từ đúng đầu nòng cùng frame giật súng; bắn trên không giữ tư thế 0.2s |
| Camera khi rơi | Tụt lại, player chạy về mép dưới / ra khỏi màn hình | Nhìn trước xuống dưới, bám sát; player luôn trong khung |
| Bị đánh trên đất | Trượt mãi tới khi bấm phím | Lùi ~0.26u rồi dừng — chỉnh `groundDeceleration` / `knockbackForce` |
| Player hết máu | Object bị tắt, game đứng | Animation chết → 2s sau hồi sinh tại điểm xuất phát (đầy máu, nhấp nháy bất tử 0.8s) — chỉnh `respawnDelay` |
| Axeman (x=9) | — | Đòn báo hiệu bằng tư thế + tô đỏ; Charge lao từ 2.5–4.2u; dưới 40% máu enrage (ửng đỏ, đánh dồn hơn). Chỉnh trong `Data/Enemy/AxemanData` + `Attacks/Axeman/*`, hitbox trên prefab |
| Knight (x=14.5) | — | Không bị ngắt khi đang vung; Overhead/Cleave tầm ~2u, Slash/Combo ~1.8u; dưới 50% thêm Cinder Wave (cháy). Cần soi: tầm hitbox có khớp vệt chém không, nhịp báo hiệu có né kịp không |
| Stalker (x=23) | — | Giữ khoảng cách, bắn 5 kiểu (tên thường / tụ lực xuyên / 3 tên / tia xuyên / mưa tên rơi từ trên), áp sát < 2.2u thì lướt lùi. Cần soi: tốc độ tên có né / nhảy qua kịp không, mưa tên rơi có đủ thời gian chạy khỏi |
| Boss — `SampleScene` (BossArena đã gộp vào, 2026-11-19) | — | Đi sang phải qua 4 enemy thường: x=32.5 vào trận Demon (tường chặn 2 bên, thanh máu trên giữa màn), x=44 vào trận Unmaker; hết map x=57 có tường. Chết giữa trận → boss về chỗ cũ đầy máu. Cần soi: độ khó / máu boss, Shield Guard có quá lì không, mưa thiên thạch + Imp có quá dồn không, tường đấu trường có làm player bám tường (layer Wall) gây khó chịu không |
| Đánh + di chuyển (CB-12) | Đánh là đứng khựng, hết clip còn đứng chờ | Đang chạy bấm đánh: còn trớn, đòn lao tới; giữ hướng khi đánh lao xa hơn, giữ ngược thì quay người giữa combo; đánh xong giữ hướng là chạy ngay; Jump / Dash / Slide ngắt được sau frame trúng; spam đánh vẫn đủ 3 đòn; đứng mép vực đánh không lao xuống. Chỉnh: `lungeDistance` từng đòn, `PlayerStats` attackLungeHoldMultiplier / attackDeceleration / attackMoveCancelDelay / comboChainDelay |
| Art FX + Imp + lính thường (EN-14) | FX mờ, Imp sprite tĩnh, MeleeEnemy hình tạm | FX / Imp / Grunt mới có animation. Cần soi: đạn có khớp collider không (sprite đổi tỉ lệ cao/rộng), lửa cột có báo hiệu rõ không, Grunt / Imp đọc được ở cỡ nhỏ không |
| Art enemy / boss mới (EN-13) | Art sheet cũ, boss 1.5–1.9× player | Thiết kế mới, Demon 2×, Unmaker 2.5× player; collider / hitbox phóng theo. Cần soi: chuyển clip có nhảy cỡ / giật không, hitbox có khớp vệt chém mới không, boss to có che tầm nhìn / chật đấu trường không, mốc trúng (`hitTimes`) có khớp frame va chạm không |

---

## Phase 9 — Asset & visual (P2)

- [x] **AS-01** 🖥️ Môi trường dark fantasy (pháo đài đổ nát, đêm tím + than hồng) thay ô vuông trắng — SampleScene + BossArena:
  - Art sprite-gen (`SpriteGenRuns/env`, ref = base Axeman / Unmaker), tile lặp bằng `sprite-gen background-tile` (seam ground / far: needs-review → soi mắt OK; mid / wall / ladder: pass): `Art/Environment/` ground_tile (PPU 259, cao 1.2u), wall_tile (426), ladder_tile (600), far_tile (90), mid_tile (100), sky (100); FullRect + Repeat.
  - Mỗi Ground / Wall / Ladder: tắt SpriteRenderer Square, thêm child `Visual` (Tiled, sorting −5..−7) khớp bounds cũ — **collider / transform không đổi**. Bục nhỏ cao 0.7u. `Underground` + `UndergroundFade` che trời dưới nền đất.
  - `Scripts/Environment/ParallaxLayer.cs` (mới): Sky (1, 1, `centerOnCameraAtStart`), Far (0.85, 0.6), Mid (0.6, 0.3) — root `Environment_BG`.
  - Đã kiểm: EditMode 27/27; Play mode Step: camera (15.6, 2.3) → trời phủ kín, các lớp dời đúng tỉ lệ; console 0 lỗi.
  - Chưa: bia tập "Enemy" vẫn hình fighter đỏ tạm; tường lặp lộ dải sắt đôi ở chỗ nối tile; tường đấu trường boss (đỏ mờ) giữ nguyên.

- [x] **AS-02** 🖥️ VFX trúng đòn + hit-stop:
  - Art sprite-gen (run effect): `Art/FX/Hit/` hit (5 frame, trắng-cam), crit (5, đỏ-vàng + vòng sóng), block (4, xanh-trắng + vệt khiên; run riêng khoá magenta vì khoá xanh tự chọn trùng màu). Prefab `Prefabs/FX/HitFx_{hit,crit,block}` (SpriteSequence 24fps + `HitFx` tự Recycle, xoay ±25°, lật theo hướng đòn); đăng ký `PrefabRegistrySO.hitFx/critFx/blockFx`.
  - `HitFeedback` (mới, nghe `Damageable.OnDamaged / OnBlocked` như DamagePopupSpawner): spawn FX qua ObjectPool ở mép thân phía đòn đến (55% chiều cao collider), chí mạng ×1.25; hit-stop qua `HitStop.Request` (mới: hạ `Time.timeScale` 0.05 theo giây thật, chồng yêu cầu lấy mốc xa nhất). Mặc định 0.06s / crit 0.11s / chặn 0.04s; player bị đánh 0.08 / 0.12. Gắn: 7 prefab enemy (cỡ FX: Demon 1.5, Unmaker 1.6, Imp 0.7…), Player + 3 bia tập ở 2 scene.
  - Đã kiểm Play mode: trúng thường → FX hit đúng phía, timeScale 0.05 → 1 sau 0.15s thật; crit → FX crit ×1.25; FX tự thu sau ~30 frame; EditMode 27/27; console 0 lỗi.
  - Chưa: rung camera (screen shake) chưa làm; hit-stop áp cho mọi đòn trúng kể cả đạn / vùng nguy hiểm — nếu thấy khựng quá nhiều khi bị mưa thiên thạch thì giảm riêng.

- [x] **AS-03** 🖥️ UI thanh máu player + thanh máu boss cùng style:
  - Art sprite-gen (`Art/UI/`): `hp_frame` (khung sắt cháy + ổ tròn trái — chỗ cho icon vũ khí sau này), `boss_frame` (khung gai + sọ sừng, rune tím), `bar_fill` (dải bóng trắng, tô màu trong game).
  - `HUD.prefab`: khung là Image đè lên (raycast off), thanh / fill khớp vùng rỗng đo từ alpha của khung; player: thanh 428×43, khung ×0.27; boss: giữ trên giữa, thanh 510×59, khung ×0.28, tên boss dưới khung — 2 khung không chồng nhau (mép phải player 625 < mép trái boss 656, canvas 1920). Logic fill / trail / màu giữ nguyên (`PlayerHUD`, `BossHealthBar`).
  - Đã thử đặt thanh boss ở đáy màn hình → che vùng chân nhân vật đánh nhau → bỏ.
  - Đã kiểm Play mode (screenshot): trận Demon → thanh boss hiện, mất máu đúng; EditMode 27/27.
  - Chưa: tường đấu trường boss vẫn là dải đỏ mờ (lệch style); chữ "Fighter" chưa có icon.

- [ ] **AS-04** 🟡 Art FX đầy đủ (sprite-gen, cùng nét vẽ tay) — ĐÃ CÓ ART trong `Art/FX/*`, CHƯA import / gắn code (cần Editor):
  - **Warm** (12): sword_slash1/2/3 + sword_air → animation event `SwordAttack(n)` / `FighterAirAttack`; fist_swoosh → `FighterAttack`; muzzle_flash → `FireProjectile` (đầu nòng); bullet_impact → `Bullet` chạm tường; burn_loop / stun_loop → `BurnEffect` / `StunEffect` Begin–End; boss_explosion → boss chết; phase_burst → `EnemyPhaseChangeState`; torch_loop → đuốc trong màn.
  - **Dust** (6): jump_dust / land_dust / run_dust (Jump, Land, Run), dash_burst (Dash), slide_dust (Slide), wall_sparks (WallSlide).
  - **Void** (8): enemy_death (`EnemyDeadState`), enemy_spawn (SpawnZone MAP-01), beam_impact / meteor_impact (đạn Unmaker chạm), exit_portal (cổng ra màn, lặp), rune_pulse (cổng rào rune, lặp), telegraph_glint (đầu Telegraph đòn nặng), hazard_warning (`GroundHazard` lúc báo trước, lặp).
  - **Cold** (2): freeze_crust (`FreezeEffect`, lặp), arrow_impact (tên Stalker chạm; đã bỏ 1 frame co lại — chọn tay).
  - **Particles**: particle_ember / smoke / fog → ParticleSystem (than bay, khói, sương nền).
  - **Environment** thêm: rune_gate_tile (tile dọc, cổng rào đấu trường), exit_gate, platform.
  - ice_shatter: 3 lần gen theo frame không ra hình vỡ tung → thay bằng `FX/Cold/ice_block.png` (khối nứt) + `FX/Particles/particle_ice_shard_0..4` cho ParticleSystem bắn mảnh (AS-05).
  - Chưa: import PPU/pivot + prefab `HitFx`-style + gắn code; FX có "frame nhỏ hơn trung vị" ở đầu/cuối là chủ ý (bùng / tàn).

- [ ] **AS-05** 🟡 Bổ sung art còn thiếu — ĐÃ CÓ ART, CHƯA import / dùng (cần Editor):
  - `UI/Controls` (8): joystick base + knob, nút attack / jump / dash / slide / swap / pause → điều khiển cảm ứng Android (game chưa có, cần code On-Screen Controls của Input System).
  - `UI/Icons` (8): icon_weapon_{fighter,sword,gun} (ổ tròn trái khung HP, thay chữ "Fighter"), icon_stat_{health,armor,damage,speed,crit} (kho đồ EQ-03). icon_weapon_sword: provider vẽ nền đen thay vì khoá → tách bằng `sprite-gen cutout --key measured`.
  - `UI/Frames` (6): ui_panel / ui_button (9-slice), ui_slot (ô trang bị), ui_title_banner (tên màn), ui_victory_frame / ui_defeat_frame (màn thắng / thua).
  - `Environment/L2` (Tường thành nguyền: trời đỏ máu, thành lũy, tường cờ đỏ, sàn gạch đỏ) + `Environment/L3` (Ngai Hư Không: vũ trụ tím, đảo đá nổi, cột hắc diện, sàn hắc diện nứt tím): sky + far_tile + mid_tile + ground_tile (tile ngang, seam soi mắt OK).
  - `Props` (10): torch_holder, banner, chains, skull_pile, broken_pillar, barrel_crate, spike_trap, brazier (checkpoint), dead_tree, gravestone.
  - `Equipment/{Iron,Ember,Void}` (18): {set}_{head,body,cape,gauntlet,sword,gun} — sprite trang bị cho EQ-02 (lớp hiển thị) + icon kho đồ; ember_gun lấy ảnh gốc có alpha (bản tách chroma mất thân súng).
  - Chưa làm: logo / tên game (chưa có tên), đổi ngoại hình player (cần quyết).
- [ ] **AS-06** 🟡 Remake toàn bộ art theo style "bóng đen + 1 màu nhấn" (`Docs/ART_STYLE.md`) — ART XONG + push, CẦN EDITOR:
  - Lý do: art cũ lộ AI (chi tiết đều khắp, glow, trang trí quá tay), nền bão hoà làm chìm nhân vật. Giờ: nền 3 màn ít màu, phẳng, nhạt dần theo chiều sâu; đất/tường gần đen + viền sáng; nhân vật mang màu (player trắng, enemy đen than + trắng xương + đỏ cam `#e5482b`); đỏ cam = nguy hiểm.
  - Đã thay (giữ tên file + kích thước canvas): Environment (trời / đất / tường / thang / bục vẽ bằng code), Props, UI (HUD bar + panel / button / slot vẽ code; icon, nút, emblem gen), FX (35 clip + particle), Equipment 18, đạn / tia enemy, 7 enemy × mọi clip (399 frame, cùng số frame như cũ).
  - **Sửa enemy lơ lửng (2026-11-18)**: `from_run.py` lấy đường chân = đáy ô, nhưng sprite-gen đặt chân cách đáy ô `cell.safe_margin_y` (36–48px) → mọi enemy lơ 0.2–0.6u (cả art EN-13). Giờ ground = đáy ô − safe_margin_y; cài lại 7 enemy + Import Enemy Art: chân lệch pivot ≤ 0.026u mọi clip, Knight `endShifts` cleave tự tính lại = [] . 27/27 EditMode.
  - **Hiệu ứng trúng đòn theo vũ khí (2026-11-18)**: vẽ bằng code `Tools/ArtPipeline/hitfx.py` (10 frame liền mạch, easing, 30fps; thay các frame AI rời rạc): Fighter `HitFx_fist` (bùng + vòng chấn động + vệt tốc độ), Sword `HitFx_slash` (lưỡi liềm quét, lật dọc ngẫu nhiên), Gun `HitFx_bullet` (tia lửa văng ngược), đòn enemy `HitFx_hit` (vệt cào đỏ cam), `HitFx_crit` giờ PHỦ THÊM lên hiệu ứng vũ khí, `HitFx_block` vẽ lại. `AttackDataSO.hitFx` + `hitFxScale` (đòn 3 = 1.35, trên không 1.1); trống = `PrefabRegistrySO.hitFx`. Cần play-test cảm giác.
  - **Việc trong Editor**: Import Enemy Art ĐÃ chạy (PPU mới); còn soi hitbox / `hitTimes` + play-test: mở Unity → refresh → **Tools/Enemies/Import Enemy Art** (PPU mới: Axeman 140, Demon 97, Grunt 135, Imp 171, Knight 141, Stalker 123, Unmaker 77 — chưa import thì cỡ enemy sai) → soi collider / hitbox + `hitTimes` (tư thế đòn đổi) → screenshot 3 màn.
  - Pipeline (ngoài git): `SpriteGenRuns/as06/` (style.py, enemies.py, fx.py, misc.py, env.py, proc.py, proc_ui.py, install.py, install_anim.py). RAM 16GB: ≤ 5 codex cùng lúc.
  - Chưa remake: `Enemies/Unmaker/FX/unmaker_imp_00/01` (chờ Q6 xoá), player (Q2).

## Phase 10 — Cảm giác chơi & vòng chơi (đề xuất 2026-11-18, user đồng ý)

Thứ tự: IN-01 → AS-07 → CB-11 làm ngay (không cần quyết) · EQ-02 / GS-02 / GS-04 chờ user chốt · MAP-02 + logo để sau.

- [x] **IN-01** 🖥️ Điều khiển cảm ứng Android — `HUD.prefab/SafeAreaPanel/TouchControls`:
  - Joystick trái dưới (`OnScreenStick` → `<Gamepad>/leftStick`, tầm 95px, vùng bắt chạm trong suốt 380px quanh núm); nút phải dưới `OnScreenButton`: Attack → buttonWest, Jump → buttonSouth, Dash → buttonEast, Slide → buttonNorth, Swap → rightShoulder. Ảnh `UI/Controls` cắt về vuông 512px (giữ GUID), alpha 0.8.
  - Sửa binding gamepad trong `PlayerInputs.inputactions` (trước: Jump / Attack / Dash / Slide cùng buttonSouth). Action mới `SwapWeapon` (Q / RB) → `PlayerInputHandler.OnSwapWeapon` → `WeaponManager.CycleWeapon` (Fighter → Sword → Gun, bỏ qua khi chết).
  - `TouchControls`: chỉ hiện khi `Application.isMobilePlatform` (bật `showOnDesktop` để thử bằng chuột trên PC).
  - Đã kiểm Play mode (giả lập pointer qua EventSystem): 4 nút → đúng buffer, Swap xoay vòng Sword → Gun → Fighter, kéo joystick → MoveInput (−0.99, 0.13), thả → (0,0); chạm lệch tâm 120px vẫn bắt joystick. EditMode 27/27.
  - Chưa: nút Pause (chưa có hệ pause — làm cùng GS-02 / setting); chưa thử trên điện thoại thật (đa chạm, cỡ nút theo DPI).
- [x] **AS-07** 🖥️ FX chuyển động:
  - Player (`PlayerFx` trên Player ở SampleScene + BossArena): theo dõi state (không sửa state) → bụi `Fx_jump_dust` (chỉ khi còn trên đất; nhảy tường = `Fx_wall_sparks`), `Fx_land_dust`, `Fx_dash_burst`, `Fx_slide_dust`, `Fx_run_dust` mỗi 0.28s khi chạy, `Fx_wall_sparks` mỗi 0.12s khi trượt tường. `PlayerAnimationAttack` gọi lúc ra đòn: kiếm `Fx_sword_slash1/2/3` theo bước combo + `Fx_sword_air`, đấm `Fx_fist_swoosh`, đòn cuối combo ×1.25; súng `Fx_muzzle_flash` ở đầu nòng. Vị trí thân tính theo hình capsule (dash tắt collider ⇒ bounds rỗng).
  - Enemy: `EnemyProjectile.impactFx` (tên Stalker → `Fx_arrow_impact`, tia Unmaker → `Fx_beam_impact`, thiên thạch → `Fx_meteor_impact`) ở điểm trên mặt vật cản (`ClosestPoint`); `EnemyDeadState` phát `PrefabRegistrySO.enemyDeathFx` lúc xác bắt đầu mờ, cỡ theo chiều cao thân.
  - Art: `Tools/ArtPipeline/motionfx.py` vẽ lại bằng code (vệt kiếm / đấm / loé nòng / bụi / tia tường / tên găm, cùng nét hitfx.py, trắng cho player, xám nhạt cho bụi, đỏ cam cho đòn enemy); giữ art AI liền mạch: `enemy_death`, `beam_impact`, `meteor_impact` (pivot đáy cho thiên thạch / tia).
  - Đã kiểm Play mode: vệt đòn đúng vị trí, slash3 ×1.29; bụi land / dash / slide / jump ở y = mặt đất (−2.27) kể cả lúc dash; xác Melee → `Fx_enemy_death` sau 0.78s; thiên thạch nổ ở −2.26 (đất −2.27). EditMode 27/27.
  - Chưa: bụi chạy / tia trượt tường chưa thử trong game (logic lặp giống nhau, chưa chạy input thật); FX boss_explosion / phase_burst / enemy_spawn / telegraph / hazard / exit_portal / rune_pulse / burn / stun / freeze chưa gắn (làm cùng GS-02 / MAP).
- [x] **CB-11** Rung màn hình theo đòn:
  - `Scripts/Camera/CameraShake.cs` (tĩnh như HitStop): trauma 0..1 cộng dồn có trần, giảm 1.6/s theo thời gian thật (vẫn rung lúc hit-stop), lệch = 0.35u × trauma² × Perlin. `CameraFollow` giữ `basePosition` riêng và cộng rung sau cùng ⇒ smoothing không bị kéo lệch.
  - `AttackDataSO.shake` → `AttackData.shake`; `HitFeedback` cộng `max(shake, minShake)` khi trúng (chí mạng ×1.5). Giá trị: đấm 0.22 / cuối 0.45 / trên không 0.3, kiếm 0.28 / cuối 0.5 / trên không 0.35, súng 0.08, 11 đòn Demon + Unmaker 0.6; player `minShake` 0.4 (bị đánh luôn rung rõ), enemy 0.
  - Đã kiểm: 4 EditMode test mới (trần, bỏ giá trị ≤ 0, giảm về 0, không vượt MaxOffset) → 31/31; Play mode: kiếm 3 → trauma 0.50, súng 0.08, camera lệch (−0.09, −0.09) lúc rung và về đúng vị trí cũ (0.0000) khi hết.
  - Chưa: cảm giác thật cần play-test (có thể giảm nếu chóng mặt); chưa có tuỳ chọn tắt rung trong setting (làm cùng setting popup). Lần đo đầu ra trauma 1.00 (không tái hiện được ở phiên Play sau: 0.50) — để ý khi play-test.
- [x] **EN-15** Boss bị hạ (user: "cứng và vô lý"):
  - Trước: đòn kết liễu khựng 0.06s như đòn thường; thanh máu + tường đấu trường biến mất ngay lúc chạm; Imp triệu hồi biến mất không hiệu ứng; clip chết 6 frame @5–7fps giật; xác Kinematic giữ velocity knockback ⇒ trôi mãi (đo: 4.38u, mọi enemy); chết trên không ⇒ xác treo lơ lửng.
  - `BossEncounter.Defeat`: khựng 0.3s (timeScale 0.02) + rung 0.9 → chậm 0.3→1 trong 0.8s thật; minion `EnemySummoner.KillAll` (chết bằng animation + khói); thanh máu (về 0) + tường giữ tới khi boss inactive rồi mới `Ended`. Bị tắt giữa chừng ⇒ trả timeScale.
  - `EnemyDeadState` (mọi enemy): xoá velocity mỗi frame; xác hất lùi `deathSlide` theo hướng đòn cuối (ease-out 0.45s, raycast tường); chết trên không ⇒ rơi theo trọng lực tới mặt đất; tan thành khói `deathBursts` (boss Demon 5 / Unmaker 6: rải theo bề ngang xác, rung 0.3, cụm cuối ×1.3 + loé trắng + rung 0.6). Khói pivot đáy (0.15) ⇒ bốc lên từ chỗ xác, không lấn xuống đất.
  - `EnemyController`: `LastHitFromX` (OnDamaged của đòn kết liễu tới sau OnDeath ⇒ hướng hất lấy ở frame đầu); `DespawnDelay` = clip chết (nếu có) + `corpseLinger` (trước: enemy không có clip chết bị trả pool ngay, không vào state chết). Imp `corpseLinger` 0 → 0.2.
  - Clip chết Demon 5 → 9 fps, Unmaker 7 → 10 fps (manifest + `SpriteGenRuns/runs/v2_*/sprite-request.json`) + Import Enemy Art.
  - Đã kiểm Play mode BossArena: Demon timeScale 0.02 → 0.41 → 0.93 → 1, xác trượt 0.45u rồi dừng, chết ở độ cao +2u rơi đúng mặt đất (−2.27), chuỗi khói + loé, tường mở ở ~2.6s sau đòn (lúc boss inactive); Unmaker + 2 Imp: Imp chết theo, chạy clip chết, ra khói. EditMode 31/31.
  - Chưa: xác boss vẫn mang màu enrage (tint phase) lúc chết; tường đấu trường vẫn dải đỏ mờ (art `rune_gate_tile` có, chưa gắn); cảm giác cần play-test.
- [x] **EN-16** Tia Unmaker khớp art (user: "vẽ lại cho chuẩn"; đóng Q9):
  - Trước: muzzle (1.86, 1.60) thấp hơn quả cầu trượng ở frame bắn ~0.85u; clip `beam` tự vẽ luồng lửa ngang tầm 2.4u khác hẳn tia bay ra; sprite tia pivot giữa ⇒ nửa tia (2.7u) + vùng sát thương đè ra sau lưng boss.
  - Gen lại row `beam` (`SpriteGenRuns/runs/v2_unmaker`, prompt mới: trượng cầm ngang hông chĩa thẳng, KHÔNG vẽ tia / lửa; bản cũ `prompts/beam.orig.txt`, frame cũ `as06/keep_beam_old/`) → `install_anim.py Unmaker` (cell 596 → 588, PPU 77 giữ nguyên).
  - Tia vẽ bằng code (`motionfx.unmaker_beam`: dải đỏ cam lõi trắng xương, đuôi nhọn, đầu tròn) — pivot đầu đuôi (0.0085, 0.5), cả trong manifest fx để Import Enemy Art không ghi đè; collider 5×1 offset x 2.6. Muzzle → (2.8, 2.03) = mũi quả cầu ở frame bắn (frame 2, t = 0.27). Tia ngang ở 2.03u chạm đầu player đứng (cao 1.74u), slide thấp người né được.
  - Chưa kiểm trong Unity (MCP mất kết nối): cần refresh + Import Enemy Art + Play mode screenshot.
- [x] **GS-07** 60 FPS: `Bootstrap.Awake` đặt `QualitySettings.vSyncCount = 0` + `Application.targetFrameRate = 60` (`[SerializeField] targetFrameRate`; Android mặc định 30). Vật lý giữ 50Hz — nhân vật / enemy đã Interpolate, đạn đi bằng Update ⇒ không giật. Chưa compile / chưa đo trên máy (MCP mất kết nối).
- [x] **CB-12** Đánh + di chuyển (user: "tối ưu hoạt ảnh đồng thời đánh và di chuyển", hướng A — không cần art mới):
  - Trước: `AttackState` ép vận tốc 0 mỗi frame (đang chạy bấm đánh là khựng); hết clip (0.2–0.4s) vẫn đứng chờ hết cửa sổ combo 0.4s; bấm tiếp combo cắt ngang đòn trước cả frame trúng.
  - Lao tới mỗi bước: `AttackDataSO.lungeDistance / lungeDuration` (đấm 0.3 / 0.35 / 0.7u, kiếm 0.35 / 0.4 / 0.9u); giữ hướng ⇒ × `attackLungeHoldMultiplier` 1.6. `AttackMotion` (struct, test được): không chậm hơn đà chạy đang có, hết lao hãm `attackDeceleration` 30 u/s²; chặn ở mép vực (`GroundCheck.HasGroundAhead`), tường do vật lý.
  - Quay mặt theo hướng giữ ở đầu mỗi bước combo.
  - Ngắt phần thu đòn sau frame trúng (animation event → `IAttackHandler.OnHitFrame`): Jump / Dash / Slide ngay; giữ hướng thêm `attackMoveCancelDelay` 0.08s ⇒ chạy (nút đánh đang chờ thì ưu tiên đánh). Súng: Jump / Dash / Slide ngay sau phát bắn, chạy sau `gunMoveCancelDelay` 0.1s.
  - Combo: bấm sớm ⇒ xếp hàng, ra đòn kế sau frame trúng + `comboChainDelay` 0.06s. Hết clip ⇒ rời đòn ngay; bấm lại trong cửa sổ combo (0.4s từ đầu bước) đánh tiếp bước sau kể cả sau khi ngắt bằng chạy (`MeleeAttackHandler.NextComboStep`). Bị đánh / đổi vũ khí / chết ⇒ combo về 1.
  - Đã kiểm: 11 EditMode test mới (`AttackMotionTests`) → 42/42. Chưa chơi thử (Editor đóng, batchmode) — số lao / delay cần chỉnh theo cảm giác.
- [ ] **EQ-02** `P2` `L` 🖥️ ⏸ chờ Q2 (đề xuất A) — Lớp hiển thị trang bị lên player que trắng: head / body / cape / gauntlet / sword / gun theo bộ Iron / Ember / Void (art `Equipment/*` có sẵn), bám xương theo từng frame animation player; giáp thân phóng to hơn bản mô phỏng.
- [ ] **GS-02** (đã có ở Phase 6) ⏸ chờ user chốt: thắng màn = tới cổng ra hay hạ boss · có giới hạn mạng / màn thua không · checkpoint = brazier (art có).
- [ ] **GS-04** (đã có ở Phase 6) ⏸ chờ user: nguồn âm thanh (user có sẵn / CC0). Ưu tiên SFX trúng đòn theo vũ khí (khớp hit FX mới), nhảy / dash, enemy đánh, BGM 3 màn.
- [ ] **MAP-02** `P3` `L` 🖥️ Dựng màn 2 + 3 (nền `Environment/L2`, `L3` đã có) — sau khi vòng chơi 1 màn (GS-02) xong.
- [ ] **AS-08** `P3` `S` ⏸ chờ Q1 — Logo / tên game (sprite-gen `logo`).

## Câu hỏi chờ user trả lời (ghi 2026-11-17)

| # | Câu hỏi | Bối cảnh / lựa chọn |
|---|---|---|
| Q1 | Tên game + logo | Chưa có tên → chưa gen logo (sprite-gen `logo` làm được khi có tên) |
| Q2 | Player giữ que trắng hay vẽ lại? | Hiện que trắng nét phẳng (asset pack), enemy vẽ tay chi tiết. A: giữ que trắng + trang bị phủ lên (EQ-02). B: vẽ lại cả bộ ~40 clip cùng style (việc lớn, đụng animation event combat) |
| Q3 | Thang có cần leo xuống từ trên không? | Hiện chỉ vào Climb khi đứng đất + bấm lên → không leo xuống được. Có thì phải sửa ClimbState / GroundedState |
| Q4 | Dash xuyên tường mỏng: giữ hay chặn? | DashState tắt collider → dash 3u xuyên tường 1u, bỏ qua được đoạn wall-jump. Chặn = tường dày > 3u hoặc không tắt collider khi dash |
| Q5 | Xác nhận thiết kế EQ-01 | Giáp trừ phẳng (damage − armor, tối thiểu 1); tốc độ chạy % áp cả khi trên không; PlayerEquipment tự gắn bằng code; tăng máu tối đa chỉ cộng phần chênh (không hồi đầy) |
| Q6 | Xoá asset cũ không dùng? | FX `unmaker_imp_00/01` + entry trong `unmaker_manifest.json` (Imp đã có art riêng) |
| Q7 | Thư mục `SpriteGenRuns/` (ngoài git) sao lưu ở đâu? | Chứa base, request, ảnh gốc — cần khi gen lại 1 clip / FX |
| Q8 | Scene test cũ (SampleScene — 2026-11-19 đã gộp BossArena vào, xoá bia tập "Enemy") | Giữ làm scene thử hay xoá khi có 3 màn mới? |
| ~~Q9~~ | (ĐÃ XỬ LÝ EN-16) Tia Unmaker bắn thấp hơn quả cầu trượng | Giữ (để trúng player) / dời muzzle lên quả cầu + tia chéo / gen lại clip beam |
| Q10 | Hit-stop cho mọi đòn trúng (kể cả đạn, mưa thiên thạch) | Sau play-test: thấy khựng nhiều thì giảm riêng cho đạn / vùng nguy hiểm |

## Việc còn mở sau EN-13 / EN-14 (chờ user test + feedback)

Chưa làm hoặc chưa kiểm chứng — user play-test rồi feedback, Claude xử lý theo feedback:

| # | Việc | Hiện trạng | Hướng xử lý đề xuất |
|---|---|---|---|
| 1 | Tia Unmaker bắn ra thấp hơn quả cầu trên trượng | Muzzle 1.6u (để tia trúng player cao 1.74u), quả cầu vẽ ở ~3.2u | Giữ; hoặc dời muzzle lên quả cầu + cho tia chéo xuống; hoặc gen lại clip `beam` chĩa trượng ngang hông |
| 2 | Unmaker `phase` → `rageIdle` có thể nhảy cỡ | Frame cuối `phase` to hơn `rageIdle` một chút (scale chuẩn hoá theo frame đầu) | Soi khi chuyển phase; nếu thấy thì `from_run.py --scale phase=…` hoặc gen lại `phase` |
| 3 | Clip chuyển động nhẹ | Axeman `hit` chỉ nghiêng đầu, Unmaker `taunt` gần như đứng yên | Gen lại nếu thấy thiếu lực |
| 4 | Đạn tia / thiên thạch Unmaker chưa xem tận mắt khi bay | Mới kiểm sprite + kích thước trên prefab | Play-test trận Unmaker |
| 5 | Collider đạn vs sprite mới | Tên mảnh hơn (cao 0.39–0.59u), thiên thạch hẹp hơn (0.75u); collider giữ như cũ | Nếu trúng / né thấy sai thì chỉnh `BoxCollider2D` từng prefab đạn |
| 6 | `hitTimes` chỉnh theo mắt (14 đòn) | Đặt theo frame va chạm thấy trên preview, chưa đo cảm giác | Feedback đòn nào "trúng sớm / trễ" |
| 7 | Collider / hitbox phóng theo tỉ lệ | Nhân đều theo mức tăng chiều cao, chưa khớp từng đòn | Feedback đòn nào tầm sai; boss to có làm đấu trường chật không |
| 8 | Cân bằng chỉ số | Chưa đổi (vd Knight cleave trúng ~48 máu) | Sau play-test |
| 9 | AnimatorController lệch bộ nhớ ↔ đĩa | 2 lần: `death_Knight` (EN-13) + controller Imp/Grunt sau import bị MCP timeout; nguyên nhân gốc chưa rõ | Sau mỗi lần import kiểm số state trong file `.controller`; nếu lặp lại thì sửa importer |
| 10 | `BossArena.unity` bị ghi ngoài ý muốn 1 lần (object "Enemy" bị tắt) | Đã khôi phục, nguyên nhân chưa rõ | Theo dõi `git status` sau khi test qua MCP |
| 11 | Asset không còn dùng / tạm | FX `unmaker_imp_00/01` + entry trong `unmaker_manifest.json` không còn dùng (Imp có art riêng). `Art/fighter_Idle_0002.png` + `Enemy.cs` (bia tập) không còn object nào dùng từ 2026-11-19 (đã xoá 3 bia tập) | Chờ user quyết xoá FX Imp cũ, sprite + script bia tập |
| 12 | Hiệu ứng trúng đòn / hit-stop | Chưa có (có thể dùng skill `beanfx` hoặc Epic Toon FX) | Nếu muốn đòn "đã tay" hơn |
| 13 | Còn từ EN-12 | Trượt chân (tốc độ đi vs nhịp bước) chưa đo; Stalker bị dồn góc tường đứng chờ | Đo / sửa sau play-test |
| 14 | Nguồn gen art (`SpriteGenRuns/`) nằm ngoài git | Base, request, raw cần khi gen lại một clip | Muốn giữ lâu dài thì sao lưu / đưa vào repo (khá nặng) |

## Thứ tự đề xuất (phần còn lại)

```
~~IN-01~~ → ~~AS-07~~ → ~~CB-11~~   (Phase 10, làm ngay)
  → FIX-07 (play-test thủ công — xem bảng "Cần play-test", gồm Phase 8: enemy mới + BossArena)
  → EQ-02 (chờ Q2) → GS-02 phần còn lại (cần chốt: điều kiện thắng màn, giới hạn mạng / màn thua, checkpoint)
  → GS-01 scene flow (cần chốt: có Menu scene không)
  → GS-04 audio (cần file âm thanh) → GS-05 save (khi có nhiều level)
  → AN-04 (khi thêm vũ khí thứ 4), OR-08 CI (cần Unity license secret trên GitHub)
  → OR-05 namespace (tuỳ chọn)
```
