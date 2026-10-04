using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Actuators;
using Unity.MLAgents.Sensors;

public class PlatformerAgent : Agent
{
    [Header("References")]
    [SerializeField] private Transform goal;
    [SerializeField] private Transform startPosition;
    [SerializeField] private LevelManager levelManager;
    [SerializeField] private RaceManager raceManager;
    [SerializeField] private AgentEyes agentEyes;

    [Header("Movement")]
    [SerializeField] private float moveSpeed = 1.5f;

    [Header("Jump")]
    [SerializeField] private float jumpPower = 7f;
    [SerializeField] private float gravity = 2f;

    [Header("Ground Check")]
    [SerializeField] private Transform groundCheck;
    [SerializeField] private float groundCheckRadius = 0.15f;
    [SerializeField] private LayerMask groundLayer;

    [Header("Rewards")]
    [SerializeField] private float progressReward = 0.1f;
    [SerializeField] private float stepPenalty = -0.001f;
    [SerializeField] private float goalReward = 1f;
    [SerializeField] private float losePenalty = -1f;
    [SerializeField] private float timeoutPenalty = -1f;

    [Header("Death")]
    [SerializeField] private float deathPenalty = -0.1f;

    [Header("Stats")]
    [HideInInspector] public int currentEpisode;
    [HideInInspector] public float cumulativeReward;

    private Rigidbody2D rb;
    private bool isGrounded;
    private float previousDistanceToGoal;
    private int facingDirection = 1;
    private Vector3 originalScale;

    public override void Initialize()
    {
        rb =
            GetComponent<Rigidbody2D>();

        if (rb == null)
        {
            Debug.LogError(
                "PlatformerAgent needs a Rigidbody2D!"
            );

            return;
        }

        originalScale =
            transform.localScale;

        rb.gravityScale =
            gravity;

        currentEpisode =
            0;

        cumulativeReward =
            0f;
    }

    public override void OnEpisodeBegin()
    {
        if (raceManager != null)
        {
            raceManager.PrepareForNewRoom();
        }
        else
        {
            ResetAgentPosition();
        }

        goal = null;
        startPosition = null;

        if (levelManager != null)
        {
            levelManager.ResetLevel(
                FinishEpisodeReset
            );

            return;
        }

        FinishEpisodeReset();
    }

    private void FinishEpisodeReset()
    {
        if (levelManager != null)
        {
            startPosition =
                levelManager.GetCurrentStartPosition();

            goal =
                levelManager.GetCurrentGoal();
        }

        if (startPosition == null)
        {
            Debug.LogError(
                "PlatformerAgent: Could not get Startpos from LevelManager!"
            );
        }

        if (goal == null)
        {
            Debug.LogError(
                "PlatformerAgent: Could not get Goal from LevelManager!"
            );
        }

        if (currentEpisode > 0)
        {
            Debug.Log(
                "================================"
            );

            Debug.Log(
                "EPISODE #" +
                currentEpisode +
                " ENDED"
            );

            Debug.Log(
                "FINAL AI REWARD: " +
                cumulativeReward.ToString("F4")
            );

            Debug.Log(
                "AI STEP COUNT: " +
                StepCount
            );

            Debug.Log(
                "================================"
            );
        }

        currentEpisode++;
        cumulativeReward = 0f;

        ResetAgent();

        if (raceManager != null)
        {
            raceManager.ResetForNewEpisode();
        }

        if (goal != null)
        {
            previousDistanceToGoal =
                Vector2.Distance(
                    transform.position,
                    goal.position
                );
        }

        Debug.Log(
            "NEW EPISODE #" +
            currentEpisode +
            " - AI AND PLAYER RESET"
        );
    }

