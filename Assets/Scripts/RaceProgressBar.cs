
using UnityEngine;

public class RaceProgressBar : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private PlatformerAgent ai;
    [SerializeField] private Transform player;
    [SerializeField] private LevelManager levelManager;

    [Header("UI")]
    [SerializeField] private RectTransform track;
    [SerializeField] private RectTransform aiIcon;
    [SerializeField] private RectTransform playerIcon;

    [Header("Settings")]
    [SerializeField] private float aiVerticalOffset = 12f;
    [SerializeField] private float playerVerticalOffset = -12f;

    private void LateUpdate()
    {
        if (ai == null || player == null || levelManager == null || track == null) return;

        Transform start = levelManager.GetCurrentStartPosition();
        Transform goal = levelManager.GetCurrentGoal();

        if (start == null || goal == null) return;

        UpdateMarker(aiIcon, ai.transform.position.x, start.position.x, goal.position.x, aiVerticalOffset);
        UpdateMarker(playerIcon, player.position.x, start.position.x, goal.position.x, playerVerticalOffset);
    }

    private void UpdateMarker(RectTransform marker, float currentX, float startX, float goalX, float verticalOffset)
    {
        if (marker == null) return;

        float distance = goalX - startX;

        if (Mathf.Abs(distance) < 0.001f) return;

        float progress = Mathf.Clamp01((currentX - startX) / distance);

        float halfWidth = track.rect.width * 0.5f;
        float markerPadding = marker.rect.width * 0.5f;
        float usableWidth = Mathf.Max(0f, halfWidth - markerPadding);

        float xPosition = Mathf.Lerp(-usableWidth, usableWidth, progress);

        marker.anchoredPosition = new Vector2(xPosition, verticalOffset);
    }
}
