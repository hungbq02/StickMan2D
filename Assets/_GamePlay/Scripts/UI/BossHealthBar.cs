using TMPro;
using UnityEngine;
using UnityEngine.UI;

// Thanh máu boss trên HUD: hiện khi BossEncounter bắt đầu, ẩn khi kết thúc. Fill đỏ nhảy ngay, vệt sáng phía sau
// chạy chậm theo (thấy được lượng máu vừa mất). Nghe Health.OnHealthChanged, không poll.
public class BossHealthBar : MonoBehaviour
{
    [SerializeField] private CanvasGroup group;
    [SerializeField] private Image fill;         // Image Type = Filled, Horizontal
    [SerializeField] private Image trailFill;    // cùng kiểu, nằm dưới fill
    [SerializeField] private TextMeshProUGUI nameText;
    [Tooltip("Tốc độ vệt sáng chạy theo (phần / giây)")]
    [SerializeField] private float trailSpeed = 0.5f;
    [SerializeField] private float fadeSpeed = 4f;

    private Health health;
    private float target = 1f;
    private bool visible;

    private void Awake()
    {
        if (group != null) group.alpha = 0f;
        BossEncounter.Started += Show;
        BossEncounter.Ended += Hide;
    }

    private void OnDestroy()
    {
        BossEncounter.Started -= Show;
        BossEncounter.Ended -= Hide;
        Unbind();
    }

    private void Show(BossEncounter encounter)
    {
        Unbind();
        health = encounter.Boss.Health;
        health.OnHealthChanged += HandleHealthChanged;
        if (nameText != null) nameText.SetText(encounter.DisplayName);
        HandleHealthChanged(health.Current, health.Max);
        if (fill != null) fill.fillAmount = target;
        if (trailFill != null) trailFill.fillAmount = target;
        visible = true;
    }

    private void Hide(BossEncounter encounter)
    {
        Unbind();
        visible = false;
    }

    private void Unbind()
    {
        if (health != null) health.OnHealthChanged -= HandleHealthChanged;
        health = null;
    }

    private void HandleHealthChanged(int current, int max)
    {
        target = max > 0 ? (float)current / max : 0f;
        if (fill != null) fill.fillAmount = target;
    }

    private void Update()
    {
        if (group != null)
            group.alpha = Mathf.MoveTowards(group.alpha, visible ? 1f : 0f, fadeSpeed * Time.unscaledDeltaTime);
        if (trailFill != null && trailFill.fillAmount > target)
            trailFill.fillAmount = Mathf.MoveTowards(trailFill.fillAmount, target, trailSpeed * Time.unscaledDeltaTime);
        else if (trailFill != null)
            trailFill.fillAmount = target;
    }
}
