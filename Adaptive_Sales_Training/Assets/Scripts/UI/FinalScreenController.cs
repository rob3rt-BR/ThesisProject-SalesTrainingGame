using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class FinalScreenController : MonoBehaviour
{
    [Header("Screens")]
    [SerializeField]
    private GameObject finalStatsScreen;
    [SerializeField]
    private GameObject thanksScreen;

    [Header("Final Results UI")]
    [SerializeField]
    private TMP_Text finalPointsText;
    [SerializeField]
    private TMP_Text correctAnswersText;
    [SerializeField]
    private TMP_Text avgResponseTimeText;

    private readonly TrainingSessionManager session = TrainingSessionManager.Instance;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        finalPointsText.text = session.TotalPointsText();
        correctAnswersText.text = session.TotalCorrectAnswersText();
        avgResponseTimeText.text = session.FinalAvgResponseTimeText();
    }

    public void ContinueToThanksScreen()
    {
        // Export final participant training data to predefined local storage directory
        session.ExportFinalTrainingData();
        
        finalStatsScreen.SetActive(false);
        thanksScreen.SetActive(true);
    }

    public void EndTrainingSession()
    {
        SceneManager.LoadScene("StartScene");
        Debug.Log("Returning to StartScene.");
    }
}
