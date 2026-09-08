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

    [Header("Start UI")]
    [SerializeField]
    private GameObject welcomeTitle;
    [SerializeField]
    private GameObject startButton;
    [SerializeField]
    private GameObject lowerButtons;

    [Header("Password UI")]
    [SerializeField]
    private GameObject passwordBox;
    [SerializeField]
    private TMP_InputField passwordInputField;
    [SerializeField]
    private TMP_Text wrongPasswordText;
    [SerializeField]
    private Button checkPasswortButton;

    [Header("Assign Training Mode UI")]
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
    private GameObject explainTitle;
    [SerializeField]
    private GameObject explainTexts;
    [SerializeField]
    private GameObject textNavButtons;
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

    [Header("Animation Controller")]
    [SerializeField]
    private AnimationController animationController;

    private const string password = "PerroLocoST456+";
    private string inputFieldText;
    private bool enteredTrainingModeUI;
    
    private TrainingSessionManager session;
    private TrainingVersion trainingVersion;

    // private int currentTextIndex = 0;
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

        // Animate screen change: start screen -> explain screen
        animationController.AnimateScreenChange_StartToExplain(
        welcomeTitle, 
        startButton, 
        lowerButtons, 
        explainTitle, 
        explainTexts, 
        textNavButtons, 
        startScreen, 
        explainScreen);
    }

    public void SetMode()
    {
        // Animate screen change: start screen -> assignTrainMode screen (just password box)
        animationController.AnimateScreenChange_StartToPasswordBox(
        welcomeTitle, 
        startButton, 
        lowerButtons, 
        passwordBox, 
        backToStartScreenButton.gameObject, 
        startScreen, 
        assignTrainingModeScreen);
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
        // IF input field text set AND equals to predefined password => Animate element enable / disable: password box -> assignTrainingModeElements,
        // and disable backToStartScreenButton interactable
        // ELSE => Display wrong password text
        if (inputFieldText != null && inputFieldText.Equals(password))
        {
            enteredTrainingModeUI = true;

            animationController.AnimateElementEnable_CorrectPassword(
            passwordBox, 
            backToStartScreenButton.gameObject, 
            assignTrainingModeElements);
        } else
        {
            wrongPasswordText.gameObject.SetActive(true);
        }
    }

    private void ReturnToStartScreen()
    {
        // IF entered training mode setup UI (Correct password) => Animate screen change: assignTrainingMode screen (all UI elements) -> start screen, 
        // and restart password box and assignTrainingModeElements active states
        // ELSE => Animate screen change: assignTrainingMode screen (just password box) -> start screen
        if (enteredTrainingModeUI)
        {
            animationController.AnimateScreenChange_TrainingModeElementsToStart(
            welcomeTitle, 
            startButton, 
            lowerButtons, 
            backToStartScreenButton.gameObject, 
            assignTrainingModeElements, 
            startScreen, 
            assignTrainingModeScreen);
        } else
        {
            animationController.AnimateScreenChange_PasswordBoxToStart(
            welcomeTitle, 
            startButton, 
            lowerButtons, 
            passwordBox, 
            backToStartScreenButton.gameObject, 
            startScreen, 
            assignTrainingModeScreen);
        }

        // Reset assignTrainingMode screen UI interal elements for later reuse
        ResetAssignTrainingModeUI();
    }

    private void ResetAssignTrainingModeUI()
    {
        // Reset password box internal values
        wrongPasswordText.gameObject.SetActive(false);
        passwordInputField.text = null;
        inputFieldText = null;
        enteredTrainingModeUI = false;

        // Reset text attributes of assignTrainingMode screen UI elements
        sessionIdText.text = null;
        trainingModeSetText.text = null;
        traditionalModeButton.interactable = true;
        aiModeButton.interactable = true;
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

        // Enable backToStartScreenButton canvas group interactbale and blocksRayCasts properties
        CanvasGroup backToStartScreenButtonCanvasGrp = backToStartScreenButton.GetComponent<CanvasGroup>();
        backToStartScreenButtonCanvasGrp.interactable = true;
        backToStartScreenButtonCanvasGrp.blocksRaycasts = true;
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
            texts[i].GetComponentInChildren<TMP_Text>().text = content.texts[i];
        }
    }

    private void ContinuePressed()
    {
        // If index in last texts => Cleanup and animation transition to next scene (TopicMenuScene)
        if (animationController.CurrentTextIndex == texts.Count - 1)
        {
            CleanUp();

            // Animate screen change: explain screen -> end of StartScene and load TopicMenuScene
            animationController.AnimateScreenChange_ExplainEnd(explainTitle, explainTexts, textNavButtons, explainScreen);
            return;
        }
        // Animate element change: current text -> next text (Forward)
        animationController.AnimateElementChange_TextFadeForward(
        explainTexts, 
        texts, 
        textNavButtons);
    }

    private void BackPressed()
    {
        // Animate element change: current text -> next text (Backwards)
        animationController.AnimateElementChange_TextFadeBackward(
        explainTexts, 
        texts, 
        textNavButtons);
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

        Debug.Log("Cleanup Completed");
    }
}
