using System;
using System.Collections;
using UnityEngine;

// Trận boss: boss "ngủ" (EnemyController tắt, đứng idle) tới khi player bước vào trigger của object này
// -> bật boss, dựng tường đấu trường, BossHealthBar hiện (event Started).
// Boss chết -> mở tường, ẩn thanh máu (Ended). Player chết giữa trận -> sau resetDelay boss về chỗ cũ, đầy máu, ngủ lại.
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

    private Vector3 bossHome;
    private Health playerHealth;
    private EnemySummoner summoner;

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
        Finish();
    }

    private void HandlePlayerDeath()
    {
        Finish();
        StartCoroutine(ResetBoss());
    }

    private void Finish()
    {
        InProgress = false;
        if (playerHealth != null) playerHealth.OnDeath -= HandlePlayerDeath;
        if (boss != null) boss.Health.OnDeath -= HandleBossDeath;
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
