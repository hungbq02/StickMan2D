using UnityEngine;

// Co RectTransform theo Screen.safeArea (tránh notch / camera đục lỗ). Đặt làm con trực tiếp của root Canvas.
[RequireComponent(typeof(RectTransform))]
public class SafeAreaPanel : MonoBehaviour
{
    private Rect appliedArea;

    private void Awake()
    {
        Apply();
    }

    // Xoay màn hình => safe area đổi
    private void Update()
    {
        if (Screen.safeArea != appliedArea)
            Apply();
    }

    private void Apply()
    {
        appliedArea = Screen.safeArea;
        if (Screen.width <= 0 || Screen.height <= 0) return;

        Vector2 anchorMin = appliedArea.position;
        Vector2 anchorMax = appliedArea.position + appliedArea.size;
        anchorMin.x /= Screen.width;
        anchorMin.y /= Screen.height;
        anchorMax.x /= Screen.width;
        anchorMax.y /= Screen.height;

        var rt = (RectTransform)transform;
        rt.anchorMin = anchorMin;
        rt.anchorMax = anchorMax;
    }
}
