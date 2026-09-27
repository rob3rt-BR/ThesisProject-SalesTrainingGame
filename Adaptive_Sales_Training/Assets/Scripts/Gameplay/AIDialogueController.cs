using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Networking;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class AIDialogueController : MonoBehaviour
{
    [Header("Screens")]
    [SerializeField]
    private GameObject infoScreen;
    [SerializeField]
    private GameObject dialogueScreen;
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
    private Button startDialogueButton;

    [Header("Dialogue UI")]
    [SerializeField]
    private TMP_Text dialogueTitle;
    [SerializeField]
    private GameObject dialogueBoxBackground;
    [SerializeField]
    private TMP_Text traineeDialogueText;
    [SerializeField]
    private TMP_Text aiResponseText;
    [SerializeField]
    private GameObject loadingIndicator;
    [SerializeField]
    private Button startQuestionsButton;
    [SerializeField]
    private Button nextQuestionButton;

    [Header("Question UI")]
    [SerializeField]
    private GameObject qCounterAndTimer;
    [SerializeField]
    private TMP_Text questionCounterText;
    [SerializeField]
    private GameObject questionTextHolder;
    [SerializeField]
    private TMP_Text questionText;
    [SerializeField]
    private TMP_Text timerText;
    [SerializeField]
    private Button[] answerButtons;
    [SerializeField]
    private TMP_Text[] answerButtonsText;
    [SerializeField]
    private GameObject correctIndicator;
    [SerializeField]
    private GameObject wrongIndicator;

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

    [Header("Backend")]
    [SerializeField]
    private string backendUrl = "http://localhost:5065/api/dialogue";
    
    [Header("Animation Controller")]
    [SerializeField]
    private AnimationController animationController;

    [Header("EventSystem")]
    [SerializeField]
    private EventSystem eventSystem;

    private List<ElementAnimation> infoScreenElements;
    private List<ElementAnimation> dialogueScreenElementsPre;
    private List<ElementAnimation> dialogueScreenElementsPost;
    private List<ElementAnimation> questionScreenElements;
    private List<ElementAnimation> currentPointScreenElements;
    private List<ElementAnimation> currentStatsElements;
    private List<ElementAnimation> finalPointScreenElements;
    private List<ElementAnimation> finalStatsElements;

    ElementAnimation traineeDialogueTextElement;
    ElementAnimation aiResponseTextElement;
    ElementAnimation loadingIndicatorElement;

    private readonly TrainingSessionManager session = TrainingSessionManager.Instance;

    private SalesTopic topic;
    private int currentModuleIndex;
    private int currentQuestionIndex;
    private float remainingTime;
    private bool isQuestionActive;

    private List<AIDialogueScenario> dialogueScenarios;
    private List<DialogueMessage> currentConversationHistory = new();
    private bool requestInProgress;
    private static readonly WaitForSeconds waitForSeconds = new(3f);

    void Start()
    {
        // Pass SelectedTopic (on TopicMenuItem) to AIDialogueController
        topic = session.SelectedTopic;

        if (topic == null)
        {
            Debug.LogError("No topic was selected.");
            return;
        }

        // Pass TopicDialogueScenarios (on TopicMenuItem) to AIDialogueController
        dialogueScenarios = session.TopicDialogueScenarios;

        if (dialogueScenarios == null)
        {
            Debug.LogError("No AI dialogue scenarios available for modules of current topic.");
            return;
        }

        // On click of module start button (info screen) => Show dialogue screen
        startDialogueButton.onClick.AddListener(StartDialogue);

        // On click of start dialogue question button (dialogue screen) => Show question screen and start first question
        startQuestionsButton.onClick.AddListener(StartQuestionSequence);

        // On click of next question button (question screen) => Go to next question (Instead of fast sequence like in normal version)
        nextQuestionButton.onClick.AddListener(ContinueToNextQuestion);

        // On click of continue button (point screen) => Go to next module info screen
        continueButton.onClick.AddListener(ContinueToNextModule);

        // On click (just 1 time at the end) of finish button (final point screen) => Exit AITrainigScene of current topic
        finishTopicButton.onClick.AddListener(ExitTopicTraining);

        // On click of answerButton[i] (question screen) => select an answer with a designated index
        for (int i = 0; i < answerButtons.Length; i++)
        {
            int capturedIndex = i;
            answerButtons[i].onClick.AddListener(() => SelectAnswer(capturedIndex));
        }

        currentModuleIndex = 0;

        // Initialize ElementAnimation objects of each element per UI screen
        InitializeElementAnimationGroups();

        SetInfoScreen();
    }

    private void InitializeElementAnimationGroups()
    {
        // Info screen elements
        ElementAnimation infoTitleElement = animationController.GetElementAnimation(moduleTitle.gameObject);
        ElementAnimation infoTextBoxElement = animationController.GetElementAnimation(infoTextHolder);
        ElementAnimation startDialogueButtonElement = animationController.GetElementAnimation(startDialogueButton.gameObject);

        infoScreenElements = new() {infoTitleElement, infoTextBoxElement, startDialogueButtonElement};

        // Dialogue screen elements 
        ElementAnimation dialogueTitleElement = animationController.GetElementAnimation(dialogueTitle.gameObject);
        ElementAnimation dialogueBoxBackgroundElement = animationController.GetElementAnimation(dialogueBoxBackground);
        traineeDialogueTextElement = animationController.GetElementAnimation(traineeDialogueText.gameObject);
        aiResponseTextElement = animationController.GetElementAnimation(aiResponseText.gameObject);
        loadingIndicatorElement = animationController.GetElementAnimation(loadingIndicator);
        ElementAnimation startQuestionsButtonElement = animationController.GetElementAnimation(startQuestionsButton.gameObject);
        ElementAnimation nextQuestionButtonElement = animationController.GetElementAnimation(nextQuestionButton.gameObject);

        // Dialogue screen elements PRE questions list
        dialogueScreenElementsPre = new() {dialogueTitleElement, dialogueBoxBackgroundElement, traineeDialogueTextElement, aiResponseTextElement, startQuestionsButtonElement};
        
        // Dialogue screen elements POST questions list
        dialogueScreenElementsPost = new() {dialogueTitleElement, dialogueBoxBackgroundElement, traineeDialogueTextElement, aiResponseTextElement, nextQuestionButtonElement};

        // Questions screen elements
        ElementAnimation qCounterAndTimerElement = animationController.GetElementAnimation(qCounterAndTimer);
        ElementAnimation questionTextHolderElement = animationController.GetElementAnimation(questionTextHolder);

        questionScreenElements = new() {qCounterAndTimerElement, questionTextHolderElement};
        
        foreach (Button currentbutton in answerButtons)
        {
            ElementAnimation currentButtonElement = animationController.GetElementAnimation(currentbutton.gameObject);
            questionScreenElements.Add(currentButtonElement);
        }

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

    void Update()
    {
        if (!isQuestionActive)
        {
            return;
        }

        if (animationController.IsCurrentTransitionPhaseNone())
        {
            // Track timer and update timer text
            remainingTime -= Time.unscaledDeltaTime;
            timerText.text = $"Zeit: {Mathf.Max(remainingTime, 0f):0.0}";

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

            // Remove dialogue text boxes from dialogueScreenElementsPre before animating opening dialogue text display
            // and restart elements to original position for correct animation position and direction
            //dialogueScreenElementsPre.Remove(traineeDialogueTextElement);
            //dialogueScreenElementsPre.Remove(aiResponseTextElement);
            //animationController.RestartElementToOriginalPosition(traineeDialogueText.gameObject);
            //animationController.RestartElementToOriginalPosition(aiResponseText.gameObject);

            List<ElementAnimation> allCurrentPointScreenElements = animationController.CombinePointScreenElementsLists(currentPointScreenElements, currentStatsElements);

            animationController.AnimateScreenChange(allCurrentPointScreenElements, infoScreenElements, currentPointScreen, infoScreen);
        }

        MicroModule currentModule = topic.modules[currentModuleIndex];

        // Initiate currentModuleResult in TrainingSessionManager
        session.BeginModule(currentModule);

        // Set text attributes in info screen
        moduleTitle.text = currentModule.moduleTitle;
        infoText.text = currentModule.moduleText;

        // Deactivate dialogue texts to prepare for later animation
        traineeDialogueText.gameObject.SetActive(false);
        aiResponseText.gameObject.SetActive(false);
    }
    public void StartDialogue()
    {
        AIDialogueScenario currentDialogueScenario = dialogueScenarios[currentModuleIndex];
        if (currentDialogueScenario == null)
        {
            Debug.LogError("No AI dialogue scenario available for current module.");
            return;
        }

        // Clear conversation history before starting dialogue scenario of current module
        currentConversationHistory.Clear();

        // Create start message of conversation history (AI) =>
        DialogueMessage startMessage = new()
        {
            role = "assistant",
            content = currentDialogueScenario.openingMessage
        };
        currentConversationHistory.Add(startMessage);

        // Set text attributes in dialogue screen => display current scenario in traineeDialogue textbox and opening message in aiResponse textbox
        dialogueTitle.text = currentDialogueScenario.module.moduleTitle;
        
        traineeDialogueText.text = TrimScenarioContext(currentDialogueScenario.scenarioContext);
        aiResponseText.text = currentDialogueScenario.openingMessage;

        loadingIndicatorElement.canvasGroup.alpha = 0f;

        // Animate screen change: info screen -> dialogue screen (PRE questions)
        animationController.AnimateScreenChange(infoScreenElements, dialogueScreenElementsPre, infoScreen, dialogueScreen);

        // Animate opening dialogue texts display
        animationController.AnimateDialogueTextsDisplay(new() {traineeDialogueTextElement, aiResponseTextElement}, true);
    }

    private string TrimScenarioContext(string scenarioContext)
    {
        string trimedScenarioContext = scenarioContext.Replace(" Das Gespräch", "$");
        int symbolIndex = trimedScenarioContext.IndexOf('$');
        trimedScenarioContext = trimedScenarioContext[..symbolIndex];

        return trimedScenarioContext;
    }

    private void StartQuestionSequence()
    {
        currentQuestionIndex = 0;
        timerText.text = "Zeit: 0.0";
 
        // Add dialogue text boxes to dialogueScreenElementsPre before animating screen change to question screen
        //dialogueScreenElementsPre.Add(traineeDialogueTextElement);
        //dialogueScreenElementsPre.Add(aiResponseTextElement);

        // Animate screen change: dialogue screen (PRE questions) -> question screen
        animationController.AnimateScreenChange(dialogueScreenElementsPre, questionScreenElements, dialogueScreen, questionScreen);

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

        // IF index in middle of question list range => Animate screen change: dialogue screen (POST question) -> question screen
        if (currentQuestionIndex > 0 && currentQuestionIndex < currentModule.questions.Count)
        {
            // Restart questionTextHolder element to original position for correct animation position and direction
            animationController.RestartElementToOriginalPosition(questionTextHolder);

            animationController.AnimateScreenChange(dialogueScreenElementsPost, questionScreenElements, dialogueScreen, questionScreen);
        }


        ModuleQuestion currentQuestion = currentModule.questions[currentQuestionIndex];

        // Set text attributes in question screen
        questionText.text = currentQuestion.questionText;
        questionCounterText.text = $"Frage {currentQuestionIndex + 1} / {currentModule.questions.Count}";

        // Set timer
        remainingTime = currentQuestion.timeLimitSeconds;

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
            animationController.AnimateAnswerIndicator(correctIndicator);

            float speedRatio = remainingTime / currentQuestion.timeLimitSeconds;
            int speedBonus = Mathf.RoundToInt(currentQuestion.speedBonusPoints * Mathf.Clamp01(speedRatio));

            pointsAwarded = currentQuestion.basePoints + speedBonus;
        }
        else
        {
            animationController.AnimateAnswerIndicator(wrongIndicator);
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
            pointsAwarded = pointsAwarded
        };

        // Record question result in currentModuleResult from TrainingSessionManager
        session.RecordQuestionResult(currentQuestionResult);
        
        isQuestionActive = false;

        // Set text attribute in dialogue screen => display trainee dialogue in textbox
        AIDialogueScenario currentDialogueScenario = dialogueScenarios[currentModuleIndex];
        AIQuestionContext currentQuestionContext = currentDialogueScenario.questionContexts[currentQuestionIndex];

        if (timedOut)
        {
            selectedAnswerIndex = currentQuestion.correctAnswerIndex switch
            {
                0 => 1,
                _ => currentQuestion.correctAnswerIndex - 1,
            };
        }

        traineeDialogueText.text = currentQuestionContext.traineeDialogueTexts[selectedAnswerIndex];
        aiResponseText.text = "";

        // Restart loadingIndicator element to original position for correct animation position and direction
        animationController.RestartElementToOriginalPosition(loadingIndicator);

        // Add loadingIndicator element again before transitioning back to dialogue screen
        dialogueScreenElementsPost.Add(loadingIndicatorElement);

        // Animate screen change: question screen -> dialogue screen (POST questions)
        animationController.AnimateScreenChange(questionScreenElements, dialogueScreenElementsPost, questionScreen, dialogueScreen);
        
        // Submit answer to AI and start coroutine of requesting AI response
        SubmitAnswerToAI(currentModule, currentQuestion, selectedAnswerIndex);
    }

    private void SubmitAnswerToAI(MicroModule module, ModuleQuestion question, int selectedAnswerIndex)
    {
        if (requestInProgress)
        {
            return;
        }

        if (selectedAnswerIndex < 0 || selectedAnswerIndex >= question.answerOptions.Count)
        {
            Debug.LogError("Invalid answer index.");
            return;
        }

        // Start coroutine of requestinf AI dialogue response based on current module, question, and selected answer data
        StartCoroutine(RequestDialogueResponse(module, question, selectedAnswerIndex));
    }

    private IEnumerator RequestDialogueResponse(MicroModule module, ModuleQuestion question, int selectedAnswerIndex)
    {
        requestInProgress = true;

        AIDialogueScenario currentDialogueScenario = dialogueScenarios[currentModuleIndex];

        if (currentDialogueScenario == null)
        {
            requestInProgress = false;
            loadingIndicator.SetActive(false);
            Debug.LogError("No AI dialogue scenario available for current module.");
            yield break;
        }

        AIQuestionContext currentQuestionContext = currentDialogueScenario.questionContexts[currentQuestionIndex];

        if (currentQuestionContext == null)
        {
            requestInProgress = false;
            loadingIndicator.SetActive(false);
            Debug.LogError("No question context available for current AI dialogue scenario.");
            yield break;
        }

        string selectedAnswer = question.answerOptions[selectedAnswerIndex];
        string currentTraineeDialogue = currentQuestionContext.traineeDialogueTexts[selectedAnswerIndex];

        // Create new dialogue request data based on passed parameters data (module, question, selected answer)
        DialogueRequest requestData = new()
        {
            sessionId = session.SessionId,
            scenarioId = currentDialogueScenario.scenarioId,
            moduleId = module.moduleId,
            questionId = question.questionId,

            scenarioContext = currentDialogueScenario.scenarioContext,
            customerPersona = currentDialogueScenario.customerPersona,
            questionDialogueContext = currentQuestionContext.dialogueContext,
            
            questionText = question.questionText,
            selectedAnswer = selectedAnswer,

            traineeDialogueText = currentTraineeDialogue,

            conversationHistory = new(currentConversationHistory)
        };

        // Create JSON format request of dialogue request data
        string jsonRequest = JsonUtility.ToJson(requestData);
        // Encode request body of JSON request
        byte[] requestBody = System.Text.Encoding.UTF8.GetBytes(jsonRequest);

        // Create new UnityWebRequest to make a communication with backend server and create handlers
        UnityWebRequest webRequest = new(backendUrl, UnityWebRequest.kHttpVerbPOST)
        {
            uploadHandler = new UploadHandlerRaw(requestBody),
            downloadHandler = new DownloadHandlerBuffer()
        };
        // Set web request headers
        webRequest.SetRequestHeader("Content-Type", "application/json");

        // Start tracking total AI response time
        var stopWatch = System.Diagnostics.Stopwatch.StartNew();

        // Begin communication with backend server
        yield return webRequest.SendWebRequest();

        // Stop tracking total AI response time
        stopWatch.Stop();
        long totalResponseTimeMilliseconds = stopWatch.ElapsedMilliseconds;

        requestInProgress = false;

        // If web request result NOT successful => Failed and break coroutine
        if (webRequest.result != UnityWebRequest.Result.Success)
        {
            Debug.LogError($"Dialogue request failed: {webRequest.error}");
            aiResponseText.text = "Dialogue currently unavailable.";
            yield break;
        }

        // If web request result successful => create new dialogue response data based on result
        DialogueResponse responseData = JsonUtility.FromJson<DialogueResponse>(webRequest.downloadHandler.text);

        // Log AI response metrics
        Debug.Log($"AI response received:\n" +
                $"Response ID: {responseData.responseId}\n" + 
                $"Model: {responseData.model}" +
                $"Prompt Tokens: {responseData.promptTokens}\n" +
                $"Completion Tokens: {responseData.completionTokens}\n" +
                $"Backend Response Time: {responseData.responseTimeMilliseconds}\n" +
                $"Total Response Time: {totalResponseTimeMilliseconds}");

        // Record current AI response values to previously recorded question
        AIResponseLog aIResponse = new()
        {
            responseId = responseData.responseId,
            model = responseData.model,
            totalResponseTimeMilliseconds = totalResponseTimeMilliseconds,
            
            replyText = responseData.replyText
        };
        session.RecordQuestionResultAIResponse(aIResponse, question.questionId);

        // Create user answer message to current question
        DialogueMessage currentUserAnswerMessage = new()
        {
            role = "user",
            content = requestData.traineeDialogueText
        };
        currentConversationHistory.Add(currentUserAnswerMessage);

        // Create AI response to user answer
        DialogueMessage currentAIResponseMessage = new()
        {
            role = "assistant",
            content = responseData.replyText
        };
        currentConversationHistory.Add(currentAIResponseMessage);

        // Set text attribute in dialogue screen => display trainee dialogue and AI response message in textbox
        aiResponseText.text = responseData.replyText;

        yield return waitForSeconds;

        // Animate loadingIndicator fadeout
        animationController.AnimateLoadingIndicatorLeave(loadingIndicator);

        // Remove loadingIndicatorElement before transitioning back to question screen => NOT animate loadingIndicatorElement
        dialogueScreenElementsPost.Remove(loadingIndicatorElement);
    }

    private void ShowPointScreen()
    {
        // Set title text attribute
        moduleTitleEnd.text = session.GetCurrentModuleResult().moduleTitle;

        // Animate screen change: dialogue screen (POST questions) -> current point screen
        animationController.AnimateScreenChange(dialogueScreenElementsPost, currentPointScreenElements, dialogueScreen, currentPointScreen);
            
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


    private void ContinueToNextQuestion()
    {   
        currentQuestionIndex++;
        ShowCurrentQuestion();
    }

    private void ContinueToNextModule()
    {
        currentModuleIndex++;
        SetInfoScreen();
    }

    private void CleanUp()
    {
        // Cleanup of button listeners
        startDialogueButton.onClick.RemoveListener(StartDialogue);
        startQuestionsButton.onClick.RemoveListener(StartQuestionSequence);
        nextQuestionButton.onClick.RemoveListener(ContinueToNextQuestion);
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
