using UnityEngine;

public class LevelSetup : MonoBehaviour
{
    [Header("Level References")]
    [SerializeField] private Transform startPosition;
    [SerializeField] private Transform goal;

    public Transform GetStartPosition()
    {
        return startPosition;
    }

    public Transform GetGoal()
    {
        return goal;
    }
}