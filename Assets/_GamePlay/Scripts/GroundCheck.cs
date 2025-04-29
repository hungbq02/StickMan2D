using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GroundCheck : MonoBehaviour
{
    [Header("Ground Check Settings")]
    public Transform groundCheckPoint;
    public Transform wallCheckPoint;


    public float checkRadius = 0.2f;
    public float wallCheckDistance = 0.2f;

    public LayerMask groundLayer;
    public LayerMask wallLayer;

    public bool IsGrounded { get; private set; }
    public bool IsTouchingWall { get; private set; }

    private PlayerController player;

    void Awake()
    {
        player = GetComponent<PlayerController>();
    }
    void FixedUpdate()
    {
        IsGrounded = Physics2D.OverlapCircle(groundCheckPoint.position, checkRadius, groundLayer);
        IsTouchingWall = Physics2D.Raycast(wallCheckPoint.position, Vector2.right * player.FacingDirection, wallCheckDistance, wallLayer);
    }

/*    void OnDrawGizmosSelected()
    {
        if (groundCheckPoint == null) return;
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(groundCheckPoint.position, checkRadius);

        if (wallCheckPoint == null) return;
        Gizmos.color = Color.red;
        Gizmos.DrawLine(wallCheckPoint.position, wallCheckPoint.position + Vector3.right * player.FacingDirection * wallCheckDistance);

    }*/
}
