using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class FinalScreenController : MonoBehaviour
{
    [SerializeField]
    private GameObject finalStatsScreen;
    [SerializeField]
    private GameObject thanksScreen;

    [SerializeField]
    private TMP_Text finalPointsText;
    [SerializeField]
    private TMP_Text correctAnswersText;
    [SerializeField]
    private TMP_Text avgResponseTimeText;


    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        finalPointsText.text = TrainingSessionManager.Instance.FinalPointsText();
        correctAnswersText.text = TrainingSessionManager.Instance.TotalCorrectAnswersText();
        avgResponseTimeText.text = TrainingSessionManager.Instance.AverageResponseTimeText();
    }

    public void ContinueToThanksScreen()
    {
        finalStatsScreen.SetActive(false);
        thanksScreen.SetActive(true);
    }

    public void EndTrainingSession()
    {
        SceneManager.LoadScene("StartScene");
    }
}
