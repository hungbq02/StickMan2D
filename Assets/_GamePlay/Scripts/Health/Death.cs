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
