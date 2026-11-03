# StickFigure — hướng dẫn project

Game hành động 2D side-scroll (stick figure): chạy, nhảy, leo, bám tường, dash, slide; 3 vũ khí (Fighter, Sword, Gun) có combo; enemy AI cận chiến.

- **Unity** 2022.3.62f2 · Android · **Input System** 1.14 (không dùng `UnityEngine.Input`) · TextMeshPro · DOTween (Plugins)
- **Framework nội bộ** (`Assets/_GamePlay/Framework`, namespace `Kore.Utils.Core`): `Service` (service locator), `ObjectPool`, `ObserverService`, `SaveGame`, `PopupBase`
- **Backlog kỹ thuật**: `BACKLOG.md` (mã task `FIX/SM/CB/DC/AN/EN/GS/OR-xx`, bảng tiến độ, mục "Cần play-test")

## Cấu trúc thư mục

```
Assets/_GamePlay/
  Scripts/
    Player/        PlayerController, PlayerStateMachine, State + State/*, AnimationController,
                   PlayerAnimationAttack (nhận animation event), PlayerInputHandler, PlayerStatsSO
    Combat/        AttackData(SO), Hitbox, Bullet, WeaponManager, WeaponAttackManager, WeaponDataSO,
                   Melee/GunAttackHandler, Shield, Team, DamagePopupSpawner, Effect/*
    Health/        Health, Damageable, Death, InvulnerabilityBlink
    Enemy/         EnemyController, EnemyDataSO, States/*  (Enemy.cs = bia tập, không có AI)
    StateMachine/  StateMachine<TState>, IState (dùng chung player + enemy)
    Debug/         DebugCheatPanel
  Framework/       Bootstrap, PrefabRegistrySO, Service, Pool, Observer, ...
  Data/            PlayerStats, PrefabRegistry, Enemy/*Data
  DataCombat/      <Weapon>/<Weapon>WeaponData + *AttackData, Enemy/*AttackData
  Prefabs/         Bootstrap, bullet, DamagePopup, ghostSpawn, Enemy/MeleeEnemy
  Anim/Animation/  <Weapon>/{state}_{Weapon}.anim
  Anim/Enemies/    <Name>/{anim}_{Name}.anim + Animator<Name>.controller (sinh tự động)
  Art/Enemies/     <Name>/{id}_{anim}_{nn}.png + {id}_manifest.json, FX/ (sinh từ Art/EnemyConcepts)
  Editor/          asmdef StickFigure.Editor — EnemyArtImporter
Tools/ArtPipeline/ build.py: sprite sheet AI-gen -> frame PNG sạch (Python, ngoài Assets)
```

## Kiến trúc

### Player
- `PlayerController` giữ component + helper di chuyển; logic nằm trong state. `PlayerStates` tạo mỗi state **1 lần** (`player.States.Idle`...), không `new` state khi chuyển.
- `PlayerStateMachine` bọc `StateMachine<State>`: chặn `ChangeState` lồng trong Enter/Exit, cảnh báo (Editor) khi đổi state nhiều lần / frame. **Luôn `return` ngay sau `ChangeState`** trong Update.
- State cha: `GroundedState` (Idle, Run, Land), `AirborneState` (Jump, Fall) gom transition chung. Thứ tự `if` = độ ưu tiên.
- Bị đánh: `Damageable.OnDamaged` → `HurtState` (`stats.hurtDuration`) → `StunState` nếu đang choáng → Idle/Fall; bật i-frame `stats.invincibilityDuration`.
- Hết máu: `Health.OnDeath` → `DeadState` (animation `death_{Weapon}`) → sau `stats.respawnDelay` gọi `PlayerController.Respawn()` (điểm hồi sinh: `SetSpawnPoint`). `Death` của Player để `deactivateOnDeath = false`.
- Input: `PlayerInputHandler` buffer nút 0.1s — dùng xong phải `Consume*()`. Coyote time qua `player.CanGroundJump`.
- Chỉ số: asset `Data/PlayerStats` (`PlayerStatsSO`), truy cập `player.stats.X`.

