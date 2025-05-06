using System.Collections;
using UnityEngine;
using TMPro;

public class DamagePopup : MonoBehaviour
{
    private TextMeshPro textMesh;

    private Color textColor;
    private Transform playerTransform;

    private float disappearTimer;
    private float disappearDuration = 0.5f;
    private float fadeOutSpeed = 3f;
    private Vector3 moveVector;
    private float gravity = -15f;
    private float rotateSpeed;

    public void SetUp(int amount)
    {
        textMesh = GetComponent<TextMeshPro>();
        playerTransform = Camera.main.transform;

        textColor = textMesh.color;
        textColor.a = 1f;
        textMesh.color = textColor;

        textMesh.SetText(amount.ToString());

        disappearTimer = disappearDuration;

        // Văng theo hướng chéo + random
        float x = Random.Range(-0.5f, 0.5f);
        float y = Random.Range(1f, 1.5f);
        moveVector = new Vector3(x, y, 0f) * 5f;

        // Rotate random
        rotateSpeed = Random.Range(-90f, 90f);

        // Reset scale 
        transform.localScale = Vector3.one;
    }

    private void Update()
    {
        // Move
        transform.position += moveVector * Time.deltaTime;
        moveVector.y += gravity * Time.deltaTime;

        // Rotate
        transform.Rotate(0f, 0f, rotateSpeed * Time.deltaTime);

        // Timer
        disappearTimer -= Time.deltaTime;
        if (disappearTimer < 0)
        {
            textColor.a -= fadeOutSpeed * Time.deltaTime;
            textMesh.color = textColor;

            if (textColor.a <= 0f)
            {
                ResetText();
                PoolManager.Instance.textDamagePool.ReturnObject(gameObject);
            }
        }

        // Always face camera
        transform.LookAt(2 * transform.position - playerTransform.position);
    }

    private void ResetText()
    {
        gameObject.transform.SetParent(PoolManager.Instance.textDamagePool.transform);
        disappearTimer = disappearDuration;
        moveVector = Vector3.zero;
        textColor.a = 1f;
        textMesh.color = textColor;
        transform.localEulerAngles = Vector3.zero;
    }
}
