
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
    [SerializeField] private float jumpPower = 7f;
    [SerializeField] private float gravity = 2f;

    [Header("Ground Check")]
    [SerializeField] private Transform groundCheck;
    [SerializeField] private float groundCheckRadius = 0.15f;
    [SerializeField] private LayerMask groundLayer;

    [Header("Rewards")]
    [SerializeField] private float progressReward = 0.1f;
    [SerializeField] private float stepPenalty = -0.001f;
    [SerializeField] private float goalReward = 2f;
    [SerializeField] private float losePenalty = -1f;
    [SerializeField] private float timeoutPenalty = -1f;
    [SerializeField] private float stuckPenalty = -0.02f;
    [SerializeField] private float stuckTimeThreshold = 0.5f;
    [SerializeField] private float stuckMovementThreshold = 0.01f;
    [SerializeField] private float deathPenalty = -0.5f;

    [Header("Stats")]
    [HideInInspector] public int currentEpisode;
    [HideInInspector] public float cumulativeReward;

    private Rigidbody2D rb;
    private bool isGrounded;
    private float previousDistanceToGoal;
    private float stuckTimer;
    private int facingDirection = 1;
    private int lastMoveAction;
    private Vector3 originalScale;
    private Vector2 lastStuckCheckPosition;

    public override void Initialize()
    {
        rb = GetComponent<Rigidbody2D>();
        if (rb == null)
        {
            Debug.LogError("PlatformerAgent needs a Rigidbody2D!");
            return;
        }

        originalScale = transform.localScale;
        rb.gravityScale = gravity;
        currentEpisode = 0;
        cumulativeReward = 0f;
    }

    public override void OnEpisodeBegin()
    {
        if (raceManager != null) raceManager.PrepareForNewRoom();
        else ResetAgentPosition();

        goal = null;
        startPosition = null;

        if (levelManager != null) levelManager.ResetLevel(FinishEpisodeReset);
        else FinishEpisodeReset();
    }

    private void FinishEpisodeReset()
    {
        if (levelManager != null)
        {
            startPosition = levelManager.GetCurrentStartPosition();
            goal = levelManager.GetCurrentGoal();
        }

        if (startPosition == null) Debug.LogError("Could not get StartPosition from LevelManager!");
        if (goal == null) Debug.LogError("Could not get Goal from LevelManager!");

        if (currentEpisode > 0)
        {
            Debug.Log($"EPISODE #{currentEpisode} ENDED | Reward: {cumulativeReward:F4} | Steps: {StepCount}");
        }

        currentEpisode++;
        cumulativeReward = 0f;

        ResetAgentPosition();

        if (raceManager != null) raceManager.ResetForNewEpisode();

        previousDistanceToGoal = goal != null ? Vector2.Distance(transform.position, goal.position) : 0f;
        lastStuckCheckPosition = transform.position;
        stuckTimer = 0f;
        lastMoveAction = 0;

        Debug.Log($"NEW EPISODE #{currentEpisode} - AI AND PLAYER RESET");
    }

    public override void CollectObservations(VectorSensor sensor)
    {
        if (goal != null)
        {
            Vector2 direction = goal.position - transform.position;
            sensor.AddObservation(Mathf.Clamp(direction.x / 20f, -1f, 1f));
            sensor.AddObservation(Mathf.Clamp(direction.y / 10f, -1f, 1f));
            sensor.AddObservation(Mathf.Clamp01(direction.magnitude / 25f));
        }
        else
        {
            sensor.AddObservation(0f);
            sensor.AddObservation(0f);
            sensor.AddObservation(0f);
        }

        if (rb == null) rb = GetComponent<Rigidbody2D>();

        Vector2 velocity = rb != null ? rb.linearVelocity : Vector2.zero;
        sensor.AddObservation(Mathf.Clamp(velocity.x / moveSpeed, -1f, 1f));
        sensor.AddObservation(Mathf.Clamp(velocity.y / jumpPower, -1f, 1f));
        sensor.AddObservation(isGrounded ? 1f : 0f);

        if (agentEyes != null)
        {
            float[] distances = agentEyes.GetEyeDistances();
            float[] types = agentEyes.GetEyeTypes();

            for (int i = 0; i < distances.Length; i++)
            {
                sensor.AddObservation(Mathf.Clamp01(distances[i] / agentEyes.EyeLength));
                sensor.AddObservation(types[i] / 2f);
            }

            float[] groundDistances = agentEyes.GetGroundDistances();
            sensor.AddObservation(groundDistances[0]);
            sensor.AddObservation(groundDistances[1]);
        }
        else
        {
            for (int i = 0; i < 8; i++)
            {
                sensor.AddObservation(1f);
                sensor.AddObservation(0f);
            }

            sensor.AddObservation(1f);
            sensor.AddObservation(1f);
        }
    }

    public override void OnActionReceived(ActionBuffers actions)
    {
        if (rb == null) return;

        int moveAction = actions.DiscreteActions[0];
        lastMoveAction = moveAction;

        float horizontalMovement = moveAction == 1 ? -moveSpeed : moveAction == 2 ? moveSpeed : 0f;
        rb.linearVelocity = new Vector2(horizontalMovement, rb.linearVelocity.y);

        if (moveAction != 0)
        {
            facingDirection = moveAction == 1 ? -1 : 1;
            UpdateFacingDirection();
        }

        if (actions.DiscreteActions[1] == 1 && isGrounded)
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpPower);

        GiveReward(stepPenalty);

        if (goal != null)
        {
            float distance = Vector2.Distance(transform.position, goal.position);
            float improvement = previousDistanceToGoal - distance;

            if (improvement > 0f) GiveReward(improvement * progressReward);
            previousDistanceToGoal = distance;
        }
    }

    private void FixedUpdate()
    {
        CheckGrounded();
        CheckIfStuck();
    }

    private void CheckGrounded()
    {
        isGrounded = groundCheck != null &&
            Physics2D.OverlapCircle(groundCheck.position, groundCheckRadius, groundLayer);
    }

    private void CheckIfStuck()
    {
        Vector2 position = transform.position;
        float movement = Mathf.Abs(position.x - lastStuckCheckPosition.x);

        if (isGrounded && lastMoveAction != 0 && movement < stuckMovementThreshold)
        {
            stuckTimer += Time.fixedDeltaTime;

            if (stuckTimer >= stuckTimeThreshold)
            {
                GiveReward(stuckPenalty);
                stuckTimer = 0f;
            }
        }
        else stuckTimer = 0f;

        lastStuckCheckPosition = position;
    }

    private void GiveReward(float reward)
    {
        AddReward(reward);
        cumulativeReward += reward;
    }

    private void UpdateFacingDirection()
    {
        Vector3 scale = originalScale;
        scale.x = Mathf.Abs(originalScale.x) * facingDirection;
        transform.localScale = scale;
    }

    public void ResetAgentPosition()
    {
        if (rb == null) rb = GetComponent<Rigidbody2D>();
        if (rb == null) return;

        if (startPosition != null)
        {
            transform.position = startPosition.position;
            rb.position = startPosition.position;
        }

        rb.linearVelocity = Vector2.zero;
        rb.angularVelocity = 0f;
        rb.gravityScale = gravity;
        transform.rotation = Quaternion.identity;

        isGrounded = false;
        stuckTimer = 0f;
        lastMoveAction = 0;
        lastStuckCheckPosition = transform.position;

        facingDirection = 1;
        UpdateFacingDirection();
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Deadly")) Die();
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Deadly")) Die();
    }

    private void Die()
    {
        GiveReward(deathPenalty);

        if (raceManager != null) raceManager.AIDied();
        else ResetAgentPosition();
    }

    public void ReachedGoal() => FinishResult(goalReward, true, "AI WON");
    public void PlayerWon() => FinishResult(losePenalty, false, "PLAYER WON");
    public void Timeout() => FinishResult(timeoutPenalty, false, "TIMEOUT");

    private void FinishResult(float reward, bool won, string result)
    {
        GiveReward(reward);

        if (levelManager != null) levelManager.FlashResult(won);

        Debug.Log($"RESULT: {result} | Reward: {cumulativeReward:F4} | Steps: {StepCount}");

        EndEpisode();
    }

    public override void Heuristic(in ActionBuffers actionsOut)
    {
        var actions = actionsOut.DiscreteActions;

        actions[0] = Input.GetKey(KeyCode.A) ? 1 : Input.GetKey(KeyCode.D) ? 2 : 0;
        actions[1] = Input.GetKey(KeyCode.W) ? 1 : 0;
    }

    private void OnDrawGizmos()
    {
        if (goal == null) return;

        Gizmos.color = Color.green;
        Gizmos.DrawLine(transform.position, goal.position);
        Gizmos.DrawWireSphere(goal.position, 0.3f);
    }
}
