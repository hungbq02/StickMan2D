using UnityEngine;
#if UNITY_EDITOR || DEVELOPMENT_BUILD
using UnityEngine.InputSystem;
#endif

// Phím cheat khi test: F1 Fighter · F2 Sword · F3 Gun.
// Class luôn tồn tại (component gắn trong scene không bị "missing script" ở release),
// nhưng toàn bộ logic chỉ có trong Editor / Development Build.
public class DebugCheatPanel : MonoBehaviour
{
#if UNITY_EDITOR || DEVELOPMENT_BUILD
    [Tooltip("Để trống => tự tìm PlayerController trong scene")]
    [SerializeField] private PlayerController player;

    private void Awake()
    {
        if (player == null)
            player = FindObjectOfType<PlayerController>();
    }

    private void Update()
    {
        Keyboard keyboard = Keyboard.current;
        if (keyboard == null || player == null) return;

        if (keyboard.f1Key.wasPressedThisFrame)
            player.WeaponManager.SetWeaponType(WeaponType.Fighter);
        else if (keyboard.f2Key.wasPressedThisFrame)
            player.WeaponManager.SetWeaponType(WeaponType.Sword);
        else if (keyboard.f3Key.wasPressedThisFrame)
            player.WeaponManager.SetWeaponType(WeaponType.Gun);
    }
#endif
}
