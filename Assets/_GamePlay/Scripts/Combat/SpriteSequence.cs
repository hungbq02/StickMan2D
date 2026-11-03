using UnityEngine;

// Phát chuỗi sprite theo fps cho hiệu ứng đơn giản (cột lửa, minion 2 frame...) — không cần Animator / clip.
[RequireComponent(typeof(SpriteRenderer))]
public class SpriteSequence : MonoBehaviour
{
    [SerializeField] private Sprite[] frames;
    [SerializeField] private float fps = 12f;
    [SerializeField] private bool loop;
    [SerializeField] private bool playOnEnable = true;

    private SpriteRenderer sr;
    private float time;
    private bool playing;

    public bool IsPlaying => playing;
    public float Duration => frames != null && fps > 0f ? frames.Length / fps : 0f;

    private void Awake()
    {
        sr = GetComponent<SpriteRenderer>();
    }

    private void OnEnable()
    {
        if (playOnEnable) Play();
        else ShowFrame(0);
    }

    public void Play()
    {
        time = 0f;
        playing = frames != null && frames.Length > 0;
        ShowFrame(0);
    }

    public void ShowFrame(int index)
    {
        if (sr == null) sr = GetComponent<SpriteRenderer>();
        if (frames != null && frames.Length > 0)
            sr.sprite = frames[Mathf.Clamp(index, 0, frames.Length - 1)];
    }

    private void Update()
    {
        if (!playing) return;
        time += Time.deltaTime;
        int index = Mathf.FloorToInt(time * fps);
        if (index >= frames.Length)
        {
            if (!loop)
            {
                playing = false;
                ShowFrame(frames.Length - 1);
                return;
            }
            index %= frames.Length;
        }
        ShowFrame(index);
    }
}
