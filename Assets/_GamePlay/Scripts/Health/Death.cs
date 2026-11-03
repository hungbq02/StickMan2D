using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Death : MonoBehaviour
{
    [Header("Death Behavior")]
    [SerializeField] private bool deactivateOnDeath = true;
    [SerializeField] private float destroyDelay = -1f;
    [Tooltip("Chờ bao lâu (giây) rồi mới về pool / tắt — để phát animation chết. Có thể gán từ code (EnemyController)")]
    [SerializeField] private float despawnDelay;

    private Health health;
    private Coroutine despawnRoutine;

    public float DespawnDelay
    {
        get => despawnDelay;
        set => despawnDelay = Mathf.Max(0f, value);
    }

    private void Awake()
    {
        health = GetComponent<Health>();
    }

    private void OnEnable()
    {
        health.OnDeath += HandleDeath;
    }

    private void OnDisable()
    {
        health.OnDeath -= HandleDeath;
        if (despawnRoutine != null)
        {
            StopCoroutine(despawnRoutine);
            despawnRoutine = null;
        }
    }

    private void HandleDeath()
    {
        OnDeath();

        if (despawnDelay > 0f)
            despawnRoutine = StartCoroutine(DespawnAfterDelay());
        else
            Despawn();
    }

    private IEnumerator DespawnAfterDelay()
    {
        yield return new WaitForSeconds(despawnDelay);
        despawnRoutine = null;
        Despawn();
    }

    private void Despawn()
    {
        // Object lấy từ ObjectPool => trả về pool (Health/Damageable/effect tự reset khi Spawn lại)
        if (ObjectPool.IsSpawned(gameObject))
        {
            ObjectPool.Recycle(gameObject);
            return;
        }

        if (deactivateOnDeath)
        {
            gameObject.SetActive(false);
        }

        if (destroyDelay > 0f)
        {
            Destroy(gameObject, destroyDelay);
        }
    }

    protected virtual void OnDeath()
    {
        // hook cho subclass hoặc component khác
    }
}