    public override void CollectObservations(
        VectorSensor sensor
    )
    {
        // ============================================================
        // GOAL OBSERVATIONS
        // ============================================================

        if (goal != null)
        {
            Vector2 directionToGoal =
                goal.position -
                transform.position;

            sensor.AddObservation(
                Mathf.Clamp(
                    directionToGoal.x / 20f,
                    -1f,
                    1f
                )
            );

            sensor.AddObservation(
                Mathf.Clamp(
                    directionToGoal.y / 10f,
                    -1f,
                    1f
                )
            );

            sensor.AddObservation(
                Mathf.Clamp01(
                    directionToGoal.magnitude / 25f
                )
            );
        }
        else
        {
            sensor.AddObservation(0f);
            sensor.AddObservation(0f);
            sensor.AddObservation(0f);
        }

        // ============================================================
        // VELOCITY OBSERVATIONS
        // ============================================================

        if (rb == null)
        {
            rb =
                GetComponent<Rigidbody2D>();
        }

        float velocityX = 0f;
        float velocityY = 0f;

        if (rb != null)
        {
            velocityX =
                rb.linearVelocity.x;

            velocityY =
                rb.linearVelocity.y;
        }

        sensor.AddObservation(
            Mathf.Clamp(
                velocityX / moveSpeed,
                -1f,
                1f
            )
        );

        sensor.AddObservation(
            Mathf.Clamp(
                velocityY / jumpPower,
                -1f,
                1f
            )
        );

        // ============================================================
        // GROUNDED OBSERVATION
        // ============================================================

        sensor.AddObservation(
            isGrounded ? 1f : 0f
        );

        // ============================================================
        // ENVIRONMENT / EYE OBSERVATIONS
        // ============================================================

        if (agentEyes != null)
        {
            float[] eyeDistances =
                agentEyes.GetEyeDistances();

            float[] eyeTypes =
                agentEyes.GetEyeTypes();

            for (int i = 0;
                 i < eyeDistances.Length;
                 i++)
            {
                // Distance:
                // 0 = object is very close
                // 1 = nothing detected within eye range

                sensor.AddObservation(
                    Mathf.Clamp01(
                        eyeDistances[i] /
                        agentEyes.EyeLength
                    )
                );

                // Type:
                // 0 = nothing
                // 0.5 = platform
                // 1 = deadly

                sensor.AddObservation(
                    eyeTypes[i] / 2f
                );
            }
        }
        else
        {
            // 8 eyes × 2 observations per eye.
            for (int i = 0; i < 8; i++)
            {
                sensor.AddObservation(1f);
                sensor.AddObservation(0f);
            }
        }
    }

    public override void OnActionReceived(
        ActionBuffers actions
    )
    {
        if (rb == null)
        {
            return;
        }

        int moveAction =
            actions.DiscreteActions[0];

        if (moveAction == 1)
        {
            rb.linearVelocity =
                new Vector2(
                    -moveSpeed,
                    rb.linearVelocity.y
                );

            facingDirection = -1;
            UpdateFacingDirection();
        }
        else if (moveAction == 2)
        {
            rb.linearVelocity =
                new Vector2(
                    moveSpeed,
                    rb.linearVelocity.y
                );

            facingDirection = 1;
            UpdateFacingDirection();
        }
        else
        {
            rb.linearVelocity =
                new Vector2(
                    0f,
                    rb.linearVelocity.y
                );
        }

        int jumpAction =
            actions.DiscreteActions[1];

        if (jumpAction == 1 &&
            isGrounded)
        {
            rb.linearVelocity =
                new Vector2(
                    rb.linearVelocity.x,
                    jumpPower
                );
        }

        GiveReward(stepPenalty);

        if (goal != null)
        {
            float currentDistanceToGoal =
                Vector2.Distance(
                    transform.position,
                    goal.position
                );

            float distanceImprovement =
                previousDistanceToGoal -
                currentDistanceToGoal;

            if (distanceImprovement > 0f)
            {
                GiveReward(
                    distanceImprovement *
                    progressReward
                );
            }

            previousDistanceToGoal =
                currentDistanceToGoal;
        }
    }

    private void GiveReward(float reward)
    {
        AddReward(reward);
        cumulativeReward += reward;
    }

