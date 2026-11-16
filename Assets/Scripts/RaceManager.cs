
using System.Collections;
using UnityEngine;
using TMPro;

public class RaceManager : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private PlatformerAgent ai;
    [SerializeField] private Transform player;
    [SerializeField] private LevelManager levelManager;
    [SerializeField] private TMP_Text countdownText;

    [Header("Settings")]
    [SerializeField] private bool startRaceAutomatically = true;
    [SerializeField] private bool isCountdown = true;

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

    private Rigidbody2D playerRb;
    private Rigidbody2D aiRb;

    private RigidbodyConstraints2D playerOriginalConstraints;
    private RigidbodyConstraints2D aiOriginalConstraints;

    private Coroutine countdownCoroutine;
    private bool charactersFrozen;

    private void Start()
    {
        if (player != null) playerRb = player.GetComponent<Rigidbody2D>();
        if (ai != null) aiRb = ai.GetComponent<Rigidbody2D>();

        if (countdownText != null) countdownText.gameObject.SetActive(false);

        if (startRaceAutomatically) StartRace();
    }

    // ============================================================
    // START RACE
    // ============================================================

    public void StartRace()
    {
        StopCountdown();

        raceFinished = true;
        raceReady = false;

        attempts++;

        aiTime = 0f;
        playerTime = 0f;

        ResetPlayer();

        if (ai != null) ai.ResetAgentPosition();

        if (isCountdown)
        {
            FreezeCharacters();
            countdownCoroutine = StartCoroutine(CountdownRoutine());
        }
        else
        {
            BeginRace();
        }

        Debug.Log("RACE STARTING - Attempt " + attempts);
    }

    private IEnumerator CountdownRoutine()
    {
        if (countdownText != null) countdownText.gameObject.SetActive(true);

        for (int i = 3; i > 0; i--)
        {
            if (countdownText != null) countdownText.text = i.ToString();

            yield return new WaitForSeconds(1f);
        }

        if (countdownText != null) countdownText.text = "GO!";

        BeginRace();

        yield return new WaitForSeconds(0.5f);

        if (countdownText != null) countdownText.gameObject.SetActive(false);

        countdownCoroutine = null;
    }

    private void StopCountdown()
    {
        if (countdownCoroutine != null)
        {
            StopCoroutine(countdownCoroutine);
            countdownCoroutine = null;
        }

        if (countdownText != null) countdownText.gameObject.SetActive(false);
    }

    private void BeginRace()
    {
        UnfreezeCharacters();

        raceFinished = false;
        raceReady = true;

        raceStartTime = Time.time;

        Debug.Log("GO! RACE STARTED!");
    }

    // ============================================================
    // FREEZE / UNFREEZE
    // ============================================================

    private void FreezeCharacters()
    {
        if (charactersFrozen) return;

        if (playerRb != null)
        {
            playerOriginalConstraints = playerRb.constraints;
            playerRb.linearVelocity = Vector2.zero;
            playerRb.angularVelocity = 0f;
            playerRb.constraints = RigidbodyConstraints2D.FreezeAll;
        }

        if (aiRb != null)
        {
            aiOriginalConstraints = aiRb.constraints;
            aiRb.linearVelocity = Vector2.zero;
            aiRb.angularVelocity = 0f;
            aiRb.constraints = RigidbodyConstraints2D.FreezeAll;
        }

        charactersFrozen = true;
    }

    private void UnfreezeCharacters()
    {
        if (!charactersFrozen) return;

        if (playerRb != null)
        {
            playerRb.constraints = playerOriginalConstraints;
            playerRb.linearVelocity = Vector2.zero;
        }

        if (aiRb != null)
        {
            aiRb.constraints = aiOriginalConstraints;
            aiRb.linearVelocity = Vector2.zero;
        }

        charactersFrozen = false;
    }

    // ============================================================
    // PLAYER RESET
    // ============================================================

    private void ResetPlayer()
    {
        if (player == null) return;

        PlayerController playerController = player.GetComponent<PlayerController>();

        if (playerController != null)
        {
            playerController.ResetForRace();
            return;
        }

        Transform startPosition = GetCurrentStartPosition();

        if (startPosition != null) player.position = startPosition.position;

        Rigidbody2D rb = player.GetComponent<Rigidbody2D>();

        if (rb != null)
        {
            rb.linearVelocity = Vector2.zero;
            rb.angularVelocity = 0f;
        }
    }

    // ============================================================
    // LEVEL REFERENCES
    // ============================================================

    public Transform GetCurrentStartPosition()
    {
        if (levelManager == null) return null;

        return levelManager.GetCurrentStartPosition();
    }

    public Transform GetCurrentGoal()
    {
        if (levelManager == null) return null;

        return levelManager.GetCurrentGoal();
    }

    // ============================================================
    // RESET BEFORE NEW ROOM
    // ============================================================

    public void PrepareForNewRoom()
    {
        StopCountdown();

        raceReady = false;
        raceFinished = true;

        FreezeCharacters();

        Debug.Log("PREPARING NEW ROOM");
    }

    // ============================================================
    // NEW EPISODE RESET
    // ============================================================

    public void ResetForNewEpisode()
    {
        StartRace();
    }

    // ============================================================
    // FINISH
    // ============================================================

    public void PlayerFinished()
    {
        if (!raceReady || raceFinished) return;

        raceFinished = true;
        raceReady = false;

        playerWins++;

        playerTime = Time.time - raceStartTime;

        Debug.Log("PLAYER WON - AI LOSES!");

        if (ai != null) ai.PlayerWon();
    }

    public void AIFinished()
    {
        if (!raceReady || raceFinished) return;

        raceFinished = true;
        raceReady = false;

        aiWins++;

        aiTime = Time.time - raceStartTime;

        if (aiTime < bestAITime) bestAITime = aiTime;

        Debug.Log("AI WON!");

        if (ai != null) ai.ReachedGoal();
    }

    // ============================================================
    // DEATH
    // ============================================================

    public void PlayerDied()
    {
        if (raceFinished) return;

        playerDeaths++;

        Debug.Log("PLAYER DIED - RESETTING");

        ResetPlayer();
    }

    public void AIDied()
    {
        if (raceFinished) return;

        aiDeaths++;

        Debug.Log("AI DIED - RESETTING POSITION");

        if (ai != null) ai.ResetAgentPosition();
    }

    // ============================================================
    // GETTERS
    // ============================================================

    public float GetCurrentRaceTime()
    {
        if (raceFinished) return 0f;

        return Time.time - raceStartTime;
    }

    public float GetPlayerTime() { return playerTime; }
    public float GetAITime() { return aiTime; }
    public bool IsRaceFinished() { return raceFinished; }
    public bool IsRaceReady() { return raceReady && !raceFinished; }
    public int GetAttempts() { return attempts; }
    public int GetAIWins() { return aiWins; }
    public int GetPlayerWins() { return playerWins; }
    public int GetAIDeaths() { return aiDeaths; }
    public int GetPlayerDeaths() { return playerDeaths; }
    public float GetBestAITime() { return bestAITime; }
    public int GetCurrentLevel() { return currentLevel; }
    public PlatformerAgent GetAI() { return ai; }
}
