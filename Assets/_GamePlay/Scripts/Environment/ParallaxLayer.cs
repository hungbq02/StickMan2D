using UnityEngine;

// Lớp nền parallax: dời theo camera với hệ số (0 = đứng yên như mặt đất, 1 = dính camera như bầu trời).
// Gắn lên object có SpriteRenderer draw mode Tiled đủ rộng để không lộ mép.
public class ParallaxLayer : MonoBehaviour
{
    [Tooltip("Tỉ lệ đi theo camera trục X (lớp xa → gần 1)")]
    [SerializeField, Range(0f, 1f)] private float factorX = 0.5f;
    [Tooltip("Tỉ lệ đi theo camera trục Y")]
    [SerializeField, Range(0f, 1f)] private float factorY = 0.2f;
    [Tooltip("Lúc Start đặt lớp vào giữa camera (bầu trời) thay vì giữ vị trí đặt trong scene")]
    [SerializeField] private bool centerOnCameraAtStart;

    private Transform cam;
    private Vector3 startPos;
    private Vector3 camStart;

    private void Start()
    {
        if (Camera.main == null)
        {
            enabled = false;
            return;
        }
        cam = Camera.main.transform;
        camStart = cam.position;
        startPos = centerOnCameraAtStart
            ? new Vector3(camStart.x, camStart.y, transform.position.z)
            : transform.position;
    }

    private void LateUpdate()
    {
        Vector3 d = cam.position - camStart;
        transform.position = new Vector3(startPos.x + d.x * factorX, startPos.y + d.y * factorY, startPos.z);
    }
}
