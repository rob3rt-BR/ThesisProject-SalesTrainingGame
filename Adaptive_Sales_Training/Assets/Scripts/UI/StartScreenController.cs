using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class StartScreenController : MonoBehaviour
{
    [Header("Topics")]
    [SerializeField]
    private List<SalesTopic> allTopics;

    [Header("Screens")]
    [SerializeField]
    private GameObject startScreen;
    [SerializeField]
    private GameObject explainScreen;

    [Header("Explanation UI")]
    [SerializeField]
    private GameObject[] texts;
    [SerializeField]
    private Button continueButton;
    [SerializeField]
    private Button backButton;
    
    private TrainingSessionManager session;

    private int currentTextIndex = 0;
    private RectTransform continueButtonTrans;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        session = TrainingSessionManager.Instance;

        if (session == null)
        {
            Debug.LogWarning("Session not started.");
            return;
        }
        session.StartSession("P_115", TrainingVersion.Traditional, allTopics);

        // On click => Go forward / back in texts
        continueButton.onClick.AddListener(ContinuePressed);
        backButton.onClick.AddListener(BackPressed);

        continueButtonTrans = continueButton.gameObject.GetComponent<RectTransform>();
    }

    public void StartGameNormal()
    {
        startScreen.SetActive(false);
        explainScreen.SetActive(true);
    }

    public void QuitGame()
    {
        Debug.Log("Game exited.");
        Application.Quit();
    }

    private void ContinuePressed()
    {
        texts[currentTextIndex].SetActive(false);
        currentTextIndex++;

        // If on second text => Translate continueButton to the right and enable backButton
        if (currentTextIndex == 1)
        {
            var newPos = continueButtonTrans.anchoredPosition;
            newPos.x += 130;
            continueButtonTrans.anchoredPosition = newPos;
            backButton.gameObject.SetActive(true);
        }

        // If index outside texts range => Go to TopicMenuScene
        if (currentTextIndex == texts.Length)
        {
            CleanUp();
            explainScreen.SetActive(false);
            SceneManager.LoadScene("TopicMenuScene");
            Debug.Log("Loading TopicMenuScene.");
            return;
        }
        texts[currentTextIndex].SetActive(true);
    }

    private void BackPressed()
    {
        texts[currentTextIndex].SetActive(false);
        currentTextIndex--;

        // If on first text again => Reset continueButton to center and disable backButton
        if (currentTextIndex == 0)
        {
            var newPos = continueButtonTrans.anchoredPosition;
            newPos.x = 0;
            continueButtonTrans.anchoredPosition = newPos;
            backButton.gameObject.SetActive(false);
        }
        texts[currentTextIndex].SetActive(true);
    }

    private void CleanUp()
    {
        continueButton.onClick.RemoveListener(ContinuePressed);
        backButton.onClick.RemoveListener(BackPressed);
    }
}
