using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Actuators;
using Unity.MLAgents.Sensors;
using System.Collections;

public class PlatformerAgent : Agent
{
    [Header("References")]
    [SerializeField] private Transform Goal;
    [SerializeField] private SpriteRenderer RoomRenderer;
    [SerializeField] private Transform StartPosition;

    [Header("Movement")]
    [SerializeField] private float MoveSpeed = 1.5f;

    [Header("Jump")] 
    [SerializeField] private float jumpPower = 7f; 
    [SerializeField] private float normalGravity = 2f; 
    [SerializeField] private float fallGravity = 3.5f; 
    [SerializeField] private float lowJumpGravity = 4f;

    [Header("Ground Check")] 
    [SerializeField] private Transform groundCheck; 
    [SerializeField] private float groundCheckRadius = 0.15f; 
    [SerializeField] private LayerMask groundLayer;

    [Header("Jump Settings")] 
    [SerializeField] private float coyoteTime = 0.1f; 
    [SerializeField] private float jumpBufferTime = 0.1f;

    [Header("Goal Spawn")]
    [SerializeField] private LayerMask[] goalBlockedLayers;
    [SerializeField] private int maxGoalSpawnAttempts = 50;

    private Rigidbody2D rb;
    private SpriteRenderer spriteRenderer;

    [HideInInspector] public int currentEpisode = 0;
    [HideInInspector] public float cumulativeReward = 0f;

    private Vector3 originalScale;
    private int facingDirection = 1;

    private Color defaultGroundColour;
    private Coroutine flashGroundCoroutine;

    // Jump variables 
    private float coyoteCounter; 
    private float jumpBufferCounter; 
    private bool jumpHeld; 
    private bool isGrounded;

    public override void Initialize()
    {
        rb = GetComponent<Rigidbody2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        originalScale = transform.localScale;

        currentEpisode = 0;
        cumulativeReward = 0f;

        rb.gravityScale = normalGravity;

        if (RoomRenderer != null)
        {
            defaultGroundColour = RoomRenderer.color;
        }
    }

    private void FixedUpdate()
    {
        CheckGrounded();
        UpdateJumpTimers();
        ApplyJumpPhysics();
    }

    public override void OnEpisodeBegin()
    {
        if (RoomRenderer != null && cumulativeReward != 0)
        {
            Color flashColor = (cumulativeReward > 0f) ? Color.green : Color.red;
            flashColor.a = defaultGroundColour.a;

            if (flashGroundCoroutine != null)
            {
                StopCoroutine(flashGroundCoroutine);
            }

            flashGroundCoroutine = StartCoroutine(FlashRoom(flashColor, 3.0f));
        }

        currentEpisode++;
        cumulativeReward = 0f;
        spriteRenderer.color = Color.white;

        SpawnObjects();
    }

    private IEnumerator FlashRoom(Color targetColour, float duration)
    {
        float elapsedTime = 0f;

        RoomRenderer.color = targetColour;

        while (elapsedTime < duration)
        {
            elapsedTime += Time.deltaTime;
            RoomRenderer.color = Color.Lerp(targetColour, defaultGroundColour, elapsedTime / duration);

            yield return null;
        }

        RoomRenderer.color = defaultGroundColour;
    }

    private void SpawnGoal()
    {
        Collider2D goalCollider = Goal.GetComponent<Collider2D>();

        if (goalCollider == null)
        {
            Debug.LogWarning("Goal does not have a Collider2D!");
            return;
        }

        for (int attempt = 0; attempt < maxGoalSpawnAttempts; attempt++)
        {
            float randomX = Random.Range(-8f, 8f);
            float randomY = Random.Range(1f, 6.5f);

            Vector3 goalPosition = new Vector3(randomX, randomY, Goal.localPosition.z);

            Goal.localPosition = goalPosition;

            Bounds bounds = goalCollider.bounds;

            bool positionBlocked = false;

            // Check against every selected layer
            foreach (LayerMask layerMask in goalBlockedLayers)
            {
                Collider2D[] overlappingObjects = Physics2D.OverlapBoxAll(
                bounds.center,
                bounds.size,
                Goal.eulerAngles.z,
                layerMask
                );

                foreach (Collider2D collider in overlappingObjects)
                {
                    // Ignore the Goal's own collider
                    if (collider != goalCollider)
                    {
                        positionBlocked = true;
                        break;
                    }
                }

                if (positionBlocked)
                {
                    break;
                }
            }

            // Valid position found
            if (!positionBlocked)
            {
                Debug.Log("Goal spawned successfully on attempt " + (attempt + 1));
                return;
            }
        }

        Debug.LogWarning(
            "Could not find a valid Goal spawn position after "
            + maxGoalSpawnAttempts + " attempts."
        );
    }

    private void SpawnObjects()
    {
        // Reset agent position
        if (StartPosition != null)
        {
            rb.position = StartPosition.position;
        }
        else
        {
            rb.position = new Vector2(0f, 0.3f);
        }

        rb.linearVelocity = Vector2.zero;
        rb.angularVelocity = 0f;
        rb.gravityScale = normalGravity;

        coyoteCounter = 0f;
        jumpBufferCounter = 0f;
        jumpHeld = false;

        // Reset rotation
        transform.localRotation = Quaternion.identity;

        // Randomly choose whether the agent starts facing left or right
        facingDirection = Random.Range(0, 2) == 0 ? -1 : 1;

        UpdateFacingDirection();

        SpawnGoal();
    }

