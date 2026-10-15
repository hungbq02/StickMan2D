using Kore.Utils.Core;
using System.Collections;
using UnityEngine;

public class Enemy : MonoBehaviour
{
    private Rigidbody2D rb;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
    }
}
