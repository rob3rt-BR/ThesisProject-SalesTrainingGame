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
    private GameObject assignTrainingModeScreen;
    [SerializeField]
    private GameObject explainScreen;

    [Header("Assign Training Mode UI")]
    [SerializeField]
    private GameObject passwordBox;
    [SerializeField]
    private TMP_InputField passwordInputField;
    [SerializeField]
    private TMP_Text wrongPasswordText;
    [SerializeField]
    private Button checkPasswortButton;

    [SerializeField]
    private GameObject assignTrainingModeElements;
    [SerializeField]
    private TMP_Text sessionIdText;
    [SerializeField]
    private TMP_Text trainingModeSetText;
    [SerializeField]
    private Button generateSessionIdButton;
    [SerializeField]
    private Button traditionalModeButton;
    [SerializeField]
    private Button aiModeButton;
    [SerializeField]
    private Button setModeButton;
    [SerializeField]
    private Button backToStartScreenButton;

    [Header("Explanation UI")]
    [SerializeField]
    private List<GameObject> texts;
    [SerializeField]
    private Button continueButton;
    [SerializeField]
    private Button backButton;
    [SerializeField]
    private ExplainScreenContent explainScreenContentTraditional;
    [SerializeField]
    private ExplainScreenContent explainScreenContentAI;

    private const string password = "PerroLocoST456+";
    private string inputFieldText;
    
    private TrainingSessionManager session;
    private TrainingVersion trainingVersion;

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

        // On end edit => Store input from password input field in variable to later compare for password
        passwordInputField.onEndEdit.AddListener(GetInput);

        // On click => Check whether input text from input field equals predefined password
        checkPasswortButton.onClick.AddListener(CheckPassword);

        // On click => Go back to start screen from assign training value screen
        backToStartScreenButton.onClick.AddListener(ReturnToStartScreen);

        // On click => Generate session ID
        generateSessionIdButton.onClick.AddListener(GenerateSessionID);

        // On click => Select normal training mode
        traditionalModeButton.onClick.AddListener(SelectTraditionalTrainingMode);

        // On click => Select AI training mode
        aiModeButton.onClick.AddListener(SelectAITrainingMode);

        // On click => Set training mode and set explain screen content based on that
        setModeButton.onClick.AddListener(SetTrainingMode);

        // On click => Go forward / back in texts
        continueButton.onClick.AddListener(ContinuePressed);
        backButton.onClick.AddListener(BackPressed);

        continueButtonTrans = continueButton.gameObject.GetComponent<RectTransform>();
    }

    public void StartGame()
    {
        // IF global session ID AND training version NOT set => Return
        if (session.SessionId == null && session.Version.Equals(TrainingVersion.Undefined))
        {
            Debug.LogWarning("Can't start game - No global session Id set and no training version specified.");
            return;
        }

        // Start tracking participant training session
        session.StartParticipantSession(allTopics);

        startScreen.SetActive(false);
        explainScreen.SetActive(true);
    }

    public void SetMode()
    {
        startScreen.SetActive(false);
        assignTrainingModeScreen.SetActive(true);
    }

    public void QuitGame()
    {
        Debug.Log("Game exited.");
        Application.Quit();
    }

    private void GetInput(string input)
    {
        inputFieldText = input;
    }

    private void CheckPassword()
    {
        // IF input field text set AND equals to predefined password => Deactivate password box and show assignTrainingModel screen UI elements
        // ELSE => Display wrong password text
        if (inputFieldText != null && inputFieldText.Equals(password))
        {
            passwordBox.SetActive(false);

            // Translate backToStartScreenButton by -20 units in the Y axis for clearer display
            RectTransform backStartButtonTrans = backToStartScreenButton.gameObject.GetComponent<RectTransform>();
            var newPos = backStartButtonTrans.anchoredPosition;
            newPos.y -= 20;
            backStartButtonTrans.anchoredPosition = newPos;

            backToStartScreenButton.interactable = false;
            assignTrainingModeElements.SetActive(true);
        } else
        {
            wrongPasswordText.gameObject.SetActive(true);
        }
    }

    private void ReturnToStartScreen()
    {
        // Reset assignTrainingMode screen UI elements for later reuse before returning to start screen
        ResetAssignTrainingModeUI();

        assignTrainingModeScreen.SetActive(false);
        startScreen.SetActive(true);
    }

    private void ResetAssignTrainingModeUI()
    {
        // Reset backToStartScreenButton to orginal position (+20 units in the Y axis)
        RectTransform backStartButtonTrans = backToStartScreenButton.gameObject.GetComponent<RectTransform>();
        var newPos = backStartButtonTrans.anchoredPosition;
        newPos.y += 20;
        backStartButtonTrans.anchoredPosition = newPos;

        // Reactivate password box and reset its internal values
        passwordBox.SetActive(true);
        wrongPasswordText.gameObject.SetActive(false);
        passwordInputField.text = null;
        inputFieldText = null;

        // Reset text attributes and deactivate assignTrainingMode screen UI elements
        sessionIdText.text = null;
        trainingModeSetText.text = null;
        assignTrainingModeElements.SetActive(false);
    }

    private void GenerateSessionID()
    {
        // Generate current global session ID internally in TrainingSessionManager => ONE session PER training version with X amount of participants
        session.GenerateGlobalSessionID();

        // Set text attribute of current global session ID
        sessionIdText.text = session.SessionId;
    }

    private void SelectTraditionalTrainingMode()
    {
        traditionalModeButton.interactable = false;

        trainingVersion = TrainingVersion.Traditional;

        aiModeButton.interactable = true;
    }

    private void SelectAITrainingMode()
    {
        aiModeButton.interactable = false;

        trainingVersion = TrainingVersion.AI;
    
        traditionalModeButton.interactable = true;
    }

    private void SetTrainingMode()
    {
        // Set current global session training version in TrainingSessionManager
        session.SetGlobalSessionTrainingVersion(trainingVersion);

        // Set text attribute of current global session training version
        trainingModeSetText.text = session.Version.ToString();

        // Set explain screen content of texts based on set training version
        SetExplainScreenTextContent(trainingVersion);

        backToStartScreenButton.interactable = true;
    }

    private void SetExplainScreenTextContent(TrainingVersion trainingVersion)
    {
        ExplainScreenContent content;

        if (trainingVersion.Equals(TrainingVersion.Traditional))
        {
            content = explainScreenContentTraditional;
        } 
        else if (trainingVersion.Equals(TrainingVersion.AI))
        {
            content = explainScreenContentAI;
        }
        else
        {
            Debug.LogError("Undefined TrainingVersion.");
            return;
        }

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
        // Clenaup Assign Training Mode Screen
        passwordInputField.onEndEdit.RemoveListener(GetInput);
        checkPasswortButton.onClick.RemoveListener(CheckPassword);

        backToStartScreenButton.onClick.RemoveListener(ReturnToStartScreen);
        generateSessionIdButton.onClick.RemoveListener(GenerateSessionID);
        traditionalModeButton.onClick.RemoveListener(SelectTraditionalTrainingMode);
        aiModeButton.onClick.RemoveListener(SelectAITrainingMode);
        setModeButton.onClick.RemoveListener(SetTrainingMode);

        // Cleanup Explanation Screen
        continueButton.onClick.RemoveListener(ContinuePressed);
        backButton.onClick.RemoveListener(BackPressed);
    }
}
