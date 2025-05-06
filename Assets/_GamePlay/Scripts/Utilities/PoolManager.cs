using UnityEngine;

public class PoolManager : MonoBehaviour
{
    public static PoolManager Instance;
    public Pooler ghostSpritePool;
    public Pooler bulletPool;
    public Pooler textDamagePool;



    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }
    private void Start()
    {
        if (ghostSpritePool == null)
        {
            ghostSpritePool = GameObject.FindWithTag("GhostSpritePool").GetComponent<Pooler>();
        }
        if (bulletPool == null)
        {
            bulletPool = GameObject.FindWithTag("BulletPool").GetComponent<Pooler>();
        }
        if (textDamagePool == null)
        {
            textDamagePool = GameObject.FindWithTag("TextDamagePool").GetComponent<Pooler>();
        }
    }
}
