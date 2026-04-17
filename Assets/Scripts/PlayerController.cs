using System;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Windows;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(SpriteRenderer))]
[RequireComponent(typeof(Collider2D))]
[RequireComponent(typeof(Animator))]
public class PlayerController : MonoBehaviour
{
    [Header("Components")]
    private Rigidbody2D rb;
    private SpriteRenderer sprite;
    private new Collider2D collider;
    private Animator animator;

    [Header("Properties")]
    [SerializeField] private float walkSpeed = 5f;
    [SerializeField] private float stopVelocityMultiplier = 0.5f;
    [SerializeField] private float jumpForce = 5f;
    [SerializeField] private float jumpDuration = 1f;
    [SerializeField] private float wallJumpBoostMultiplier = 5f;
    [SerializeField] private float wallJumpBoostInterval = 1f;
    [SerializeField] private float headHitMultiplyer = 2f;
    private float jumpEndTime;
    private float wallJumpBoostEndTime;
    private Vector2 moveDirection;

    [Header("States")]
    [SerializeField] private bool isAlive = true;
    private bool IsAlive
    {
        get { return isAlive; }
        set
        {
            isAlive = value;
        }
    }
    [SerializeField] private bool isJumping = false;
    private bool IsJumping
    {
        get { return isJumping; }
        set 
        {
            isJumping = value;
            animator.SetBool("IsJumping", isJumping);
        }
    }
    [SerializeField] private bool isFalling = false;
    private bool IsFalling
    {
        get { return isFalling; }
        set
        {
            isFalling = value;
            animator.SetBool("IsFalling", isFalling);
        }
    }
    [SerializeField] private bool isWalking = false;
    private bool IsWalking
    {
        get { return isWalking; }
        set
        {
            isWalking = value;
            animator.SetBool("IsWalking", isWalking);
        }
    }
    [SerializeField] private bool isGrounded = false;
    private bool IsGrounded
    {
        get { return isGrounded; }
        set
        {
            isGrounded = value;
            animator.SetBool("IsGrounded", isGrounded);
        }
    }
    [SerializeField] private bool isOnWall = false;
    private bool IsOnWall
    {
        get { return isOnWall; }
        set
        {
            isOnWall = value;
        }
    }

    [SerializeField] private LayerMask layerMask;
    private const float raycastDistance = 0.1f;
    private Vector2 originBottom;
    private RaycastHit2D hitBottom;
    private Vector2 originTop;
    private RaycastHit2D hitTop;
    private Vector2 originLeft;
    private RaycastHit2D hitLeft;
    private Vector2 originRight;
    private RaycastHit2D hitRight;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        sprite = GetComponent<SpriteRenderer>();
        collider = GetComponent<Collider2D>();
        animator = GetComponent<Animator>();
    }

    void Start()
    {

    }

    private void Update()
    {
        LocamotionHandler();
    }

    private void LocamotionHandler()
    {
        // Walking
        rb.linearVelocityX = (IsWalking) ? moveDirection.x : rb.linearVelocityX * stopVelocityMultiplier;
        animator.SetFloat("Speed", Mathf.Abs(rb.linearVelocity.x));
        animator.SetFloat("Height", Math.Abs(rb.linearVelocity.y));

        // Jumping
        if (IsJumping && jumpEndTime >= Time.time)
        {
            if (IsGrounded) { rb.linearVelocityY = jumpForce; }
        }
        else
        {
            IsJumping = false;
            rb.linearVelocityY = rb.linearVelocity.y;
        }

        // Falling & Grounded
        IsFalling = rb.linearVelocity.y < 0 && !IsGrounded;
        IsGrounded = hitBottom.collider != null;


        // Wall Jumping
        if (hitLeft.collider != null || hitRight.collider != null) 
        {
            if (!IsOnWall)
            {
                wallJumpBoostEndTime = Time.time + wallJumpBoostInterval;
            }
            IsOnWall = true; 
        }
        else { IsOnWall = false; }
        Debug.Log("Within Interval: " + (wallJumpBoostEndTime >= Time.time));
        Debug.Log(wallJumpBoostEndTime + " : " + Time.time);
        if (hitLeft.collider != null && IsJumping)
        {
            if (moveDirection.x <= 0f) { return; }
            rb.linearVelocity = (wallJumpBoostEndTime >= Time.time) ? new Vector2(walkSpeed, jumpForce) * wallJumpBoostMultiplier : new Vector2(walkSpeed, jumpForce);
        }
        if (hitRight.collider != null && IsJumping)
        {
            if (moveDirection.x >= 0f) { return; }
            rb.linearVelocity = (wallJumpBoostEndTime >= Time.time) ? new Vector2(-walkSpeed, jumpForce) * wallJumpBoostMultiplier : new Vector2(-walkSpeed, jumpForce);
        }

        // Head Bumping
        if (hitTop.collider != null & IsJumping)
        {
            if(rb.linearVelocityX == 0f) { return; }
            rb.linearVelocityX *= headHitMultiplyer;
        }
    }

    void FixedUpdate()
    {
        RayCast();
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        
        // Bottom
        originBottom = new Vector2(transform.position.x, transform.position.y - sprite.bounds.extents.y);
        Gizmos.DrawLine(originBottom, originBottom + Vector2.down * raycastDistance);

        // Top
        originTop = new Vector2(transform.position.x, transform.position.y + sprite.bounds.extents.y);
        Gizmos.DrawLine(originTop, originTop + Vector2.up * raycastDistance);
        // Left
        originLeft = new Vector2(transform.position.x - sprite.bounds.extents.x, transform.position.y);
        Gizmos.DrawLine(originLeft, originLeft + Vector2.left * raycastDistance);
        
        // Right
        originRight = new Vector2(transform.position.x + sprite.bounds.extents.x, transform.position.y);
        Gizmos.DrawLine(originRight, originRight + Vector2.right * raycastDistance);
    }
    private void RayCast()
    {
        // Bottom
        originBottom = new Vector2(transform.position.x, transform.position.y - sprite.bounds.extents.y);
        hitBottom = Physics2D.Raycast(originBottom, Vector2.down, raycastDistance, layerMask);

        // Top
        originTop = new Vector2(transform.position.x, transform.position.y + sprite.bounds.extents.y);
        hitTop = Physics2D.Raycast(originTop, Vector2.up, raycastDistance, layerMask);
        // Left
        originLeft = new Vector2(transform.position.x - sprite.bounds.extents.x, transform.position.y);
        hitLeft = Physics2D.Raycast(originLeft, Vector2.left, raycastDistance, layerMask);

        // Right
        originRight = new Vector2(transform.position.x + sprite.bounds.extents.x, transform.position.y);
        hitRight = Physics2D.Raycast(originRight, Vector2.right, raycastDistance, layerMask);
    }

    public void OnMove(InputAction.CallbackContext context)
    {
        Vector2 input = context.ReadValue<Vector2>();

        if (context.performed)
        {
            IsWalking = true;
            Debug.Log("Started moving");
            moveDirection = input.normalized * walkSpeed;
            sprite.flipX = moveDirection.x < 0;
        }
        else if (context.canceled)
        {
            IsWalking = false;
            Debug.Log("Stopped moving");
            if(IsGrounded) { rb.linearVelocityX *= stopVelocityMultiplier; }
        }
    }

    public void OnJump(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            IsJumping = true;
            Debug.Log("Started jumping");
            jumpEndTime = Time.time + jumpDuration;
        }
        else if (context.canceled)
        {
            IsJumping = false;
            Debug.Log("Stopped jumping");
        }
    }
}
