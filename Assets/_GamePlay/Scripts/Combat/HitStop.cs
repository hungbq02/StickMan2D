using UnityEngine;

// Hit-stop: khựng thời gian rất ngắn khi đòn trúng để đòn đánh "có lực".
// Nhiều yêu cầu chồng nhau → giữ mốc kết thúc xa nhất, timeScale thấp nhất; hết thì trả lại timeScale cũ.
public static class HitStop
{
    private static HitStopRunner runner;

    public static bool IsActive => runner != null && runner.Active;

    /// <param name="duration">Thời gian khựng (giây thật, không bị timeScale ảnh hưởng)</param>
    /// <param name="timeScale">Tốc độ thời gian trong lúc khựng (0.05 = gần như đứng yên)</param>
    public static void Request(float duration, float timeScale = 0.05f)
    {
        if (duration <= 0f || !Application.isPlaying) return;
        if (runner == null)
        {
            var go = new GameObject("[HitStop]");
            Object.DontDestroyOnLoad(go);
            runner = go.AddComponent<HitStopRunner>();
        }
        runner.Begin(duration, timeScale);
    }
}

public class HitStopRunner : MonoBehaviour
{
    private float until;
    private float restoreScale = 1f;

    public bool Active { get; private set; }

    public void Begin(float duration, float scale)
    {
        if (!Active)
        {
            restoreScale = Time.timeScale;
            Active = true;
        }
        until = Mathf.Max(until, Time.unscaledTime + duration);
        Time.timeScale = Mathf.Min(Time.timeScale, scale);
    }

    private void Update()
    {
        if (Active && Time.unscaledTime >= until)
            End();
    }

    private void OnDisable()
    {
        if (Active) End();
    }

    private void End()
    {
        Time.timeScale = restoreScale;
        Active = false;
    }
}
