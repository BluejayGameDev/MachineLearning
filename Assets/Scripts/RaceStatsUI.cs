using TMPro;
using UnityEngine;

public class RaceStatsUI : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private RaceManager raceManager;
    [SerializeField] private PlatformerAgent ai;

    [Header("UI Text")]
    [SerializeField] private TMP_Text statsText;

    [Header("Debug Settings")]
    [SerializeField] private bool isDebugMode = true;

    [Header("Update Settings")]
    [SerializeField] private float updateInterval = 0.1f;

    private float updateTimer;

    private void Update()
    {
        updateTimer -= Time.deltaTime;

        if (updateTimer > 0f)
        {
            return;
        }

        updateTimer = updateInterval;

        UpdateStats();
    }

    private void UpdateStats()
    {
        if (raceManager == null ||
            ai == null ||
            statsText == null)
        {
            return;
        }

        if (!isDebugMode)
        {
            statsText.text =
                "AI Wins:       " +
                raceManager.GetAIWins() + "\n" +

                "Player Wins:   " +
                raceManager.GetPlayerWins() + "\n";

            return;
        }

        float currentTime =
            raceManager.GetCurrentRaceTime();

        float bestTime =
            raceManager.GetBestAITime();

        string bestTimeText;

        if (float.IsInfinity(bestTime))
        {
            bestTimeText = "--";
        }
        else
        {
            bestTimeText =
                bestTime.ToString("F2") + "s";
        }

        // This is the AI's episode step count.
        // It should reset when the episode resets.
        int trainingSteps =
            ai.StepCount;

        statsText.text =
            "AI TRAINING\n" +
            "────────────────────\n" +
            "Level:           " +
            raceManager.GetCurrentLevel() + "\n" +

            "Episode:         " +
            ai.currentEpisode + "\n" +

            "Training Steps:  " +
            trainingSteps + "\n" +

            "Attempts:        " +
            raceManager.GetAttempts() + "\n\n" +

            "AI Reward:       " +
            ai.cumulativeReward.ToString("F3") + "\n" +

            "AI Wins:         " +
            raceManager.GetAIWins() + "\n" +

            "Player Wins:     " +
            raceManager.GetPlayerWins() + "\n\n" +

            "AI Deaths:       " +
            raceManager.GetAIDeaths() + "\n" +

            "Player Deaths:   " +
            raceManager.GetPlayerDeaths() + "\n\n" +

            "Race Time:       " +
            currentTime.ToString("F2") + "s\n" +

            "Best AI Time:    " +
            bestTimeText;
    }
}
