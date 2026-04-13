using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Windows;

[RequireComponent(typeof(Rigidbody2D))]
public class PlayerController : MonoBehaviour
{
    [Header("Components")]
    private Rigidbody2D rb;

    [Header("Properties")]
    [SerializeField] private float moveSpeed = 5f;
    [SerializeField] private float stopVelocityMultiplier = 0.5f;
    [SerializeField] private float jumpForce = 5f;
    [SerializeField] private float jumpDuration = 1f;
    private float jumpEndTime;
    private Vector2 moveDirection;

    [Header("States")]
    [SerializeField] private bool isGrounded = true;
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
        }
    }
    [SerializeField] private bool isMoving = false;
    private bool IsMoving
    {
        get { return isMoving; }
        set
        {
            isMoving = value;
        }
    }

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    void Start()
    {

    }

    void Update()
    {
        rb.linearVelocityX = (IsMoving) ? moveDirection.x : rb.linearVelocityX;
        rb.linearVelocityY = (IsJumping && jumpEndTime >= Time.time) ? jumpForce : rb.linearVelocity.y;
    }

    public void OnMove(InputAction.CallbackContext context)
    {
        Vector2 input = context.ReadValue<Vector2>();

        if (context.performed)
        {
            IsMoving = true;
            Debug.Log("Started moving");
            moveDirection = input.normalized * moveSpeed;
        }
        else if (context.canceled)
        {
            IsMoving = false;
            Debug.Log("Stopped moving");
            if(isGrounded) { rb.linearVelocityX *= stopVelocityMultiplier; }
        }
    }

    public void OnJump(InputAction.CallbackContext context)
    {
        if (context.performed && isGrounded)
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
