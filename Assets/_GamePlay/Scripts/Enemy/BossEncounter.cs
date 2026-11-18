using System;
using System.Collections;
using UnityEngine;

// Trận boss: boss "ngủ" (EnemyController tắt, đứng idle) tới khi player bước vào trigger của object này
// -> bật boss, dựng tường đấu trường, BossHealthBar hiện (event Started).
// Boss chết -> đòn kết liễu khựng + chậm thời gian + rung, minion chết theo; đợi xác boss tan hết (chuỗi khói của EnemyDeadState)
// rồi mới mở tường, ẩn thanh máu (Ended). Player chết giữa trận -> sau resetDelay boss về chỗ cũ, đầy máu, ngủ lại.
[RequireComponent(typeof(Collider2D))]
public class BossEncounter : MonoBehaviour
{
    public static event Action<BossEncounter> Started;
    public static event Action<BossEncounter> Ended;

    [SerializeField] private EnemyController boss;
    [SerializeField] private string displayName = "Boss";
    [Tooltip("Bật khi vào trận (chặn đường ra), tắt khi xong")]
    [SerializeField] private GameObject[] arenaWalls;
    [SerializeField] private float resetDelay = 1f;

    [Header("Boss bị hạ")]
    [Tooltip("Khựng ở đòn kết liễu (giây thật)")]
    [SerializeField] private float killHitStop = 0.3f;
    [Tooltip("Sau khựng: chậm thời gian từ mức này về 1 trong slowMoDuration giây thật")]
    [SerializeField, Range(0.05f, 1f)] private float slowMoScale = 0.3f;
    [SerializeField] private float slowMoDuration = 0.8f;
    [SerializeField, Range(0f, 1f)] private float killShake = 0.9f;

    private Vector3 bossHome;
    private Health playerHealth;
    private EnemySummoner summoner;
    private bool defeating;

    public EnemyController Boss => boss;
    public string DisplayName => displayName;
    public bool InProgress { get; private set; }

    private void Awake()
    {
        GetComponent<Collider2D>().isTrigger = true;
        if (boss != null)
        {
            bossHome = boss.transform.position;
            summoner = boss.GetComponent<EnemySummoner>();
        }
        SetWalls(false);
    }

    // Start chạy sau OnEnable của mọi object => boss đã khởi tạo xong rồi mới cho ngủ
    private void Start()
    {
        if (boss != null) boss.enabled = false;
    }

    private void OnDisable()
    {
        if (InProgress) Finish();
        if (defeating)
        {
            defeating = false; // coroutine bị ngắt giữa lúc chậm thời gian => trả tốc độ thường
            if (!HitStop.IsActive) Time.timeScale = 1f;
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (InProgress || boss == null || !boss.gameObject.activeInHierarchy || !boss.Health.IsAlive) return;
        PlayerController player = other.GetComponentInParent<PlayerController>();
        if (player == null) return;
        playerHealth = player.GetComponent<Health>();
        if (playerHealth != null && !playerHealth.IsAlive) return;
        Begin();
    }

    private void Begin()
    {
        InProgress = true;
        if (playerHealth != null) playerHealth.OnDeath += HandlePlayerDeath;
        boss.Health.OnDeath += HandleBossDeath;
        boss.enabled = true;
        SetWalls(true);
        Started?.Invoke(this);
    }

    private void HandleBossDeath()
    {
        InProgress = false;
        Unsubscribe();
        if (summoner != null) summoner.KillAll();
        StartCoroutine(Defeat());
    }

    private IEnumerator Defeat()
    {
        defeating = true;
        HitStop.Request(killHitStop, 0.02f); // đè khựng thường của HitFeedback (giữ mốc xa nhất)
        CameraShake.Add(killShake);
        while (HitStop.IsActive) yield return null;

        // chậm thời gian rồi nhả dần về 1 (theo thời gian thật)
        for (float t = 0f; t < slowMoDuration; t += Time.unscaledDeltaTime)
        {
            if (!HitStop.IsActive) Time.timeScale = Mathf.Lerp(slowMoScale, 1f, t / slowMoDuration);
            yield return null;
        }
        if (!HitStop.IsActive) Time.timeScale = 1f;
        defeating = false;

        // thanh máu (đã về 0) + tường giữ tới khi xác boss tan xong
        var death = boss.GetComponent<Death>();
        float timeout = (death != null ? death.DespawnDelay : 2f) + 1f;
        for (float t = 0f; t < timeout && boss.gameObject.activeInHierarchy; t += Time.deltaTime)
            yield return null;

        SetWalls(false);
        Ended?.Invoke(this);
    }

    private void HandlePlayerDeath()
    {
        Finish();
        StartCoroutine(ResetBoss());
    }

    private void Unsubscribe()
    {
        if (playerHealth != null) playerHealth.OnDeath -= HandlePlayerDeath;
        if (boss != null) boss.Health.OnDeath -= HandleBossDeath;
    }

    private void Finish()
    {
        InProgress = false;
        Unsubscribe();
        SetWalls(false);
        if (summoner != null) summoner.DespawnAll();
        Ended?.Invoke(this);
    }

    private IEnumerator ResetBoss()
    {
        yield return new WaitForSeconds(resetDelay);
        if (boss == null || !boss.gameObject.activeInHierarchy) yield break;
        boss.enabled = false; // OnEnable lần vào trận sau đặt lại máu tối đa + phase 0
        boss.Rigidbody.velocity = Vector2.zero;
        boss.Rigidbody.position = bossHome;
        boss.transform.position = bossHome;
        boss.Health.Revive();
        boss.EnterPhase(0);                  // hết màu enrage lúc đứng chờ
        boss.PlayAnim(boss.Data.animIdle);   // thoát tư thế đòn đang dở
    }

    private void SetWalls(bool on)
    {
        if (arenaWalls == null) return;
        foreach (GameObject w in arenaWalls)
            if (w != null) w.SetActive(on);
    }
}
