
using UnityEngine;

public class AgentEyes : MonoBehaviour
{
    [Header("Eye Settings")]
    [SerializeField] private float eyeLength = 5f;

    public float EyeLength { get { return eyeLength; } }

    [Header("Ground Detection")]
    [SerializeField] private float groundRayLength = 2f;
    [SerializeField] private float groundRayForwardOffset = 0.5f;
    [SerializeField] private float groundRaySideOffset = 0.2f;

    [Header("Detection Layers")]
    [SerializeField] private LayerMask platformLayer;
    [SerializeField] private LayerMask deadlyLayer;

    [Header("Eye Directions")]
    [SerializeField] private Vector2[] eyeDirections =
    {
        // Left
        new Vector2(-1f, 0.5f),
        new Vector2(-1f, 0.15f),
        new Vector2(-1f, -0.35f),

        // Centre
        new Vector2(0f, 1f),
        new Vector2(0f, -1f),

        // Right
        new Vector2(1f, 0.5f),
        new Vector2(1f, 0.15f),
        new Vector2(1f, -0.35f)
    };

    public float[] GetEyeDistances()
    {
        float[] distances = new float[eyeDirections.Length];

        for (int i = 0; i < eyeDirections.Length; i++)
        {
            Vector2 direction = eyeDirections[i].normalized;
            RaycastHit2D platformHit = Physics2D.Raycast(transform.position, direction, eyeLength, platformLayer);
            RaycastHit2D deadlyHit = Physics2D.Raycast(transform.position, direction, eyeLength, deadlyLayer);
            float closestDistance = eyeLength;

            if (platformHit.collider != null) closestDistance = Mathf.Min(closestDistance, platformHit.distance);
            if (deadlyHit.collider != null) closestDistance = Mathf.Min(closestDistance, deadlyHit.distance);

            distances[i] = closestDistance;
        }

        return distances;
    }

    public float[] GetEyeTypes()
    {
        float[] types = new float[eyeDirections.Length];

        for (int i = 0; i < eyeDirections.Length; i++)
        {
            Vector2 direction = eyeDirections[i].normalized;
            RaycastHit2D platformHit = Physics2D.Raycast(transform.position, direction, eyeLength, platformLayer);
            RaycastHit2D deadlyHit = Physics2D.Raycast(transform.position, direction, eyeLength, deadlyLayer);

            if (platformHit.collider == null && deadlyHit.collider == null)
            {
                types[i] = 0f;
            }
            else if (deadlyHit.collider != null && (platformHit.collider == null || deadlyHit.distance < platformHit.distance))
            {
                types[i] = 2f;
            }
            else
            {
                types[i] = 1f;
            }
        }

        return types;
    }

    public float[] GetGroundDistances()
    {
        float[] distances = new float[2];
        Vector2 leftOrigin = (Vector2)transform.position + Vector2.left * groundRayForwardOffset + Vector2.down * groundRaySideOffset;
        Vector2 rightOrigin = (Vector2)transform.position + Vector2.right * groundRayForwardOffset + Vector2.down * groundRaySideOffset;

        distances[0] = GetGroundDistance(leftOrigin);
        distances[1] = GetGroundDistance(rightOrigin);

        return distances;
    }

    private float GetGroundDistance(Vector2 origin)
    {
        RaycastHit2D hit = Physics2D.Raycast(origin, Vector2.down, groundRayLength, platformLayer);
        if (hit.collider == null) return 1f;

        return hit.distance / groundRayLength;
    }

    private void OnDrawGizmos()
    {
        if (eyeDirections != null)
        {
            for (int i = 0; i < eyeDirections.Length; i++)
            {
                Vector2 direction = eyeDirections[i].normalized;
                RaycastHit2D platformHit = Physics2D.Raycast(transform.position, direction, eyeLength, platformLayer);
                RaycastHit2D deadlyHit = Physics2D.Raycast(transform.position, direction, eyeLength, deadlyLayer);
                Vector3 endPoint = transform.position + (Vector3)(direction * eyeLength);

                if (deadlyHit.collider != null && (platformHit.collider == null || deadlyHit.distance < platformHit.distance))
                {
                    Gizmos.color = Color.red;
                    endPoint = transform.position + (Vector3)(direction * deadlyHit.distance);
                }
                else if (platformHit.collider != null)
                {
                    Gizmos.color = Color.green;
                    endPoint = transform.position + (Vector3)(direction * platformHit.distance);
                }
                else
                    Gizmos.color = Color.yellow;

                Gizmos.DrawLine(transform.position, endPoint);
                Gizmos.DrawSphere(endPoint, 0.08f);
            }
        }

        Vector2 leftOrigin = (Vector2)transform.position + Vector2.left * groundRayForwardOffset + Vector2.down * groundRaySideOffset;
        Vector2 rightOrigin = (Vector2)transform.position + Vector2.right * groundRayForwardOffset + Vector2.down * groundRaySideOffset;

        DrawGroundRay(leftOrigin);
        DrawGroundRay(rightOrigin);
    }

    private void DrawGroundRay(Vector2 origin)
    {
        RaycastHit2D hit = Physics2D.Raycast(origin, Vector2.down, groundRayLength, platformLayer);
        Gizmos.color = hit.collider != null ? Color.cyan : Color.magenta;
        Vector3 endPoint = hit.collider != null ? (Vector3)hit.point : (Vector3)(origin + Vector2.down * groundRayLength);

        Gizmos.DrawLine(origin, endPoint);
        Gizmos.DrawSphere(endPoint, 0.06f);
    }
}
