using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    [SerializeField] private Collider2D bodyCollider;
    [SerializeField] private Collider2D feetCollider;
    private Rigidbody2D rb;

    // movement
    private Vector2 moveVelocity;
    private bool isFacingRight;

    //
    private RaycastHit2D groundHit;
    private RaycastHit2D headHit;
    private bool isGrounded;
    private bool bumpedHead;


    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
    }

}
