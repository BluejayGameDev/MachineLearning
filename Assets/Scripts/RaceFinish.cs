using UnityEngine;

public class RaceFinish : MonoBehaviour
{
    private RaceManager raceManager;

    private void Awake()
    {
        raceManager =
            FindFirstObjectByType<RaceManager>();

        if (raceManager == null)
        {
            Debug.LogError(
                "RaceFinish: Could not find a RaceManager in the scene!"
            );
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (raceManager == null)
        {
            return;
        }

        PlayerController player =
            collision.GetComponent<PlayerController>();

        if (player != null)
        {
            Debug.Log(
                "PLAYER REACHED THE FINISH!"
            );

            raceManager.PlayerFinished();
            return;
        }

        if (collision.CompareTag("AI"))
        {
            Debug.Log(
                "AI REACHED THE FINISH!"
            );

            raceManager.AIFinished();
        }
    }
}