using UnityEngine;

public class RaceManager : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private PlatformerAgent ai;
    [SerializeField] private Transform player;
    [SerializeField] private LevelManager levelManager;

    [Header("Settings")]
    [SerializeField] private bool startRaceAutomatically = true;

    [Header("Level")]
    [SerializeField] private int currentLevel = 1;

    private float raceStartTime;
    private bool raceFinished;
    private bool raceReady;

    private int attempts;
    private int aiWins;
    private int playerWins;
    private int aiDeaths;
    private int playerDeaths;

    private float aiTime;
    private float playerTime;

    private float bestAITime = Mathf.Infinity;

    private void Start()
    {
        if (startRaceAutomatically)
        {
            StartRace();
        }
    }

    public void StartRace()
    {
        raceFinished = false;
        raceReady = true;

        attempts++;

        aiTime = 0f;
        playerTime = 0f;

        ResetPlayer();

        raceStartTime = Time.time;

        Debug.Log(
            "RACE STARTED - Attempt " +
            attempts
        );
    }

    private void ResetPlayer()
    {
        if (player == null)
        {
            return;
        }

        PlayerController playerController =
            player.GetComponent<PlayerController>();

        if (playerController != null)
        {
            playerController.ResetForRace();
            return;
        }

        Transform startPosition =
            GetCurrentStartPosition();

        if (startPosition != null)
        {
            player.position =
                startPosition.position;
        }

        Rigidbody2D rb =
            player.GetComponent<Rigidbody2D>();

        if (rb != null)
        {
            rb.linearVelocity =
                Vector2.zero;

            rb.angularVelocity =
                0f;
        }
    }

    // ============================================================
    // LEVEL REFERENCES
    // ============================================================

    public Transform GetCurrentStartPosition()
    {
        return levelManager.GetCurrentStartPosition();
    }

    public Transform GetCurrentGoal()
    {
        if (levelManager == null)
        {
            return null;
        }

        return levelManager.GetCurrentGoal();
    }

    // ============================================================
    // RESET BEFORE NEW ROOM
    // ============================================================

    public void PrepareForNewRoom()
    {
        raceReady = false;
        raceFinished = true;

        ResetPlayer();

        if (ai != null)
        {
            ai.ResetAgentPosition();
        }

        Debug.Log(
            "AI AND PLAYER RESET BEFORE NEW ROOM"
        );
    }

    // ============================================================
    // NEW EPISODE RESET
    // ============================================================

    public void ResetForNewEpisode()
    {
        ResetPlayer();

        if (ai != null)
        {
            ai.ResetAgentPosition();
        }

        raceFinished = false;
        raceReady = true;

        raceStartTime = Time.time;

        Debug.Log(
            "PLAYER AND AI RESET - NEW EPISODE"
        );
    }

    // ============================================================
    // FINISH
    // ============================================================

    public void PlayerFinished()
    {
        if (!raceReady || raceFinished)
        {
            return;
        }

        raceFinished = true;
        raceReady = false;

        playerWins++;

        playerTime =
            Time.time -
            raceStartTime;

        Debug.Log(
            "PLAYER WON - AI LOSES!"
        );

        if (ai != null)
        {
            ai.PlayerWon();
        }
    }

    public void AIFinished()
    {
        if (!raceReady || raceFinished)
        {
            return;
        }

        raceFinished = true;
        raceReady = false;

        aiWins++;

        aiTime =
            Time.time -
            raceStartTime;

        if (aiTime < bestAITime)
        {
            bestAITime =
                aiTime;
        }

        Debug.Log(
            "AI WON!"
        );

        if (ai != null)
        {
            ai.ReachedGoal();
        }
    }

    // ============================================================
    // DEATH
    // ============================================================

    public void PlayerDied()
    {
        if (raceFinished)
        {
            return;
        }

        playerDeaths++;

        Debug.Log(
            "PLAYER DIED - RESETTING"
        );

        ResetPlayer();
    }

    public void AIDied()
    {
        if (raceFinished)
        {
            return;
        }

        aiDeaths++;

        Debug.Log(
            "AI DIED - RESETTING POSITION"
        );

        if (ai != null)
        {
            ai.ResetAgentPosition();
        }
    }

    // ============================================================
    // GETTERS
    // ============================================================

    public float GetCurrentRaceTime()
    {
        if (raceFinished)
        {
            return 0f;
        }

        return Time.time -
            raceStartTime;
    }

    public float GetPlayerTime()
    {
        return playerTime;
    }

    public float GetAITime()
    {
        return aiTime;
    }

    public bool IsRaceFinished()
    {
        return raceFinished;
    }

    public int GetAttempts()
    {
        return attempts;
    }

    public int GetAIWins()
    {
        return aiWins;
    }

    public int GetPlayerWins()
    {
        return playerWins;
    }

    public int GetAIDeaths()
    {
        return aiDeaths;
    }

    public int GetPlayerDeaths()
    {
        return playerDeaths;
    }

    public float GetBestAITime()
    {
        return bestAITime;
    }

    public int GetCurrentLevel()
    {
        return currentLevel;
    }

    public PlatformerAgent GetAI()
    {
        return ai;
    }
}