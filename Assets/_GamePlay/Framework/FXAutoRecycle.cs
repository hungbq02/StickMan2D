using UnityEngine;
using System.Collections;

[RequireComponent(typeof(ParticleSystem))]
public class FXAutoRecycle : MonoBehaviour
{
    public ParticleSystem particleSys;
    float countdown = .5f;

    void OnEnable()
    {
        if (particleSys == null)
            particleSys = GetComponent<ParticleSystem>();
    }

    private void Reset()
    {
        if (particleSys == null)
            particleSys = GetComponent<ParticleSystem>();
    }


    void Update()
    {
        countdown -= Time.deltaTime;
        if (countdown <= 0)
        {
            countdown += .5f;
            if (!particleSys.IsAlive(true))
            {
                this.transform.parent = null;
                ObjectPool.Recycle(this.gameObject);
            }
        }
    }
}