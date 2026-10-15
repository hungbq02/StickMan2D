using UnityEngine;
using TMPro;

public class DamagePopup : MonoBehaviour
{
    [SerializeField] private TextMeshPro textMesh;

    private const float LifeTime = 0.5f;
    private const float FadeSpeed = 3f;
    private const float Gravity = -15f;

    private float timer;
    private Vector3 velocity;
    private float rotateSpeed;
    private Color baseColor;

    private void Awake()
    {
        if (textMesh == null)
            textMesh = GetComponent<TextMeshPro>();

        baseColor = textMesh.color;
    }

    public void Init(int amount)
    {
        textMesh.SetText(amount.ToString());
        baseColor.a = 1f;
        textMesh.color = baseColor;

        timer = LifeTime;

        velocity = new Vector3(
            Random.Range(-0.5f, 0.5f),
            Random.Range(1f, 1.5f),
            0f) * 5f;

        rotateSpeed = Random.Range(-15f, 15f);

        transform.localScale = Vector3.one;
        transform.localEulerAngles = Vector3.zero;
    }

    private void Update()
    {
        UpdateMovement();
        UpdateFade();
    }

    private void UpdateMovement()
    {
        transform.position += velocity * Time.deltaTime;
        velocity.y += Gravity * Time.deltaTime;
        transform.Rotate(0f, 0f, rotateSpeed * Time.deltaTime);
    }

    private void UpdateFade()
    {
        timer -= Time.deltaTime;
        if (timer > 0f) return;

        baseColor.a -= FadeSpeed * Time.deltaTime;
        textMesh.color = baseColor;

        if (baseColor.a <= 0f)
        {
            ObjectPool.Recycle(gameObject);
        }
    }
}
