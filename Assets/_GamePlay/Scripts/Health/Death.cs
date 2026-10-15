using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Death : MonoBehaviour
{
    [Header("Death Behavior")]
    [SerializeField] private bool deactivateOnDeath = true;
    [SerializeField] private float destroyDelay = -1f;

    private Health health;

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
    }

    private void HandleDeath()
    {
        OnDeath();

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
