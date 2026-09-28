using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class ModuleFlowController : MonoBehaviour
{
    [Header("Screens")]
    [SerializeField]
    private GameObject infoScreen;
    [SerializeField]
    private GameObject questionScreen;
    [SerializeField]
    private GameObject currentPointScreen;
    [SerializeField]
    private GameObject finalPointScreen;

    [Header("Information UI")]
    [SerializeField]
    private TMP_Text moduleTitle;
    [SerializeField]
    private GameObject infoTextHolder;
    [SerializeField]
    private TMP_Text infoText;
    [SerializeField]
    private Button startQuestionsButton;

    [Header("Question UI")]
    [SerializeField]
    private GameObject qCounterAndTimer;
    [SerializeField]
    private TMP_Text questionCounterText;
    [SerializeField]
    private TMP_Text timerText;
    [SerializeField]
    private Slider timerBar;
    [SerializeField]
    private GameObject questionTextHolder;
    [SerializeField]
    private TMP_Text questionText;
    [SerializeField]
    private Button[] answerButtons;
    [SerializeField]
    private TMP_Text[] answerButtonsText;
    [SerializeField]
    private GameObject correctIndicator;
    [SerializeField]
    private GameObject wrongIndicator;
    [SerializeField]
    private GameObject pointsDisplayersHolder;
    [SerializeField]
    private TMP_Text[] pointsDisplayers;

    [Header("Points UI")]
    [SerializeField]
    private TMP_Text moduleTitleEnd;
    [SerializeField]
    private GameObject currentStatsBackground;
    [SerializeField]
    private GameObject pointsTitle;
    [SerializeField]
    private TMP_Text pointsText;
    [SerializeField]
    private GameObject correctAnswersTitle;
    [SerializeField]
    private TMP_Text correctAnswersText;
    [SerializeField]
    private GameObject qRoundCompleteText;
    [SerializeField]
    private Button continueButton;

    [Header("Final Points UI")]
    [SerializeField]
    private TMP_Text topicCompletedText;
    [SerializeField]
    private GameObject finalStatsBackground;
    [SerializeField]
    private GameObject finalPointsTitle;
    [SerializeField]
    private TMP_Text finalPointsText;
    [SerializeField]
    private GameObject topicCorrectAnsTitle;
    [SerializeField]
    private TMP_Text topicCorrectAnsText;
    [SerializeField]
    private GameObject topicAvgResponseTimeTitle;
    [SerializeField]
    private TMP_Text topicAvgResponseTimeText;
    [SerializeField]
    private Button finishTopicButton;

    [Header("Animation Controller")]
    [SerializeField]
    private AnimationController animationController;

    [Header("EventSystem")]
    [SerializeField]
    private EventSystem eventSystem;

    private List<ElementAnimation> infoScreenElements;
    private List<ElementAnimation> questionScreenElements;
    private List<ElementAnimation> currentPointScreenElements;
    private List<ElementAnimation> currentStatsElements;
    private List<ElementAnimation> finalPointScreenElements;
    private List<ElementAnimation> finalStatsElements;

    ElementAnimation questionTextHolderElement;
    ElementAnimation correctIndicatorElement;
    ElementAnimation wrongIndicatorElement;
    ElementAnimation pointsDisplayersHolderElement;

    private readonly TrainingSessionManager session = TrainingSessionManager.Instance;
    private static readonly System.Random random = new();

    private SalesTopic topic;
    private int currentModuleIndex;
    private int currentQuestionIndex;
    private float remainingTime;
    private bool isQuestionActive;
    private TMP_Text currentPointsDisplayer;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        // Pass SelectedTopic (on TopicMenuItem) to ModuleFlowController
        topic = session.SelectedTopic;

        if (topic == null)
        {
            Debug.LogError("No topic was selected.");
            return;
        }

        // On click of module start button (info screen) => Start module question sequence
        startQuestionsButton.onClick.AddListener(StartQuestionSequence);

        // On click of continue button (point screen) => Go to next module info screen
        continueButton.onClick.AddListener(ContinueToNextModule);

        // On click (just 1 time at the end) of finish button (final point screen) => Exit TrainigScene of current topic
        finishTopicButton.onClick.AddListener(ExitTopicTraining);

        // On click of answerButton[i] (question screen) => select an answer with a designated index
        for (int i = 0; i < answerButtons.Length; i++)
        {
            int capturedIndex = i;
            answerButtons[i].onClick.AddListener(() => SelectAnswer(capturedIndex));
        }

        // Deactivate all point displayers before starting
        foreach (TMP_Text displayer in pointsDisplayers)
        {
            displayer.gameObject.SetActive(false);
        }

        currentModuleIndex = 0;

        // Initialize ElementAnimation objects of each element per UI screen
        InitializeElementAnimationGroups();

        correctIndicatorElement.canvasGroup.alpha = 0f;
        wrongIndicatorElement.canvasGroup.alpha = 0f;
        pointsDisplayersHolderElement.canvasGroup.alpha = 0f;

        SetInfoScreen();
    }

    private void InitializeElementAnimationGroups()
    {
        // Info screen elements
        ElementAnimation infoTitleElement = animationController.GetElementAnimation(moduleTitle.gameObject);
        ElementAnimation infoTextBoxElement = animationController.GetElementAnimation(infoTextHolder);
        ElementAnimation startQuestionsButtonElement = animationController.GetElementAnimation(startQuestionsButton.gameObject);

        infoScreenElements = new() {infoTitleElement, infoTextBoxElement, startQuestionsButtonElement};

        // Questions screen elements
        ElementAnimation qCounterAndTimerElement = animationController.GetElementAnimation(qCounterAndTimer);
        questionTextHolderElement = animationController.GetElementAnimation(questionTextHolder);

        questionScreenElements = new() {qCounterAndTimerElement, questionTextHolderElement};
        
        foreach (Button currentbutton in answerButtons)
        {
            ElementAnimation currentButtonElement = animationController.GetElementAnimation(currentbutton.gameObject);
            questionScreenElements.Add(currentButtonElement);
        }

        correctIndicatorElement = animationController.GetElementAnimation(correctIndicator);
        wrongIndicatorElement = animationController.GetElementAnimation(wrongIndicator);
        pointsDisplayersHolderElement = animationController.GetElementAnimation(pointsDisplayersHolder);

        // Current points screen elements
        ElementAnimation currStatsTitleElement = animationController.GetElementAnimation(moduleTitleEnd.gameObject);
        ElementAnimation currStatsBackgroundElement = animationController.GetElementAnimation(currentStatsBackground);
        ElementAnimation continueButtonElement = animationController.GetElementAnimation(continueButton.gameObject);

        currentPointScreenElements = new() {currStatsTitleElement, currStatsBackgroundElement, continueButtonElement};

        // Current points screen stats elements
        ElementAnimation pointsTitleElement = animationController.GetElementAnimation(pointsTitle);
        ElementAnimation pointsTextElement = animationController.GetElementAnimation(pointsText.gameObject);
        ElementAnimation correctAnswersTitleElement = animationController.GetElementAnimation(correctAnswersTitle);
        ElementAnimation correctAnswersTextElement = animationController.GetElementAnimation(correctAnswersText.gameObject);
        ElementAnimation qRoundCompleteTextElement = animationController.GetElementAnimation(qRoundCompleteText);

        currentStatsElements = new() {pointsTitleElement, pointsTextElement, correctAnswersTitleElement, correctAnswersTextElement, qRoundCompleteTextElement};

        // Final points screen elements
        ElementAnimation finalStatsTitleElement = animationController.GetElementAnimation(topicCompletedText.gameObject);
        ElementAnimation finalStatsBackgroundElement = animationController.GetElementAnimation(finalStatsBackground);
        ElementAnimation finishTopicButtonElement = animationController.GetElementAnimation(finishTopicButton.gameObject);

        finalPointScreenElements = new() {finalStatsTitleElement, finalStatsBackgroundElement, finishTopicButtonElement};

        // Final points screen stats elements
        ElementAnimation finalPointsTitleElement = animationController.GetElementAnimation(finalPointsTitle);
        ElementAnimation finalPointsTextElement = animationController.GetElementAnimation(finalPointsText.gameObject);
        ElementAnimation finalCorrectAnswersTitleElement = animationController.GetElementAnimation(topicCorrectAnsTitle);
        ElementAnimation finalCorrectAnswersTextElement = animationController.GetElementAnimation(topicCorrectAnsText.gameObject);
        ElementAnimation finalAvgResponseTimeTitleElement = animationController.GetElementAnimation(topicAvgResponseTimeTitle);
        ElementAnimation finalAvgResponseTimeTextElement = animationController.GetElementAnimation(topicAvgResponseTimeText.gameObject);

        finalStatsElements = new() {finalPointsTitleElement, finalPointsTextElement, finalCorrectAnswersTitleElement, finalCorrectAnswersTextElement, finalAvgResponseTimeTitleElement, finalAvgResponseTimeTextElement};
    }

    // Update is called once per frame
    void Update()
    {
        if (!isQuestionActive)
        {
            return;
        }

        if (animationController.IsCurrentTransitionPhaseNone())
        {
            // Track timer and update timer text / bar
            remainingTime -= Time.unscaledDeltaTime;
            timerText.text = $"{Mathf.Max(remainingTime, 0f):0.0}s";

            timerBar.value = remainingTime;

            if (remainingTime <= 0f)
            {
                SelectAnswer(-1);
            }    
        }
    }

    private void SetInfoScreen()
    {
        if (topic == null || topic.modules == null)
        {
            return;
        }

        // If index outside module list range => Finish topic
        if (currentModuleIndex >= topic.modules.Count)
        {
            FinishTopic();
            return;
        }

        if (currentModuleIndex == 0)
        {
            // Animate scene start: start of Training Scene with info screen
            animationController.AnimateSceneStart(infoScreenElements);
        }
        
        // IF index in middle of module list range => Animate screen change: current point screen -> info screen
        if (currentModuleIndex > 0 && currentModuleIndex < topic.modules.Count)
        {
            // Restart infoTextBox element to original position for correct animation position and direction
            animationController.RestartElementToOriginalPosition(infoTextHolder);

            // Restart questionTextHolder element to original position for correct animation position and direction
            animationController.RestartElementToOriginalPosition(questionTextHolder);

            List<ElementAnimation> allCurrentPointScreenElements = animationController.CombinePointScreenElementsLists(currentPointScreenElements, currentStatsElements);

            animationController.AnimateScreenChange(allCurrentPointScreenElements, infoScreenElements, currentPointScreen, infoScreen);
        }

        MicroModule currentModule = topic.modules[currentModuleIndex];

        // Initiate currentModuleResult in TrainingSessionManager
        session.BeginModule(currentModule);

        // Set text attributes in info screen
        moduleTitle.text = currentModule.moduleTitle;
        infoText.text = currentModule.moduleText;
    }

    private void StartQuestionSequence()
    {
        currentQuestionIndex = 0;
        timerText.text = "0.0s";

        // Animate screen change: info screen -> question screen
        animationController.AnimateScreenChange(infoScreenElements, questionScreenElements, infoScreen, questionScreen);

        ShowCurrentQuestion();   
    }

    private void ShowCurrentQuestion()
    {
        // Deselect current answer button from last question
        eventSystem.SetSelectedGameObject(null);

        MicroModule currentModule = topic.modules[currentModuleIndex];

        // If index outside question list range => Finish module
        if (currentQuestionIndex >= currentModule.questions.Count)
        {
            FinishModule();
            return;
        }

        ModuleQuestion currentQuestion = currentModule.questions[currentQuestionIndex];

        // Set text attributes in question screen
        questionText.text = currentQuestion.questionText;
        questionCounterText.text = $"Frage {currentQuestionIndex + 1} / {currentModule.questions.Count}";

        // Animate question text appear
        questionTextHolderElement.textVisualFX.Play();

        // Set timer => For traditional training needs to be quicker -> half predefined timeLimitSeconds
        remainingTime = currentQuestion.timeLimitSeconds / 2;
        timerBar.maxValue = remainingTime;
        timerBar.value = remainingTime;

        // Activate buttons based on available answers
        for (int i = 0; i < answerButtons.Length; i++)
        {
            bool optionsExists = i < currentQuestion.answerOptions.Count;

            answerButtons[i].gameObject.SetActive(optionsExists);

            if (optionsExists)
            {
                answerButtonsText[i].text = currentQuestion.answerOptions[i];
            }
        }

        isQuestionActive = true;
    }

    private void SelectAnswer(int selectedAnswerIndex)
    {
        if (!isQuestionActive)
        {
            return;
        }

        MicroModule currentModule = topic.modules[currentModuleIndex];
        ModuleQuestion currentQuestion = currentModule.questions[currentQuestionIndex];

        bool isCorrect = selectedAnswerIndex >= 0 && selectedAnswerIndex == currentQuestion.correctAnswerIndex;
        bool timedOut = selectedAnswerIndex < 0;
        float responseTime = currentQuestion.timeLimitSeconds - remainingTime;
        int pointsAwarded = 0;

        // If answer correct => Calculate speed bonus points and add points to current module points
        if (isCorrect)
        {
            float speedRatio = remainingTime / currentQuestion.timeLimitSeconds;
            int speedBonus = Mathf.RoundToInt(currentQuestion.speedBonusPoints * Mathf.Clamp01(speedRatio));

            pointsAwarded = currentQuestion.basePoints + speedBonus;

            currentPointsDisplayer = pointsDisplayers[random.Next(0, pointsDisplayers.Length)];
            currentPointsDisplayer.text = $"+ {pointsAwarded}xp";
            
            // Animate correct answer and points awarded fade in random points displayer (predefined)
            animationController.AnimateCorrectIndicators(new() {correctIndicatorElement, pointsDisplayersHolderElement}, currentPointsDisplayer.gameObject);
        }
        else
        {
            // Animate wrong answer fade
            animationController.AnimateWrongIndicator(wrongIndicatorElement);
        }

        // Creat new question result and set its values based on answer
        QuestionResult currentQuestionResult = new()
        {
            moduleId = currentModule.moduleId,
            questionId = currentQuestion.questionId,
            selectedAnswerIndex = selectedAnswerIndex,
            wasCorrect = isCorrect,
            timedOut = timedOut,
            responseTime = responseTime,
            pointsAwarded = pointsAwarded,
            aIResponse = null
        };

        // Record question result in currentModuleResult from TrainingSessionManager
        session.RecordQuestionResult(currentQuestionResult);

        isQuestionActive = false;
        
        currentQuestionIndex++;
        ShowCurrentQuestion();
    }

    private void ShowPointScreen()
    {
        // Set title text attribute
        moduleTitleEnd.text = session.GetCurrentModuleResult().moduleTitle;

        // Animate screen change: question screen -> current point screen
        animationController.AnimateScreenChange(questionScreenElements, currentPointScreenElements, questionScreen, currentPointScreen);
            
        // Animate current stats display (one line of text at a time)
        animationController.AnimateModuleStatsDisplay(currentStatsElements, session.GetCurrentModuleResult());
    }

    private void ShowFinalPointScreen()
    {
        // Set title text attribute
        topicCompletedText.text = session.GetCurrentTopicResult().topicTitle;

        List<ElementAnimation> allCurrentPointScreenElements = animationController.CombinePointScreenElementsLists(currentPointScreenElements, currentStatsElements);

        // Animate screen change: current point screen (last) -> final point screen
        animationController.AnimateScreenChange(allCurrentPointScreenElements, finalPointScreenElements, currentPointScreen, finalPointScreen);
        
        // Animate final stats display (one line of text at a time)
        animationController.AnimateTopicStatsDisplay(finalStatsElements, session.GetCurrentTopicResult());
    }

    private void ContinueToNextModule()
    {
        currentModuleIndex++;
        SetInfoScreen();
    }

    private void CleanUp()
    {
        // Cleanup of button listeners
        startQuestionsButton.onClick.RemoveListener(StartQuestionSequence);
        continueButton.onClick.RemoveListener(ContinueToNextModule);
        finishTopicButton.onClick.RemoveListener(ContinueToNextModule);

        for (int i = 0; i < answerButtons.Length; i++)
        {
            int capturedIndex = i;
            answerButtons[i].onClick.RemoveListener(() => SelectAnswer(capturedIndex));
        }
    }

    private void FinishModule()
    {
        // Set currentModuleResult.completed to true in TrainingSessionManager
        session.CompleteCurrentModule();
        // Record currentModuleResult attributes (final) in currentTopicResult in TrainingSessionManager
        session.RecordCurrentModuleResult();

        if (currentModuleIndex > 0 && currentModuleIndex < topic.modules.Count)
        {
            // Restart current point stats elements to original position for correct animation position and direction
            animationController.RestartPointStatsElementsPosition(currentStatsElements);
        }
        
        // Go to current points screen (module)
        ShowPointScreen();
    }

    private void FinishTopic()
    {
        // Set currentTopicResult.completed to true in TrainingSessionManager
        session.CompleteCurrentTopic();

        // Go to final points screen (topic)
        ShowFinalPointScreen();
    }

    private void ExitTopicTraining()
    {
        CleanUp();

        List<ElementAnimation> allFinalPointScreenElements = animationController.CombinePointScreenElementsLists(finalPointScreenElements, finalStatsElements);

        // If all topics completed => End participant session, send respective values to local store and go to SessionResultsScene
        // Else => Record intermediate participant results (save and export), and return to TopicMenuScene for next topic
        if (session.AllTopicsCompleted())
        {
            session.EndParticipantSession();
            
            // Animate screen change: final point screen -> end of TrainingScene and load SessionResultsScene
            animationController.AnimateSceneEnd(allFinalPointScreenElements, "SessionResultsScene");
            Debug.Log("Loading SessionResultsScene.");
        }
        else
        {
            session.RecordIntermediateResults();

            // Animate screen change: final point screen -> end of TrainingScene and load TopicMenuScene
            animationController.AnimateSceneEnd(allFinalPointScreenElements, "TopicMenuScene");
            Debug.Log("Returning to TopicMenuScene.");
        }
        return;
    }
}
