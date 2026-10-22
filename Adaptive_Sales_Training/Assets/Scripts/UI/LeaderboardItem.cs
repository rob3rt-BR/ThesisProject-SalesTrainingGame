using TMPro;
using UnityEngine;

public class LeaderboardItem : MonoBehaviour
{
    [SerializeField]
    private TMP_Text playerName;
    [SerializeField]
    private TMP_Text playerStats;

    public void InitializeItem()
    {
        playerName = gameObject.GetComponent<TMP_Text>();
        playerStats = gameObject.GetComponent<TMP_Text>();
    }

    public void SetPlayerTexts(string nameText, string statsText)
    {
        playerName.text = nameText;
        playerStats.text = statsText;
    }
}
