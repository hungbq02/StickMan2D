using System.Collections;
using UnityEngine;

public class GhostSpriteSpawner : MonoBehaviour
{
    private SpriteRenderer spriteRenderer;

    [SerializeField] private float spawnInterval = 0.1f;
    [SerializeField] private float fadeDuration = 0.15f; // time to fade out the ghost sprite

    private float spawnTimer = 0f;

    private bool isSpawning = false;
    private PlayerController player;

    void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        player = GetComponentInParent<PlayerController>();
    }

    void Update()
    {
        if (!isSpawning) return;

        spawnTimer += Time.deltaTime;
        if (spawnTimer >= spawnInterval)
        {
            SpawnGhost();
            spawnTimer = 0f;
        }
    }

    public void BeginSpawn()
    {
        isSpawning = true;
        spawnTimer = 0f;
    }

    public void StopSpawn()
    {
        isSpawning = false;
    }

    private void SpawnGhost()
    {
        GameObject ghost = PoolManager.Instance.ghostSpritePool.GetObject();

        ghost.transform.position = transform.position;
        ghost.SetActive(true);

        SpriteRenderer ghostRenderer = ghost.GetComponent<SpriteRenderer>();
        ghostRenderer.sprite = spriteRenderer.sprite;

        // Flip sprite based on player's facing direction
        if (player.FacingDirection == -1)
            ghostRenderer.flipX = true;
        else
            ghostRenderer.flipX = false;
        ghostRenderer.color = new Color(1f, 1f, 1f, 1f);
        StartCoroutine(FadeAndReturnGhost(ghost));
    }

/*    private IEnumerator ReturnGhostToPool(GameObject ghost)
    {
        yield return new WaitForSeconds(0.1f);
        PoolManager.Instance.ghostSpritePool.ReturnObject(ghost);
    }*/
    private IEnumerator FadeAndReturnGhost(GameObject ghost)
    {
        SpriteRenderer ghostRenderer = ghost.GetComponent<SpriteRenderer>();
        Color originalColor = ghostRenderer.color;
        float elapsed = 0f;

        while (elapsed < fadeDuration)
        {
            elapsed += Time.deltaTime;
            float alpha = Mathf.Lerp(1f, 0f, elapsed / fadeDuration); // Giảm dần alpha
            ghostRenderer.color = new Color(originalColor.r, originalColor.g, originalColor.b, alpha);
            yield return null;
        }

        // done fading, return ghost to pool
        ghostRenderer.color = new Color(originalColor.r, originalColor.g, originalColor.b, 1f); // Reset alpha về 1
        PoolManager.Instance.ghostSpritePool.ReturnObject(ghost);
    }

}
