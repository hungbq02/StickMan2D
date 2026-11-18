using UnityEngine;

// Bộ nút cảm ứng (OnScreenStick / OnScreenButton giả lập Gamepad -> cùng action với bàn phím).
// Chỉ hiện trên thiết bị di động; trên PC tắt để virtual gamepad không giành control scheme của bàn phím.
public class TouchControls : MonoBehaviour
{
    [Tooltip("Bật trong Editor / PC để thử bằng chuột")]
    [SerializeField] private bool showOnDesktop;

    private void Awake()
    {
        gameObject.SetActive(Application.isMobilePlatform || showOnDesktop);
    }
}