    private void FixedUpdate()
    {
        CheckGrounded();
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

    public void ResetAgentPosition()
    {
        if (rb == null)
        {
            rb =
                GetComponent<Rigidbody2D>();
        }

        if (rb == null)
        {
            return;
        }

        if (startPosition != null)
        {
            transform.position =
                startPosition.position;

            rb.position =
                startPosition.position;
        }

        rb.linearVelocity =
            Vector2.zero;

        rb.angularVelocity =
            0f;

        rb.gravityScale =
            gravity;

        transform.rotation =
            Quaternion.identity;

        isGrounded = false;

        facingDirection = 1;
        UpdateFacingDirection();
    }

    private void OnTriggerEnter2D(
        Collider2D collision
    )
    {
        if (collision.CompareTag("Deadly"))
        {
            Die();
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
            Die();
        }
    }

    private void Die()
    {
        GiveReward(deathPenalty);

        if (raceManager != null)
        {
            raceManager.AIDied();
        }
        else
        {
            ResetAgentPosition();
        }
    }

    public void ReachedGoal()
    {
        GiveReward(goalReward);

        if (levelManager != null)
        {
            levelManager.FlashResult(true);
        }

        Debug.Log("================================");
        Debug.Log("RESULT: AI WON");
        Debug.Log(
            "FINAL AI REWARD: " +
            cumulativeReward.ToString("F4")
        );
        Debug.Log(
            "AI STEP COUNT: " +
            StepCount
        );
        Debug.Log("================================");

        EndEpisode();
    }

    public void PlayerWon()
    {
        GiveReward(losePenalty);

        if (levelManager != null)
        {
            levelManager.FlashResult(false);
        }

        Debug.Log("================================");
        Debug.Log("RESULT: PLAYER WON");
        Debug.Log(
            "FINAL AI REWARD: " +
            cumulativeReward.ToString("F4")
        );
        Debug.Log(
            "AI STEP COUNT: " +
            StepCount
        );
        Debug.Log("================================");

        EndEpisode();
    }

    public void Timeout()
    {
        GiveReward(timeoutPenalty);

        if (levelManager != null)
        {
            levelManager.FlashResult(false);
        }

        Debug.Log("================================");
        Debug.Log("RESULT: TIMEOUT");
        Debug.Log(
            "FINAL AI REWARD: " +
            cumulativeReward.ToString("F4")
        );
        Debug.Log(
            "AI STEP COUNT: " +
            StepCount
        );
        Debug.Log("================================");

        EndEpisode();
    }

    private void ResetAgent()
    {
        if (rb == null)
        {
            rb =
                GetComponent<Rigidbody2D>();
        }

        if (rb == null)
        {
            return;
        }

        if (startPosition != null)
        {
            transform.position =
                startPosition.position;

            rb.position =
                startPosition.position;
        }

        rb.linearVelocity =
            Vector2.zero;

        rb.angularVelocity =
            0f;

        rb.gravityScale =
            gravity;

        transform.rotation =
            Quaternion.identity;

        isGrounded = false;

        facingDirection = 1;
        UpdateFacingDirection();
    }

    public override void Heuristic(
        in ActionBuffers actionsOut
    )
    {
        var actions =
            actionsOut.DiscreteActions;

        actions[0] = 0;

        if (Input.GetKey(KeyCode.A))
        {
            actions[0] = 1;
        }
        else if (Input.GetKey(KeyCode.D))
        {
            actions[0] = 2;
        }

        actions[1] = 0;

        if (Input.GetKey(KeyCode.W))
        {
            actions[1] = 1;
        }
    }

    private void OnDrawGizmos()
    {
        if (goal == null)
        {
            return;
        }

        Gizmos.color =
            Color.green;

        Gizmos.DrawLine(
            transform.position,
            goal.position
        );

        Gizmos.DrawWireSphere(
            goal.position,
            0.3f
        );
    }
}