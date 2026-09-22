using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Actuators;
using Unity.MLAgents.Sensors;

public class PlatformerAgent : Agent
{
    [SerializeField] private Transform Goal;
    [SerializeField] private float MoveSpeed = 1.5f;
    [SerializeField] private Transform StartPosition;

    private SpriteRenderer spriteRenderer;

    private int currentEpisode = 0;
    private float cumulativeReward = 0f;

    private Vector3 originalScale;
    private int facingDirection = 1;

    public override void Initialize()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        originalScale = transform.localScale;

        currentEpisode = 0;
        cumulativeReward = 0f;
    }

    public override void OnEpisodeBegin()
    {
        currentEpisode++;
        cumulativeReward = 0f;
        spriteRenderer.color = Color.white;

        SpawnObjects();
    }

    private void SpawnObjects()
    {
        // Reset agent position
        if (StartPosition != null)
        {
            transform.localPosition = StartPosition.localPosition;
        }
        else
        {
            transform.localPosition = new Vector3(0f, 0.3f, 0f);
        }

        // Reset rotation
        transform.localRotation = Quaternion.identity;

        // Randomly choose whether the agent starts facing left or right
        facingDirection = Random.Range(0, 2) == 0 ? -1 : 1;

        UpdateFacingDirection();

        float randomX = Random.Range(-8f, 8f);
        float randomY = Random.Range(-10f, 10f);

        Vector3 goalPosition = transform.localPosition + new Vector3(randomX, -1f, 0f);

        Goal.localPosition = goalPosition;
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
        float goalPosX_normalized = Goal.localPosition.x / 5f;
        float goalPosY_normalized = Goal.localPosition.y / 5f;

        float PlatformerAgentPosX_normalized = transform.localPosition.x / 5f;
        float PlatformerAgentPosY_normalized = transform.localPosition.y / 5f;

        float PlatformerFacing = facingDirection;

        sensor.AddObservation(goalPosX_normalized);
        sensor.AddObservation(goalPosY_normalized);
        sensor.AddObservation(PlatformerAgentPosX_normalized);
        sensor.AddObservation(PlatformerAgentPosY_normalized);
        sensor.AddObservation(PlatformerFacing);
    }

    public override void OnActionReceived(ActionBuffers actions)
    {
        MoveAgent(actions.DiscreteActions);

        AddReward(-2f / MaxStep);

        cumulativeReward = GetCumulativeReward();
    }

    public void MoveAgent(ActionSegment<int> act)
    {
        var action = act[0];

        switch (action)
        {
            case 1:
                facingDirection = -1;
                UpdateFacingDirection();
                transform.position += Vector3.left * MoveSpeed * Time.deltaTime;
                break;

            case 2:
                facingDirection = 1;
                UpdateFacingDirection();
                transform.position += Vector3.right * MoveSpeed * Time.deltaTime;
                break;
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
