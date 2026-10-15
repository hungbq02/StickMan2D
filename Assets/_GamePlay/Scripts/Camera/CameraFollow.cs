using UnityEngine;

// Camera 2D theo player: SmoothDamp (độc lập FPS) + dead zone + giới hạn trong biên level.
// Khi rơi: nhìn trước xuống dưới + bám trục Y nhanh hơn, và player không bao giờ ra khỏi vùng an toàn của khung hình.
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

    [Header("Falling")]
    [Tooltip("Thời gian bắt kịp trục Y khi rơi ở tốc độ tối đa (nhỏ hơn smoothTime => bám sát hơn)")]
    [SerializeField] private float fallSmoothTime = 0.04f;
    [Tooltip("Rơi nhanh => dời điểm nhắm xuống dưới tối đa khoảng này (unit) để thấy chỗ sắp đáp")]
    [SerializeField] private float fallLookAhead = 2f;
    [Tooltip("Tốc độ rơi (unit/s) ứng với look-ahead / bám tối đa")]
    [SerializeField] private float fullFallSpeed = 15f;
    [Tooltip("Rơi chậm hơn tốc độ này thì coi như không rơi (tránh camera nhúc nhích khi nhảy thấp)")]
    [SerializeField] private float fallSpeedThreshold = 3f;
    [Tooltip("Thời gian look-ahead chạy tới / trả về")]
    [SerializeField] private float lookAheadSmoothTime = 0.2f;

    [Header("Safe Frame")]
    [Tooltip("Player luôn cách mép khung hình ít nhất tỉ lệ này (theo nửa khung, 0..1) — camera bắt kịp ngay nếu vượt")]
    [SerializeField] private Vector2 safeMargin = new Vector2(0.2f, 0.2f);

    [Header("Level Bounds")]
    [SerializeField] private bool useBounds = true;
    [Tooltip("Biên level theo world space (min = góc dưới trái). Camera không nhìn ra ngoài vùng này")]
    [SerializeField] private Rect bounds = new Rect(-7.7f, -2.7f, 32.2f, 28f);

    private Camera cam;
    private Rigidbody2D targetBody;
    private Vector3 focus; // điểm camera đang nhắm (trước khi clamp biên)
    private float velocityX, velocityY;
    private float lookAheadY, lookAheadVelocity;
    private Vector3 lastTargetPosition;

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
        targetBody = target.GetComponent<Rigidbody2D>();
        focus = target.position + offset;
        lastTargetPosition = target.position;
        velocityX = velocityY = lookAheadY = lookAheadVelocity = 0f;
        transform.position = WithZ(Clamp(focus));
    }

    private void LateUpdate()
    {
        if (target == null) return;
        float dt = Time.deltaTime;
        if (dt <= 0f) return;

        // Dead zone: chỉ dời điểm nhắm khi player ra khỏi vùng chết
        Vector3 desired = target.position + offset;
        Vector3 delta = desired - focus;
        if (Mathf.Abs(delta.x) > deadZone.x) focus.x = desired.x - Mathf.Sign(delta.x) * deadZone.x;
        if (Mathf.Abs(delta.y) > deadZone.y) focus.y = desired.y - Mathf.Sign(delta.y) * deadZone.y;

        // Mức độ rơi 0..1
        float vy = targetBody != null ? targetBody.velocity.y : (target.position.y - lastTargetPosition.y) / dt;
        lastTargetPosition = target.position;
        float fall = Mathf.InverseLerp(fallSpeedThreshold, fullFallSpeed, -vy);

        lookAheadY = Mathf.SmoothDamp(lookAheadY, -fallLookAhead * fall, ref lookAheadVelocity, lookAheadSmoothTime);

        Vector3 goal = Clamp(focus + new Vector3(0f, lookAheadY, 0f));
        Vector3 pos = transform.position;
        if (smoothTime > 0f)
        {
            pos.x = Mathf.SmoothDamp(pos.x, goal.x, ref velocityX, smoothTime);
            pos.y = Mathf.SmoothDamp(pos.y, goal.y, ref velocityY, Mathf.Lerp(smoothTime, fallSmoothTime, fall));
        }
        else
        {
            pos.x = goal.x;
            pos.y = goal.y;
        }

        transform.position = WithZ(Clamp(KeepTargetInFrame(pos)));
    }

    // Player vượt vùng an toàn của khung hình => kéo camera theo ngay (không chờ smoothing)
    private Vector3 KeepTargetInFrame(Vector3 position)
    {
        if (cam == null || !cam.orthographic) return position;

        float halfHeight = cam.orthographicSize;
        float halfWidth = halfHeight * cam.aspect;
        float limitX = halfWidth * (1f - safeMargin.x);
        float limitY = halfHeight * (1f - safeMargin.y);
        Vector3 t = target.position + offset;

        float clampedX = Mathf.Clamp(position.x, t.x - limitX, t.x + limitX);
        float clampedY = Mathf.Clamp(position.y, t.y - limitY, t.y + limitY);
        if (!Mathf.Approximately(clampedX, position.x)) velocityX = 0f;
        if (!Mathf.Approximately(clampedY, position.y)) velocityY = 0f;
        position.x = clampedX;
        position.y = clampedY;
        return position;
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