    private void UpdateFacingDirection()
    {
        Vector3 newScale = originalScale;

        if (facingDirection == -1)
        {
            newScale.x = -Mathf.Abs(originalScale.x);
        }
        else
        {
            newScale.x = Mathf.Abs(originalScale.x);
        }

        transform.localScale = newScale;
    }

    public override void CollectObservations(VectorSensor sensor)
    {
        float goalPosX_normalized = Goal.localPosition.x / 10f;
        float goalPosY_normalized = Goal.localPosition.y / 10f;

        float PlatformerAgentPosX_normalized = transform.localPosition.x / 5f;
        float PlatformerAgentPosY_normalized = transform.localPosition.y / 5f;

        float PlatformerFacing = facingDirection;

        float grounded = isGrounded ? 1f : 0f;

        float velocityX = rb.linearVelocity.x / MoveSpeed;
        float velocityY = rb.linearVelocity.y / jumpPower;

        sensor.AddObservation(goalPosX_normalized);
        sensor.AddObservation(goalPosY_normalized);
        sensor.AddObservation(PlatformerAgentPosX_normalized);
        sensor.AddObservation(PlatformerAgentPosY_normalized);
        sensor.AddObservation(PlatformerFacing);
        sensor.AddObservation(grounded);

        sensor.AddObservation(velocityX);
        sensor.AddObservation(velocityY);
    }

    public override void Heuristic(in ActionBuffers actionsOut)
    {
        var discreteActionsOut = actionsOut.DiscreteActions;

        discreteActionsOut[0] = 0;

        if (Input.GetKey(KeyCode.A))
        {
            discreteActionsOut[0] = 1;
        }
        else if (Input.GetKey(KeyCode.D))
        {
            discreteActionsOut[0] = 2;
        }


        discreteActionsOut[1] = 0;

        if (Input.GetKey(KeyCode.W))
        {
            discreteActionsOut[1] = 1;
        }
    }

    public override void OnActionReceived(ActionBuffers actions)
    {
        ActionSegment<int> discreteActions = actions.DiscreteActions;

        int movementAction = discreteActions[0];

        int jumpAction = discreteActions[1];

        MoveAgent(movementAction);

        HandleJumpAction(jumpAction);

        AddReward(-2f / MaxStep);

        cumulativeReward = GetCumulativeReward();
    }

    public void MoveAgent(int action)
    {
        switch (action)
        {
            case 0:
                rb.linearVelocity = new Vector2(0f, rb.linearVelocity.y);
                break;

            case 1:
                facingDirection = -1;
                UpdateFacingDirection();
                rb.linearVelocity = new Vector2(-MoveSpeed, rb.linearVelocity.y);
                break;

            case 2:
                facingDirection = 1;
                UpdateFacingDirection();
                rb.linearVelocity = new Vector2(MoveSpeed, rb.linearVelocity.y);
                break;
        }
    }

    public void HandleJumpAction(int action)
    {
        if (action == 1)
        {
            if (!jumpHeld)
            {
                jumpBufferCounter = jumpBufferTime;
            }

            jumpHeld = true;
        }
        else
        {
            jumpHeld = false;
        }
    }

    private void CheckGrounded()
    {
        if (groundCheck == null)
        {
            isGrounded = false;
            return;
        }

        isGrounded = Physics2D.OverlapCircle(groundCheck.position, groundCheckRadius, groundLayer);
    }

    private void UpdateJumpTimers()
    {
        if (isGrounded)
        {
            coyoteCounter = coyoteTime;
        }
        else
        {
            coyoteCounter -= Time.fixedDeltaTime;
        }


        if (jumpBufferCounter > 0f)
        {
            jumpBufferCounter -= Time.fixedDeltaTime;
        }


        HandleJump();
    }

    private void HandleJump()
    {
        if (jumpBufferCounter > 0f && coyoteCounter > 0f)
        {
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpPower);

            jumpBufferCounter = 0f;
            coyoteCounter = 0f;
        }
    }

    private void ApplyJumpPhysics()
    {
        if (rb.linearVelocity.y < 0f)
        {
            rb.gravityScale = fallGravity;
        }
        else if (rb.linearVelocity.y > 0f && !jumpHeld)
        {
            rb.gravityScale = lowJumpGravity;

            coyoteCounter = 0f;
        }
        else
        {
            rb.gravityScale = normalGravity;
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.CompareTag("Goal"))
        {
            GoalReached();
        }
    }

    private void GoalReached()
    {
        AddReward(1.0f);
        cumulativeReward = GetCumulativeReward();

        EndEpisode();
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Wall"))
        {
            AddReward(-0.05f);

            if(spriteRenderer != null)
            {
                spriteRenderer.color = Color.red;
            }
        }
    }

    private void OnCollisionStay2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Wall"))
        {
            AddReward(-0.01f * Time.fixedDeltaTime);
        }
    }

    private void OnCollisionExit2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Wall"))
        {
            if (spriteRenderer != null)
            {
                spriteRenderer.color = Color.white;
            }
        }
    }
}
