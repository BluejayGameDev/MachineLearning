using UnityEngine;

public class SecondaryGoal : MonoBehaviour
{
    [SerializeField] private float reward = 0.2f;

    private bool hasBeenReached;

    private void OnEnable()
    {
        hasBeenReached = false;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (hasBeenReached)
        {
            return;
        }

        if (collision.CompareTag("AI"))
        {
            hasBeenReached = true;

            gameObject.SetActive(false);

            Debug.Log("SECONDARY GOAL REACHED!");
        }
    }

    public float GetReward()
    {
        return reward;
    }

    public bool HasBeenReached()
    {
        return hasBeenReached;
    }
}