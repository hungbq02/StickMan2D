using UnityEngine;

// Camera 2D theo player: SmoothDamp (độc lập FPS) + dead zone + giới hạn trong biên level.
[RequireComponent(typeof(Camera))]
public class CameraFollow : MonoBehaviour
{
    [Tooltip("Transform của nhân vật (player) mà camera sẽ theo dõi")]
    public Transform target;

    [Tooltip("Khoảng cách offset giữa camera và nhân vật (theo X, Y)")]
    public Vector3 offset;

    [Tooltip("Thời gian (giây) để camera bắt kịp — 0 = bám cứng")]
    [SerializeField] private float smoothTime = 0.12f;

    [Tooltip("Nửa kích thước vùng chết quanh tâm camera: player di chuyển trong vùng này thì camera đứng yên")]
    [SerializeField] private Vector2 deadZone = new Vector2(0.5f, 0.75f);

    [Header("Level Bounds")]
    [SerializeField] private bool useBounds = true;
    [Tooltip("Biên level theo world space (min = góc dưới trái). Camera không nhìn ra ngoài vùng này")]
    [SerializeField] private Rect bounds = new Rect(-7.7f, -2.7f, 32.2f, 28f);

    private Camera cam;
    private Vector3 velocity;
    private Vector3 focus; // điểm camera đang nhắm (trước khi clamp biên)

    private void Awake()
    {
        cam = GetComponent<Camera>();
    }

    private void OnEnable()
    {
        SnapToTarget();
    }

    // Đặt camera ngay vào vị trí đích (dùng khi bắt đầu level / respawn)
    public void SnapToTarget()
    {
        if (target == null) return;
        focus = target.position + offset;
        velocity = Vector3.zero;
        transform.position = WithZ(Clamp(focus));
    }

    private void LateUpdate()
    {
        if (target == null) return;

        // Dead zone: chỉ dời điểm nhắm khi player ra khỏi vùng chết
        Vector3 desired = target.position + offset;
        Vector3 delta = desired - focus;
        if (Mathf.Abs(delta.x) > deadZone.x) focus.x = desired.x - Mathf.Sign(delta.x) * deadZone.x;
        if (Mathf.Abs(delta.y) > deadZone.y) focus.y = desired.y - Mathf.Sign(delta.y) * deadZone.y;

        Vector3 goal = WithZ(Clamp(focus));
        transform.position = smoothTime > 0f
            ? Vector3.SmoothDamp(transform.position, goal, ref velocity, smoothTime)
            : goal;
    }

    private Vector3 Clamp(Vector3 position)
    {
        if (!useBounds || cam == null || !cam.orthographic) return position;

        float halfHeight = cam.orthographicSize;
        float halfWidth = halfHeight * cam.aspect;
        position.x = ClampAxis(position.x, bounds.xMin + halfWidth, bounds.xMax - halfWidth);
        position.y = ClampAxis(position.y, bounds.yMin + halfHeight, bounds.yMax - halfHeight);
        return position;
    }

    // Level nhỏ hơn khung hình theo trục này => căn giữa
    private static float ClampAxis(float value, float min, float max)
    {
        return min > max ? (min + max) * 0.5f : Mathf.Clamp(value, min, max);
    }

    private Vector3 WithZ(Vector3 position)
    {
        position.z = transform.position.z;
        return position;
    }

    private void OnDrawGizmosSelected()
    {
        if (useBounds)
        {
            Gizmos.color = Color.cyan;
            Gizmos.DrawWireCube(bounds.center, bounds.size);
        }
        Gizmos.color = Color.green;
        Gizmos.DrawWireCube(Application.isPlaying ? focus : transform.position, new Vector3(deadZone.x * 2f, deadZone.y * 2f));
    }
}
