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
    private float jumpEndTime;
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


    private Vector2 origin;
    private RaycastHit2D hit;

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
        rb.linearVelocityX = (IsWalking) ? moveDirection.x : rb.linearVelocityX;

        // Jumping
        if (IsJumping && jumpEndTime >= Time.time)
        {
            rb.linearVelocityY = jumpForce;
        }
        else
        {
            IsJumping = false;
            rb.linearVelocityY = rb.linearVelocity.y;
        }

        // Falling & Grounded
        IsFalling = rb.linearVelocity.y < 0 && !IsGrounded;
        IsGrounded = hit.collider != null;
    }

    void FixedUpdate()
    {
        origin = new Vector2(transform.position.x, transform.position.y - collider.bounds.extents.y - .01f);
        hit = Physics2D.Raycast(origin, Vector2.down, .01f);
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
        if (context.performed && IsGrounded)
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
