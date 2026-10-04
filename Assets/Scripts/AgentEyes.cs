using UnityEngine;

public class AgentEyes : MonoBehaviour
{
    [Header("Eye Settings")]
    [SerializeField] private float eyeLength = 5f;

    public float EyeLength
    {
        get { return eyeLength; }
    }

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
        float[] distances =
            new float[eyeDirections.Length];

        for (int i = 0;
             i < eyeDirections.Length;
             i++)
        {
            Vector2 direction =
                eyeDirections[i].normalized;

            RaycastHit2D platformHit =
                Physics2D.Raycast(
                    transform.position,
                    direction,
                    eyeLength,
                    platformLayer
                );

            RaycastHit2D deadlyHit =
                Physics2D.Raycast(
                    transform.position,
                    direction,
                    eyeLength,
                    deadlyLayer
                );

            float closestDistance =
                eyeLength;

            if (platformHit.collider != null)
            {
                closestDistance =
                    Mathf.Min(
                        closestDistance,
                        platformHit.distance
                    );
            }

            if (deadlyHit.collider != null)
            {
                closestDistance =
                    Mathf.Min(
                        closestDistance,
                        deadlyHit.distance
                    );
            }

            distances[i] =
                closestDistance;
        }

        return distances;
    }

    public float[] GetEyeTypes()
    {
        float[] types =
            new float[eyeDirections.Length];

        for (int i = 0;
             i < eyeDirections.Length;
             i++)
        {
            Vector2 direction =
                eyeDirections[i].normalized;

            RaycastHit2D platformHit =
                Physics2D.Raycast(
                    transform.position,
                    direction,
                    eyeLength,
                    platformLayer
                );

            RaycastHit2D deadlyHit =
                Physics2D.Raycast(
                    transform.position,
                    direction,
                    eyeLength,
                    deadlyLayer
                );

            if (platformHit.collider == null &&
                deadlyHit.collider == null)
            {
                types[i] = 0f;
            }
            else if (
                deadlyHit.collider != null &&
                (
                    platformHit.collider == null ||
                    deadlyHit.distance <
                    platformHit.distance
                )
            )
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

    private void OnDrawGizmos()
    {
        if (eyeDirections == null)
        {
            return;
        }

        for (int i = 0;
             i < eyeDirections.Length;
             i++)
        {
            Vector2 direction =
                eyeDirections[i].normalized;

            RaycastHit2D platformHit =
                Physics2D.Raycast(
                    transform.position,
                    direction,
                    eyeLength,
                    platformLayer
                );

            RaycastHit2D deadlyHit =
                Physics2D.Raycast(
                    transform.position,
                    direction,
                    eyeLength,
                    deadlyLayer
                );

            Vector3 endPoint =
                transform.position +
                (Vector3)(
                    direction *
                    eyeLength
                );

            if (
                deadlyHit.collider != null &&
                (
                    platformHit.collider == null ||
                    deadlyHit.distance <
                    platformHit.distance
                )
            )
            {
                Gizmos.color =
                    Color.red;

                endPoint =
                    transform.position +
                    (Vector3)(
                        direction *
                        deadlyHit.distance
                    );
            }
            else if (platformHit.collider != null)
            {
                Gizmos.color =
                    Color.green;

                endPoint =
                    transform.position +
                    (Vector3)(
                        direction *
                        platformHit.distance
                    );
            }
            else
            {
                Gizmos.color =
                    Color.yellow;
            }

            Gizmos.DrawLine(
                transform.position,
                endPoint
            );

            Gizmos.DrawSphere(
                endPoint,
                0.08f
            );
        }
    }
}