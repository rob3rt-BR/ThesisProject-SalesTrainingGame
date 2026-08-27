using System.Collections.Generic;
using TMPro;
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
    private GameObject assignValuesScreen;
    [SerializeField]
    private GameObject explainScreen;

    [Header("Assign Values UI")]
    [SerializeField]
    private TMP_Text participantIdText;
    [SerializeField]
    private Button generateParticipantValuesButton;
    [SerializeField]
    private Button continueToExplanationButton;

    [Header("Explanation UI")]
    [SerializeField]
    private List<GameObject> texts;
    [SerializeField]
    private Button continueButton;
    [SerializeField]
    private Button backButton;
    [SerializeField]
    private ExplainScreenContent explainScreenContentNormal;
    [SerializeField]
    private ExplainScreenContent explainScreenContentAI;
    
    private TrainingSessionManager session;
    private string participantId;
    private TrainingVersion trainingVersion;

    private int participantIdCounter;

    private int currentTextIndex = 0;
    private RectTransform continueButtonTrans;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        session = TrainingSessionManager.Instance;

        if (session == null)
        {
            Debug.LogWarning("SessionManager not started.");
            return;
        }

        // On click => Generate participant ID and define training version based on that
        generateParticipantValuesButton.onClick.AddListener(GenerateParticipantValues);

        // On click => Show explanation screen and start session
        continueToExplanationButton.onClick.AddListener(StartExplanation);

        // On click => Go forward / back in texts
        continueButton.onClick.AddListener(ContinuePressed);
        backButton.onClick.AddListener(BackPressed);

        continueButtonTrans = continueButton.gameObject.GetComponent<RectTransform>();
    }

    public void StartGame()
    {
        startScreen.SetActive(false);
        assignValuesScreen.SetActive(true);
    }

    public void QuitGame()
    {
        Debug.Log("Game exited.");
        Application.Quit();
    }

    private void GenerateParticipantValues()
    {
        participantIdCounter = session.GetParticipantIdCounter();
        participantId = $"P_{participantIdCounter}";

        if (participantIdCounter % 2 == 0)
        {
            trainingVersion = TrainingVersion.Traditional;
            SetExplainScreenTextContent(explainScreenContentNormal);
        } else
        {
            trainingVersion = TrainingVersion.AI;
            SetExplainScreenTextContent(explainScreenContentAI);
        }

        participantIdText.text += $" {participantId}";

        generateParticipantValuesButton.interactable = false;
        continueToExplanationButton.interactable = true;
    }

    private void StartExplanation()
    {
        session.StartSession(participantId, trainingVersion, allTopics); // Setting temporal
        
        assignValuesScreen.SetActive(false);
        explainScreen.SetActive(true);
    }

    private void SetExplainScreenTextContent(ExplainScreenContent content)
    {
        if (content == null)
        {
            Debug.LogError("ExplainScreen is null");
            return;
        }

        if (content.texts == null)
        {
            Debug.LogError("ExplainScreen texts is null");
            return;
        }

        for (int i = 0; i < texts.Count; i++)
        {
            texts[i].GetComponent<TMP_Text>().text = content.texts[i];
        }
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
        if (currentTextIndex == texts.Count)
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
        generateParticipantValuesButton.onClick.RemoveListener(GenerateParticipantValues);
        continueToExplanationButton.onClick.RemoveListener(StartExplanation);
        continueButton.onClick.RemoveListener(ContinuePressed);
        backButton.onClick.RemoveListener(BackPressed);
    }
}
