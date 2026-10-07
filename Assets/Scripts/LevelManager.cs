using System;
using System.Collections;
using UnityEngine;

public class LevelManager : MonoBehaviour
{
    [Header("Levels")]
    [SerializeField] private GameObject[] levels;

    [Header("Spawn Point")]
    [SerializeField] private Transform levelSpawnPoint;

    [Header("Race Manager")]
    [SerializeField] private RaceManager raceManager;

    [Header("Background")]
    [SerializeField] private Transform background;

    [Header("Settings")]
    [SerializeField] private bool preventSameLevelRepeating = true;

    [Header("Result Flash")]
    [SerializeField] private float flashDuration = 0.2f;

    private GameObject currentLevel;

    private int previousLevelIndex = -1;

    private LevelSetup currentLevelSetup;

    private void Awake()
    {
        LoadRandomLevel();
    }

    public void LoadRandomLevel()
    {
        if (levels == null || levels.Length == 0)
        {
            Debug.LogError(
                "LevelManager: No levels have been assigned!"
            );

            return;
        }

        int randomIndex =
            GetRandomLevelIndex();

        Vector3 spawnPosition =
            Vector3.zero;

        if (levelSpawnPoint != null)
        {
            spawnPosition =
                levelSpawnPoint.position;
        }

        currentLevel =
            Instantiate(
                levels[randomIndex],
                spawnPosition,
                Quaternion.identity
            );

        previousLevelIndex =
            randomIndex;

        currentLevelSetup =
            currentLevel.GetComponent<LevelSetup>();

        if (currentLevelSetup == null)
        {
            Debug.LogError(
                "LevelManager: The spawned level does not have a LevelSetup component!"
            );
        }

        AssignRaceManagerToFinish();

        Debug.Log(
            "Loaded Level: " +
            levels[randomIndex].name
        );
    }

    public void ResetLevel(Action onComplete)
    {
        if (levels == null || levels.Length == 0)
        {
            Debug.LogError(
                "LevelManager: No levels have been assigned!"
            );

            return;
        }

        if (currentLevel != null)
        {
            Destroy(currentLevel);

            currentLevel =
                null;

            currentLevelSetup =
                null;
        }

        StartCoroutine(
            LoadNewLevelAfterDestroy(onComplete)
        );
    }

    private IEnumerator LoadNewLevelAfterDestroy(
        Action onComplete
    )
    {
        yield return null;

        int randomIndex =
            GetRandomLevelIndex();

        Vector3 spawnPosition =
            Vector3.zero;

        if (levelSpawnPoint != null)
        {
            spawnPosition =
                levelSpawnPoint.position;
        }

        currentLevel =
            Instantiate(
                levels[randomIndex],
                spawnPosition,
                Quaternion.identity
            );

        previousLevelIndex =
            randomIndex;

        currentLevelSetup =
            currentLevel.GetComponent<LevelSetup>();

        if (currentLevelSetup == null)
        {
            Debug.LogError(
                "LevelManager: The spawned level does not have a LevelSetup component!"
            );
        }

        AssignRaceManagerToFinish();

        Debug.Log(
            "Loaded New Level: " +
            levels[randomIndex].name
        );

        if (onComplete != null)
        {
            onComplete();
        }
    }

    private void AssignRaceManagerToFinish()
    {
        if (raceManager == null)
        {
            Debug.LogError(
                "LevelManager: RaceManager is not assigned!"
            );

            return;
        }

        if (currentLevel == null)
        {
            return;
        }

        RaceFinish[] finishes =
            currentLevel.GetComponentsInChildren<RaceFinish>(
                true
            );

        foreach (RaceFinish finish in finishes)
        {
            finish.SetRaceManager(
                raceManager
            );
        }
    }

    public void FlashResult(bool aiWon)
    {
        if (background == null)
        {
            Debug.LogWarning(
                "LevelManager: Background is not assigned!"
            );

            return;
        }

        StartCoroutine(
            FlashBackgroundCoroutine(
                background,
                aiWon
            )
        );
    }

    private IEnumerator FlashBackgroundCoroutine(
        Transform background,
        bool aiWon
    )
    {
        SpriteRenderer[] renderers =
            background.GetComponentsInChildren<SpriteRenderer>(
                true
            );

        Color flashColor;

        if (aiWon)
        {
            flashColor =
                Color.green;
        }
        else
        {
            flashColor =
                Color.red;
        }

        Color[] originalColors =
            new Color[renderers.Length];

        for (int i = 0; i < renderers.Length; i++)
        {
            originalColors[i] =
                renderers[i].color;

            Color newColor =
                flashColor;

            newColor.a =
                originalColors[i].a;

            renderers[i].color =
                newColor;
        }

        yield return new WaitForSeconds(
            flashDuration
        );

        for (int i = 0; i < renderers.Length; i++)
        {
            if (renderers[i] != null)
            {
                renderers[i].color =
                    originalColors[i];
            }
        }
    }

    public Transform GetCurrentStartPosition()
    {
        if (currentLevelSetup == null)
        {
            return null;
        }

        return currentLevelSetup.GetStartPosition();
    }

    public Transform GetCurrentGoal()
    {
        if (currentLevelSetup == null)
        {
            return null;
        }

        return currentLevelSetup.GetGoal();
    }

    private int GetRandomLevelIndex()
    {
        if (!preventSameLevelRepeating ||
            levels.Length == 1)
        {
            return UnityEngine.Random.Range(
                0,
                levels.Length
            );
        }

        int randomIndex;

        do
        {
            randomIndex =
                UnityEngine.Random.Range(
                    0,
                    levels.Length
                );
        }
        while (
            randomIndex ==
            previousLevelIndex
        );

        return randomIndex;
    }
}