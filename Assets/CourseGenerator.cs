using UnityEngine;

public class CourseGenerator : MonoBehaviour
{
    [System.Serializable]
    public class SpawnPoint
    {
        public Transform point;
        public GameObject[] objects;
    }

    [Header("Spawn Points")]
    public SpawnPoint[] spawnPoints;

    void Start()
    {
        GenerateLevel();
    }

    public void GenerateLevel()
    {
        ClearLevel();

        foreach (SpawnPoint spawnPoint in spawnPoints)
        {
            // Make sure the spawn point has objects assigned
            if (spawnPoint.point == null || 
                spawnPoint.objects == null || 
                spawnPoint.objects.Length == 0)
            {
                continue;
            }

            // Pick a random object for this spawn point
            int randomIndex = Random.Range(0, spawnPoint.objects.Length);

            GameObject prefab = spawnPoint.objects[randomIndex];

            // Spawn it
            Instantiate(prefab, spawnPoint.point.position, spawnPoint.point.rotation, transform);
        }
    }

    void ClearLevel()
    {
        foreach (Transform child in transform)
        {
            Destroy(child.gameObject);
        }
    }
}