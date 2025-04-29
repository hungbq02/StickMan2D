using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Death : MonoBehaviour
{
    private ParticleManager particleManager;
    private HeathSystem heathSystem;

    private void Awake()
    {
        particleManager = GetComponent<ParticleManager>();
        heathSystem = GetComponent<HeathSystem>();
    }
    private void OnEnable()
    {
        heathSystem.OnHealthZero += HandleDeath;
    }
    private void OnDisable()
    {
        heathSystem.OnHealthZero -= HandleDeath;
    }
    private void HandleDeath()
    {
        Debug.Log("Player is dead!!");
        //particleManager.StartParticles();
        gameObject.SetActive(false);
    }


}
