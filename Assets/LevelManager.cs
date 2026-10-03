using System;
using System.Collections;
using UnityEngine;

public class LevelManager : MonoBehaviour
{
    [Header("Levels")]
    [SerializeField] private GameObject[] levels;

    [Header("Spawn Point")]
    [SerializeField] private Transform levelSpawnPoint;

    [Header("Settings")]
    [SerializeField] private bool preventSameLevelRepeating = true;

    private GameObject currentLevel;
    private int previousLevelIndex = -1;

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
            currentLevel = null;
        }

        StartCoroutine(
            LoadNewLevelAfterDestroy(onComplete)
        );
    }

    private IEnumerator LoadNewLevelAfterDestroy(
        Action onComplete
    )
    {
        // Wait until Unity has actually destroyed
        // the previous level.
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

        Debug.Log(
            "Loaded New Level: " +
            levels[randomIndex].name
        );

        // Tell the agent that the new room
        // now exists and can be used.
        if (onComplete != null)
        {
            onComplete();
        }
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