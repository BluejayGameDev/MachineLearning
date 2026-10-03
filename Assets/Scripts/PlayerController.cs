using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Transform StartPosition;
    [SerializeField] private RaceManager raceManager;
    [SerializeField] private LevelManager levelManager;

    [Header("Movement")]
    [SerializeField] private float MoveSpeed = 1.5f;

    [Header("Jump")]
    [SerializeField] private float jumpPower = 7f;
    [SerializeField] private float normalGravity = 2f;
    [SerializeField] private float fallGravity = 3.5f;

    [Header("Ground Check")]
    [SerializeField] private Transform groundCheck;
    [SerializeField] private float groundCheckRadius = 0.15f;
    [SerializeField] private LayerMask groundLayer;

    private Rigidbody2D rb;
    private SpriteRenderer spriteRenderer;

    private bool isGrounded;

    private float coyoteCounter;
    private float coyoteTime = 0.1f;

    private float jumpBufferCounter;
    private float jumpBufferTime = 0.1f;

    private int facingDirection = 1;

    private Vector3 originalScale;

    // PLAYER DEMONSTRATION DATA
    // -1 left, 0 stopped, 1 right
    private int currentMovementDirection;
    private float jumpDemonstrationTimer;

    [SerializeField]
    private float jumpDemonstrationDuration = 0.15f;

    private void Awake()
    {
        rb =
            GetComponent<Rigidbody2D>();

        spriteRenderer =
            GetComponent<SpriteRenderer>();

        originalScale =
            transform.localScale;

        rb.gravityScale =
            normalGravity;
    }

    private void Start()
    {
        ResetForRace();
    }

    private void Update()
    {
        HandleMovementInput();
        HandleJumpInput();

        if (jumpDemonstrationTimer > 0f)
        {
            jumpDemonstrationTimer -=
                Time.deltaTime;
        }
    }

    private void FixedUpdate()
    {
        CheckGrounded();
        UpdateJumpTimers();
        ApplyJumpPhysics();
    }

    private void HandleMovementInput()
    {
        float movement = 0f;

        if (Input.GetKey(KeyCode.A))
        {
            movement = -1f;
        }
        else if (Input.GetKey(KeyCode.D))
        {
            movement = 1f;
        }

        if (movement < 0f)
        {
            currentMovementDirection = -1;

            facingDirection = -1;
            UpdateFacingDirection();

            rb.linearVelocity =
                new Vector2(
                    -MoveSpeed,
                    rb.linearVelocity.y
                );
        }
        else if (movement > 0f)
        {
            currentMovementDirection = 1;

            facingDirection = 1;
            UpdateFacingDirection();

            rb.linearVelocity =
                new Vector2(
                    MoveSpeed,
                    rb.linearVelocity.y
                );
        }
        else
        {
            currentMovementDirection = 0;

            rb.linearVelocity =
                new Vector2(
                    0f,
                    rb.linearVelocity.y
                );
        }
    }

    private void HandleJumpInput()
    {
        if (Input.GetKeyDown(KeyCode.W))
        {
            jumpBufferCounter =
                jumpBufferTime;

            jumpDemonstrationTimer =
                jumpDemonstrationDuration;
        }
    }

    private void CheckGrounded()
    {
        if (groundCheck == null)
        {
            isGrounded = false;
            return;
        }

        isGrounded =
            Physics2D.OverlapCircle(
                groundCheck.position,
                groundCheckRadius,
                groundLayer
            );
    }

    private void UpdateJumpTimers()
    {
        if (isGrounded)
        {
            coyoteCounter =
                coyoteTime;
        }
        else
        {
            coyoteCounter -=
                Time.fixedDeltaTime;
        }

        if (jumpBufferCounter > 0f)
        {
            jumpBufferCounter -=
                Time.fixedDeltaTime;
        }

        HandleJump();
    }

    private void HandleJump()
    {
        if (jumpBufferCounter > 0f &&
            coyoteCounter > 0f)
        {
            rb.linearVelocity =
                new Vector2(
                    rb.linearVelocity.x,
                    jumpPower
                );

            jumpBufferCounter = 0f;
            coyoteCounter = 0f;
        }
    }

    private void ApplyJumpPhysics()
    {
        if (rb.linearVelocity.y < 0f)
        {
            rb.gravityScale =
                fallGravity;
        }
        else
        {
            rb.gravityScale =
                normalGravity;
        }
    }

    private void UpdateFacingDirection()
    {
        Vector3 newScale =
            originalScale;

        if (facingDirection == -1)
        {
            newScale.x =
                -Mathf.Abs(
                    originalScale.x
                );
        }
        else
        {
            newScale.x =
                Mathf.Abs(
                    originalScale.x
                );
        }

        transform.localScale =
            newScale;
    }

    public void ResetForRace()
    {
        if (levelManager == null)
        {
            Debug.LogError(
                "PlayerController: LevelManager is not assigned!"
            );

            return;
        }

        StartPosition =
            levelManager.GetCurrentStartPosition();

        if (StartPosition != null)
        {
            transform.position =
                StartPosition.position;

            rb.position =
                StartPosition.position;
        }
        else
        {
            Debug.LogError(
                "PlayerController: Could not get Startpos from LevelManager!"
            );
        }

        rb.linearVelocity =
            Vector2.zero;

        rb.angularVelocity =
            0f;

        rb.gravityScale =
            normalGravity;

        coyoteCounter =
            0f;

        jumpBufferCounter =
            0f;

        currentMovementDirection =
            0;

        jumpDemonstrationTimer =
            0f;

        facingDirection =
            1;

        UpdateFacingDirection();
    }

    private void OnTriggerEnter2D(
        Collider2D collision
    )
    {
        if (collision.CompareTag("Deadly"))
        {
            PlayerDied();
        }
    }

    private void OnCollisionEnter2D(
        Collision2D collision
    )
    {
        if (collision.gameObject.CompareTag(
            "Deadly"
        ))
        {
            PlayerDied();
        }
    }

    private void PlayerDied()
    {
        if (raceManager != null)
        {
            raceManager.PlayerDied();
        }
        else
        {
            ResetForRace();
        }
    }

    public int GetMovementDirection()
    {
        return currentMovementDirection;
    }

    public bool IsDemonstratingJump()
    {
        return jumpDemonstrationTimer > 0f;
    }

    public bool IsGrounded()
    {
        return isGrounded;
    }

    public float GetVelocityX()
    {
        return rb.linearVelocity.x;
    }

    public float GetVelocityY()
    {
        return rb.linearVelocity.y;
    }
}