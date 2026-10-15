using TMPro;
using UnityEngine;
using UnityEngine.UI;

// HUD player: thanh máu (fill chạy mượt, đổi màu theo %) + vũ khí đang cầm. Nghe event, không poll Health.
public class PlayerHUD : MonoBehaviour
{
    [Tooltip("Để trống => tự tìm PlayerController trong scene")]
    [SerializeField] private PlayerController player;

    [Header("Health")]
    [SerializeField] private Image healthFill;          // Image Type = Filled, Horizontal
    [SerializeField] private TextMeshProUGUI healthText;
    [SerializeField] private Gradient healthColor;
    [Tooltip("Tốc độ fill chạy tới giá trị mới (phần / giây)")]
    [SerializeField] private float fillSpeed = 2f;

    [Header("Weapon")]
    [SerializeField] private TextMeshProUGUI weaponText;

    private Health health;
    private float targetFill = 1f;

    private void Awake()
    {
        if (player == null)
            player = FindObjectOfType<PlayerController>();
    }

    // Start (không phải OnEnable): chạy sau Awake của PlayerController => WeaponManager đã sẵn
    private void Start()
    {
        if (player == null) return;

        health = player.GetComponent<Health>();
        if (health != null)
        {
            health.OnHealthChanged += HandleHealthChanged;
            HandleHealthChanged(health.Current, health.Max);
            if (healthFill != null) healthFill.fillAmount = targetFill; // không chạy hiệu ứng lúc mở
        }

        player.WeaponManager.OnWeaponChanged += HandleWeaponChanged;
        HandleWeaponChanged(player.WeaponManager.CurrentType);
    }

    private void OnDestroy()
    {
        if (health != null)
            health.OnHealthChanged -= HandleHealthChanged;
        if (player != null && player.WeaponManager != null)
            player.WeaponManager.OnWeaponChanged -= HandleWeaponChanged;
    }

    private void Update()
    {
        if (healthFill == null || Mathf.Approximately(healthFill.fillAmount, targetFill)) return;

        healthFill.fillAmount = Mathf.MoveTowards(healthFill.fillAmount, targetFill, fillSpeed * Time.unscaledDeltaTime);
        healthFill.color = healthColor.Evaluate(healthFill.fillAmount);
    }

    private void HandleHealthChanged(int current, int max)
    {
        targetFill = max > 0 ? (float)current / max : 0f;
        if (healthText != null)
            healthText.SetText("{0}/{1}", current, max);
        if (healthFill != null)
            healthFill.color = healthColor.Evaluate(healthFill.fillAmount);
    }

    private void HandleWeaponChanged(WeaponType weaponType)
    {
        if (weaponText != null)
            weaponText.SetText(weaponType.ToString());
    }
}
