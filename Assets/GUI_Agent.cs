using TMPro;
using UnityEngine;

public class GUI_Agent : MonoBehaviour
{
    [SerializeField] private PlatformerAgent agent;

    [SerializeField] private TMP_Text episodeNumberText;
    [SerializeField] private TMP_Text rewardAmountText;

    private string debugEpisode;
    private string debugReward;

    // Update is called once per frame
    void Update()
    {
        debugEpisode = "Episode: " + agent.currentEpisode + " - Step: " + agent.StepCount;
        debugReward = "Reward: " + agent.cumulativeReward.ToString();

        episodeNumberText.text = debugEpisode;
        rewardAmountText.text = debugReward;

        if (agent.cumulativeReward >= 0)
        {
            rewardAmountText.color = Color.green;
        }
        else
        {
            rewardAmountText.color = Color.red;
        }
    }
}