### Combat
- **Đòn đánh = `AttackData` (struct)** tạo từ `AttackDataSO` lúc ra đòn: crit roll 1 lần, `FinalDamage`, `sourceTeam`, `attackerPosition` (hướng knockback).
- **Melee**: animation event → `PlayerAnimationAttack.FighterAttack(int)` / `SwordAttack(int)` / `FighterAirAttack()` → `WeaponAttackManager.GetAttackData(weapon, step)` → `Hitbox.CheckHit`. Hitbox dedupe theo `IDamageable` (1 đòn trúng 1 mục tiêu 1 lần).
- **Ranged**: **không dùng animation event** — `GunAttackHandler.Attack` / `AirAttackState.Enter` gọi `PlayAttack` rồi `PlayerAnimationAttack.FireProjectile(step)` trong cùng frame (event ở đầu clip không chắc bắn lại khi phát lại cùng state). Đạn sinh tại `bulletSpawnPoint` / `airBulletSpawnPoint` = đầu nòng ở frame bắn (đuôi đạn trùng đầu nòng), bay theo world space; prefab đạn = `WeaponDataSO.projectile`.
- **Nhận damage**: `Damageable.TakeDamage` theo thứ tự: chết / i-frame / cùng team → bỏ qua · `IDamageBlocker` (Shield) chặn → `OnBlocked` · trừ `Health` → `OnDamaged`. Popup, knockback, status effect, Hurt đều **nghe `OnDamaged`**, không gọi trực tiếp.
- **Status effect**: kế thừa `StatusEffect` (Stun, Burn, Freeze), tự bật theo `AttackData.effectType` + `effectDuration`, tự dọn khi object tắt. Knockback không phải effect — luôn theo `knockbackForce`.
- Damage & máu dùng **`int`** ở mọi nơi.

### Enemy
- `EnemyController` + `StateMachine<EnemyState>`: Patrol → Chase → Attack → Hurt / PhaseChange / Taunt / Dead. Chỉ số trong `EnemyDataSO`, mỗi đòn là 1 `EnemyAttackSO` (Telegraph → Active với các mốc `hitTimes` → Recovery; chọn theo tầm / phase / cooldown / weight). Choáng / đóng băng ⇒ đứng yên (đang ra đòn thì bị ngắt).
- Animation: `EnemyAnimator` ở child Visual, `Play("smash")` → state `smash_{animSet}` (clip sinh bởi EnemyArtImporter). Thiếu state ⇒ bỏ qua, state vẫn chạy theo timer.
- Damage của đòn: `EnemyAttackSO.damage` + hitbox (`attackHitbox` hoặc theo tên trong `namedHitboxes`). Đạn / vùng nguy hiểm / triệu hồi: component implement `IEnemyAttackHandler` nhận mốc trúng.
- Phase (`EnemyDataSO.phases`, ngưỡng % máu giảm dần): qua ngưỡng ⇒ `EnemyPhaseChangeState` (bất tử + animation), rồi đổi tốc độ / nhịp đánh / idle / màu; đòn lọc theo `minPhase..maxPhase`.
- Prefab `Prefabs/Enemy/MeleeEnemy`: **giữ `EnemyController` đứng trước các StatusEffect** trong danh sách component (thứ tự nghe `OnDamaged`). Màu sprite chỉ đổi qua `EnemyController` (phase × tô báo hiệu × mờ xác).
- Chết: `EnemyDeadState` phát `death` + mờ dần `corpseLinger`; `Death.DespawnDelay` (EnemyController tự gán) chờ xong mới `Recycle` về pool. `Health`, `Damageable`, effect, `Shield` tự reset khi Spawn lại.

### Service & prefab
- `Bootstrap` (prefab trong scene) đăng ký `Service.Set<Bootstrap>` và `Service.Set<PrefabRegistrySO>`. Lấy prefab runtime: `Service.Get<PrefabRegistrySO>().bullet`.
- Spawn/huỷ object lặp lại (đạn, popup, ghost, enemy) qua `ObjectPool.Spawn/Recycle`, không `Instantiate/Destroy`.

## Quy ước

| Chủ đề | Quy ước |
|---|---|
| Tên clip / state Animator | `{state}_{Weapon}` (vd `run_Sword`), combo `attack{n}_{Weapon}`, trên không `airAttack_{Weapon}`, súng `attack_Gun`. `AnimationController` log lỗi nếu thiếu state |
| Tham số `int` của animation event đòn đánh | `0` = đòn trên không, `1..N` = bước combo (khớp `WeaponDataSO.comboSteps`) |
| Rigidbody2D nhân vật | `Interpolate` (physics 50Hz vs camera mỗi frame — tắt là bị rung) |
| Animation đòn đánh | `PlayAttack`/`PlayAirAttack` luôn phát lại từ đầu (`Play(hash, 0, 0)`) |
| Team | `Damageable.team`: Player / Enemy / Neutral. Đòn mang `sourceTeam`; cùng team không trúng nhau |
| Layer | 6 Player · 7 Enemy · 8 Ground · 9 Ladder · 10 Wall. `Hitbox.targetLayer` lọc va chạm |
| Cheat | `DebugCheatPanel`: F1 Fighter · F2 Sword · F3 Gun (chỉ Editor / Development Build) |
| Asset | Di chuyển / đổi tên asset **trong Unity** (hoặc qua MCP) để giữ GUID trong `.meta` |
| Ngôn ngữ | Comment tiếng Việt, identifier tiếng Anh |

