using Kore.Utils.Core;
using UnityEngine;

// Hiệu ứng trúng đòn (pool): phát SpriteSequence 1 lần rồi tự Recycle. Xoay / lật ngẫu nhiên cho đỡ lặp.
[RequireComponent(typeof(SpriteSequence))]
public class HitFx : MonoBehaviour
{
    [SerializeField] private float randomRotation = 25f;
    [Tooltip("Lật dọc ngẫu nhiên (nhát chém từ trên xuống / dưới lên) cho đỡ lặp")]
    [SerializeField] private bool randomFlipY;
    [Tooltip("Cỡ ngẫu nhiên ± tỉ lệ này")]
    [SerializeField, Range(0f, 0.5f)] private float randomScale = 0.1f;

    private SpriteSequence sequence;
    private float remain;

    private void Awake()
    {
        sequence = GetComponent<SpriteSequence>();
    }

    private void OnEnable()
    {
        remain = sequence.Duration;
        transform.rotation = Quaternion.Euler(0f, 0f, Random.Range(-randomRotation, randomRotation));
    }

    /// <param name="dirX">Hướng đòn đi tới (+1 sang phải): hiệu ứng lật theo hướng đó</param>
    public void Init(float dirX, float scale)
    {
        scale *= 1f + Random.Range(-randomScale, randomScale);
        float sy = randomFlipY && Random.value < 0.5f ? -scale : scale;
        transform.localScale = new Vector3(dirX < 0f ? -scale : scale, sy, 1f);
    }

    private void Update()
    {
        remain -= Time.deltaTime;
        if (remain <= 0f)
            ObjectPool.Recycle(gameObject);
    }
}
