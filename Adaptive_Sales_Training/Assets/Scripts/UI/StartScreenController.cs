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
    private GameObject passwordScreen;
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
    [SerializeField]
    private GameObject leaderboardsButton;

    [Header("Password UI")]
    [SerializeField]
    private GameObject passwordBox;
    [SerializeField]
    private TMP_InputField passwordInputField;
    [SerializeField]
    private TMP_Text wrongPasswordText;
    [SerializeField]
    private Button checkPasswortButton;
    [SerializeField]
    private Button pswdBackButton;

    [Header("Assign Training Mode UI")]
    [SerializeField]
    private GameObject assignTrainingModeGroup;
    [SerializeField]
    private TMP_Text participantIdText;
    [SerializeField]
    private TMP_Text trainingModeSetText;
    [SerializeField]
    private TMP_InputField participantIdInputField;
    [SerializeField]
    private Button setParticipantIdButton;
    [SerializeField]
    private Button traditionalModeButton;
    [SerializeField]
    private Button aiModeButton;
    [SerializeField]
    private Button setModeButton;
    [SerializeField]
    private Button backToStartButton;


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
    private string passwordInputFieldText;
    private string participantIdInputFieldText;

    private CanvasGroup backToStartButtonCanvasGrp;

    private int currentTextIndex;
    private RectTransform continueButtonRect;
    
    private TrainingSessionManager session;
    private TrainingVersion trainingVersion;

    private List<ElementAnimation> startScreenElements;
    private List<ElementAnimation> passwordScreenElements;
    private List<ElementAnimation> assignTrainingModeScreenElements;
    private List<ElementAnimation> explainScreenElements;


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
        passwordInputField.onEndEdit.AddListener(GetPasswordInput);

        // On click => Check whether input text from input field equals predefined password
        checkPasswortButton.onClick.AddListener(CheckPassword);

        // On click => Go back to start screen from password screen
        pswdBackButton.onClick.AddListener(PswdReturnToStartScreen);

        // On end edit => Store input from participantId input field in variable to later set to participant Id in TrainingSessionManager
        participantIdInputField.onEndEdit.AddListener(GetParticipantIDInput);

        // On click => Generate session ID
        setParticipantIdButton.onClick.AddListener(FixParticipantID);

        // On click => Select normal training mode
        traditionalModeButton.onClick.AddListener(SelectTraditionalTrainingMode);

        // On click => Select AI training mode
        aiModeButton.onClick.AddListener(SelectAITrainingMode);

        // On click => Set training mode and set explain screen content based on that
        setModeButton.onClick.AddListener(SetTrainingMode);

        // On click => Go back to start screen from assign training value screen
        backToStartButton.onClick.AddListener(ReturnToStartScreen);

        // On click => Go forward / back in texts
        continueButton.onClick.AddListener(ContinuePressed);
        backButton.onClick.AddListener(BackPressed);

        // Initialize backToStartButton canvas group component
        backToStartButtonCanvasGrp = backToStartButton.GetComponent<CanvasGroup>();

        continueButtonRect = continueButton.GetComponent<RectTransform>();

        currentTextIndex = 0;

        // Initialize ElementAnimation objects of each element per UI screen
        InitializeElementAnimationGroups();
    }

    private void InitializeElementAnimationGroups()
    {
        // Start screen elements
        ElementAnimation welcomeTitleElement = animationController.GetElementAnimation(welcomeTitle);
        ElementAnimation startButtonElement = animationController.GetElementAnimation(startButton);
        ElementAnimation lowerButtonsElement = animationController.GetElementAnimation(lowerButtons);
        ElementAnimation leaderboardsElement = animationController.GetElementAnimation(leaderboardsButton);

        startScreenElements = new() {welcomeTitleElement, startButtonElement, lowerButtonsElement, leaderboardsElement};
        
        // Password screen elements
        ElementAnimation passwordBoxElement = animationController.GetElementAnimation(passwordBox);
        ElementAnimation pswdBackButtonElement = animationController.GetElementAnimation(pswdBackButton.gameObject);

        passwordScreenElements = new() {passwordBoxElement, pswdBackButtonElement};

        // Assign training values screen elements
        ElementAnimation assignTrainingModeGroupElement = animationController.GetElementAnimation(assignTrainingModeGroup);
        ElementAnimation backToStartButtonElement = animationController.GetElementAnimation(backToStartButton.gameObject);

        assignTrainingModeScreenElements = new() {assignTrainingModeGroupElement, backToStartButtonElement};

        // Assign explian screen elements
        ElementAnimation explainTitleElement = animationController.GetElementAnimation(explainTitle);
        ElementAnimation explainTextsElement = animationController.GetElementAnimation(explainTexts);
        ElementAnimation textNavButtonsElement = animationController.GetElementAnimation(textNavButtons);

        explainScreenElements = new() {explainTitleElement, explainTextsElement, textNavButtonsElement};
    }

    public void StartGame()
    {
        // IF participant Id OR training version NOT set => Return
        if (session.ParticipantId == null || IsInvalidParticipantID(session.ParticipantId))
        {
            Debug.LogWarning("Can't start game - No participant Id specified.");
            return;
        }
        if (session.Version == TrainingVersion.Undefined)
        {
            Debug.LogWarning("Can't start game - No training version specified.");
            return;
        }

        // Start tracking participant training session
        session.StartParticipantSession(allTopics);

        // Set explain screen content of texts based on set training version
        SetExplainScreenTextContent(session.Version);

        // Animate screen change: start screen -> explain screen
        animationController.AnimateScreenChange(startScreenElements, explainScreenElements, startScreen, explainScreen);
    }

    public void SetMode()
    {
        // Animate screen change: start screen -> password screen
        animationController.AnimateScreenChange(startScreenElements, passwordScreenElements, startScreen, passwordScreen);
    }

    public void QuitGame()
    {
        Debug.Log("Game exited.");
        Application.Quit();
    }

    public void ViewLeaderboards()
    {
        // Animate screen change: start screen -> end of StartScene and load LeaderboardsScene
        animationController.AnimateSceneEnd(startScreenElements, "LeaderboardsScene");
    }

    private void GetPasswordInput(string input)
    {
        passwordInputFieldText = input;
    }

    private void CheckPassword()
    {
        // IF input field text set AND equals to predefined password => Animate screen change: password screen -> assignTrainingMode screen,
        // and disable backToStartButton canvas group interactbale and blocksRayCasts properties
        // ELSE => Display wrong password text
        if (passwordInputFieldText != null && passwordInputFieldText.Equals(password))
        {
            animationController.EnableElementInteraction(backToStartButton.gameObject, enableStart:true, postAnimation: true);
            animationController.AnimateScreenChange(passwordScreenElements, assignTrainingModeScreenElements, passwordScreen, assignTrainingModeScreen);
        } 
        else
        {
            wrongPasswordText.gameObject.SetActive(true);
        }
    }

    private void PswdReturnToStartScreen()
    {
        // Animate screen change: password screen -> start screen
        animationController.AnimateScreenChange(passwordScreenElements, startScreenElements, passwordScreen, startScreen);
    }

    private void ReturnToStartScreen()
    {
        // Animate screen change: assignTrainingMode screen -> start screen, 
        animationController.AnimateScreenChange(assignTrainingModeScreenElements, startScreenElements, assignTrainingModeScreen, startScreen);

        // Reset password screen and assignTrainingMode screen UI interal elements for later reuse
        ResetPasswordAndAssignTrainingModeGroupUI();
    }

    private void ResetPasswordAndAssignTrainingModeGroupUI()
    {
        // Reset password box internal values
        wrongPasswordText.gameObject.SetActive(false);
        passwordInputField.text = null;
        passwordInputFieldText = null;

        // Reset text attributes of assignTrainingMode screen UI elements
        participantIdText.text = null;
        participantIdInputField.text = null;
        participantIdInputFieldText = null;
        trainingModeSetText.text = null;
        traditionalModeButton.interactable = true;
        aiModeButton.interactable = true;
    }

    private void GetParticipantIDInput(string input)
    {
        participantIdInputFieldText = input;
    }

    private bool IsInvalidParticipantID(string id)
    {
        return !id.StartsWith("P-") || id[2..].Length != 8;
    }

    private void FixParticipantID()
    {
        if (IsInvalidParticipantID(participantIdInputFieldText))
        {
            Debug.LogWarning("Incorrect participant ID format");
            return;
        }
        // Set ParticipantId with stored value from input field
        session.SetParticipantID(participantIdInputFieldText);

        // Display ParticipantId on screen
        participantIdText.text = session.ParticipantId;
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

        // Enable backToStartScreenButton canvas group interactbale and blocksRayCasts properties
        backToStartButtonCanvasGrp.interactable = true;
        backToStartButtonCanvasGrp.blocksRaycasts = true;
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
        // If on second text => Translate continueButton to the right and enable backButton
        if (currentTextIndex == 0)
        {
            var newPos = continueButtonRect.anchoredPosition;
            newPos.x += 60;
            continueButtonRect.anchoredPosition = newPos;
            backButton.gameObject.SetActive(true);
        }
        // If index in last text => Cleanup and animation transition to next scene (TopicMenuScene)
        if (currentTextIndex == texts.Count - 1)
        {
            CleanUp();

            // Animate screen change: explain screen -> end of StartScene and load TopicMenuScene
            animationController.AnimateSceneEnd(explainScreenElements, "TopicMenuScene");
            return;
        }
        animationController.EnableElementInteraction(textNavButtons, enableStart:false, preAnimation:true, postAnimation: true);
        
        // Animate element change: current text -> next text (Forward)
        currentTextIndex = animationController.AnimateExplainTextChange(explainTexts, texts, currentTextIndex, true);
    }

    private void BackPressed()
    {
        if (currentTextIndex == 1)
        {
            var newPos = continueButtonRect.anchoredPosition;
            newPos.x = 0;
            continueButtonRect.anchoredPosition = newPos;
            backButton.gameObject.SetActive(false);
        }
        animationController.EnableElementInteraction(textNavButtons, enableStart:false, preAnimation:true, postAnimation: true);
        
        // Animate element change: current text -> next text (Backwards)
        currentTextIndex = animationController.AnimateExplainTextChange(explainTexts, texts, currentTextIndex, false);
    }

    private void CleanUp()
    {
        // Clenaup Assign Training Mode Screen
        passwordInputField.onEndEdit.RemoveListener(GetPasswordInput);
        checkPasswortButton.onClick.RemoveListener(CheckPassword);
        pswdBackButton.onClick.RemoveListener(PswdReturnToStartScreen);

        participantIdInputField.onEndEdit.RemoveListener(GetParticipantIDInput);
        setParticipantIdButton.onClick.RemoveListener(FixParticipantID);
        traditionalModeButton.onClick.RemoveListener(SelectTraditionalTrainingMode);
        aiModeButton.onClick.RemoveListener(SelectAITrainingMode);
        setModeButton.onClick.RemoveListener(SetTrainingMode);
        backToStartButton.onClick.RemoveListener(ReturnToStartScreen);

        // Cleanup Explanation Screen
        continueButton.onClick.RemoveListener(ContinuePressed);
        backButton.onClick.RemoveListener(BackPressed);

        Debug.Log("Cleanup Completed");
    }
}