## Cách làm thường gặp

**Thêm đòn combo cho vũ khí cận chiến**: thêm `AttackDataSO` vào `comboSteps` của `<Weapon>WeaponData` → tạo clip `attack{n}_{Weapon}` + state cùng tên trong Animator → animation event gọi `FighterAttack`/`SwordAttack` với int = `n`. Số đòn combo tự theo `comboSteps.Count`.

**Thêm vũ khí mới**: thêm giá trị `WeaponType` → tạo `WeaponDataSO` (`attackStyle`, `comboSteps`, `airAttack`, `projectile` nếu Ranged) → thêm vào list `weapons` của `WeaponAttackManager` trên Player → tạo đủ clip `{state}_{Weapon}` (idle, run, jump, fall, land, dash, slide, climb, wallSlide, hit, attack..., airAttack). Melee cần thêm hàm event + hitbox trong `PlayerAnimationAttack`.

**Thêm loại enemy**: dựng art (mục dưới) → tạo `EnemyDataSO` + mỗi đòn 1 `EnemyAttackSO` (+ `AttackDataSO` damage) → duplicate prefab enemy có sẵn, gán data, `EnemyAnimator.animSet` = tên nhân vật, controller `Animator<Name>`, chỉnh hitbox / collider theo sprite. Hành vi mới: ưu tiên làm bằng data (đòn, phase); cần logic riêng thì thêm `IEnemyAttackHandler` hoặc `EnemyState`.

**Dựng lại / thêm art enemy** (từ sprite sheet trong `Art/EnemyConcepts`): sửa cấu hình sheet trong `Tools/ArtPipeline/build.py` (toạ độ = pixel sheet gốc: `band` mỗi animation, nền ca rô `tones`/`period`, vùng xoá nhãn) → `python -I -B Tools/ArtPipeline/build.py <Name> --preview <thư mục>` (soi ảnh preview: cột cuối là onion skin) → `refresh_unity` → menu **Tools/Enemies/Import Enemy Art** (pivot/PPU + clip `{anim}_{Name}` + controller). Không sửa tay PNG trong `Art/Enemies` (bị ghi đè lần build sau).

**Thêm status effect**: thêm giá trị vào `AttackEffect` (**số mới, không dùng lại số cũ** — asset serialize theo số) → class kế thừa `StatusEffect` (override `EffectType`, `Begin/End` hoặc `Tick`) → gắn component cho object nhận effect.

## Quy trình làm việc (Claude / Unity MCP)

- User thường mở **nhiều Unity Editor** cùng lúc: trước khi dùng Unity MCP luôn đọc `mcpforunity://instances` và `set_active_instance` đúng **StickFigure** (`D:/Unity/StickFigure`), xác nhận lại `Application.dataPath` trước khi sửa.
- Sau khi sửa script: `refresh_unity` (mode `force` nếu có file mới) → `read_console` lỗi → mới dùng type mới.
- Test: EditMode test ở `Assets/Tests/EditMode` (asmdef `StickFigure.Tests.EditMode`) — chạy sau khi sửa logic combat / state machine / health.
- Editor không chạy frame khi mất focus: test logic bằng `execute_code` gọi trực tiếp method / `Update`, cảm giác chơi để user tự play-test (ghi vào bảng "Cần play-test" trong `BACKLOG.md`).
- Mỗi task backlog xong → cập nhật `BACKLOG.md` + **1 commit riêng** (message bắt đầu bằng mã task) + push `origin main`. Không commit `Assets/_GamePlay/Art/EnemyConcepts/` (concept art của user) trừ khi được yêu cầu.

## Skill name map (canonical → project)

| Canonical (unity-skills) | Project |
|---|---|
| `AppBootstrap` / `GameBootstrap` | `Bootstrap` (prefab, `Service.Set` trong `Awake`) |
| Prefab registry | `PrefabRegistrySO` |
| `GameFlowService`, `ProgressionService`, `CurrencyService`, `AudioService` | chưa có (GS-01..04) |
