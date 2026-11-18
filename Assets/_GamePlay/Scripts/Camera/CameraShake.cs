using UnityEngine;

// Rung màn hình kiểu "trauma" (giống HitStop: gọi tĩnh từ bất kỳ đâu): Add cộng dồn mức rung 0..1 (có trần 1),
// giảm tuyến tính theo thời gian thật; độ lệch = MaxOffset × trauma² × nhiễu Perlin (nhẹ thì gần như không thấy, nặng mới rung rõ).
// CameraFollow cộng Sample() vào vị trí sau khi bám theo => không làm lệch phần smoothing.
public static class CameraShake
{
    /// <summary>Độ lệch tối đa (unit) khi trauma = 1.</summary>
    public const float MaxOffset = 0.35f;
    /// <summary>Trauma giảm mỗi giây (1 = hết rung sau tối đa 1s).</summary>
    public const float DecayPerSecond = 1.6f;
    private const float Frequency = 22f;

    private static float time;

    public static float Trauma { get; private set; }

    /// <param name="amount">0..1 — gợi ý: đạn 0.08, đòn thường 0.25–0.3, đòn cuối combo 0.45–0.5, bị boss đánh 0.6</param>
    public static void Add(float amount)
    {
        if (amount <= 0f) return;
        Trauma = Mathf.Min(1f, Trauma + amount);
    }

    /// <summary>Gọi 1 lần / frame với deltaTime thật (không bị hit-stop làm chậm).</summary>
    public static Vector3 Sample(float unscaledDeltaTime)
    {
        if (Trauma <= 0f) return Vector3.zero;
        time += unscaledDeltaTime;
        float k = MaxOffset * Trauma * Trauma;
        var offset = new Vector3(
            k * Mathf.Clamp(Mathf.PerlinNoise(time * Frequency, 0.37f) * 2f - 1f, -1f, 1f), // Perlin có thể lố [0,1] chút ít
            k * Mathf.Clamp(Mathf.PerlinNoise(0.71f, time * Frequency) * 2f - 1f, -1f, 1f),
            0f);
        Trauma = Mathf.Max(0f, Trauma - DecayPerSecond * unscaledDeltaTime);
        return offset;
    }

    public static void Reset()
    {
        Trauma = 0f;
        time = 0f;
    }
}
