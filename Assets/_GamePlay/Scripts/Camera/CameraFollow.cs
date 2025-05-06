using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    [Tooltip("Transform của nhân vật (player) mà camera sẽ theo dõi")]
    public Transform target;

    [Tooltip("Độ trễ khi camera theo nhân vật, càng cao thì càng mượt")]
    public float smoothSpeed = 0.125f;

    [Tooltip("Khoảng cách offset giữa camera và nhân vật (theo X, Y)")]
    public Vector3 offset;

    private void LateUpdate()
    {
        if (target == null) return;

        Vector3 desiredPosition = target.position + offset;
        Vector3 smoothedPosition = Vector3.Lerp(transform.position, desiredPosition, smoothSpeed);
        transform.position = new Vector3(smoothedPosition.x, smoothedPosition.y, transform.position.z);
    }
}
