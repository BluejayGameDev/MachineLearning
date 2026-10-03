using UnityEngine;

public class CourseSection : MonoBehaviour
{
    public enum SectionDifficulty
    {
        Easy,
        Medium,
        Hard
    }

    [Header("Section Settings")]
    public SectionDifficulty difficulty;

    public Transform startPoint;
    public Transform endPoint;
}