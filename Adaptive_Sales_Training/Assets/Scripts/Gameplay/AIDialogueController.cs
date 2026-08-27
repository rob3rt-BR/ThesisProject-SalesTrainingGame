using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
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
    private GameObject pointScreen;
    [SerializeField]
    private GameObject finalPointScreen;

    [Header("Information UI")]
    [SerializeField]
    private TMP_Text moduleTitle;
    [SerializeField]
    private TMP_Text infoText;
    [SerializeField]
    private Button startDialogueButton;

    [Header("Dialogue UI")]
    [SerializeField]
    private TMP_Text dialogueTitle;
    [SerializeField]
    private TMP_Text dialogueText;
    [SerializeField]
    private GameObject loadingIndicator;
    [SerializeField]
    private Button startQuestionsButton;

    [Header("Question UI")]
    [SerializeField]
    private TMP_Text questionText;
    [SerializeField]
    private TMP_Text questionCounterText;
    [SerializeField]
    private TMP_Text timerText;
    [SerializeField]
    private Button[] answerButtons;
    [SerializeField]
    private TMP_Text[] answerButtonsText;
    [SerializeField]
    private Button nextQuestionButton;

    [Header("Points UI")]
    [SerializeField]
    private TMP_Text moduleTitleEnd;
    [SerializeField]
    private TMP_Text pointsText;
    [SerializeField]
    private TMP_Text correctAnswersText;
    [SerializeField]
    private Button continueButton;

    [Header("Final Points UI")]
    [SerializeField]
    private TMP_Text topicCompletedText;
    [SerializeField]
    private TMP_Text finalPointsText;
    [SerializeField]
    private TMP_Text topicCorrectAnsText;
    [SerializeField]
    private TMP_Text topicAvgResponseTimeText;
    [SerializeField]
    private Button finishTopicButton;

    [Header("Backend")]
    [SerializeField]
    private string backendUrl = "http://localhost:5065/api/dialogue";

    private readonly TrainingSessionManager session = TrainingSessionManager.Instance;

    private SalesTopic topic;
    private int currentModuleIndex;
    private int currentQuestionIndex;
    private float remainingTime;
    private bool isQuestionActive;
    private bool isScenarioModule;

    private List<AIDialogueScenario> dialogueScenarios;
    private List<DialogueMessage> currentConversationHistory;
    private bool requestInProgress;

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

        // On click (just 1 time at the end) of finish button (final point screen) => Go to SetInfoScreen() edge case => FinishTopic()
        finishTopicButton.onClick.AddListener(ContinueToNextModule);

        // On click of answerButton[i] (question screen) => select an answer with a designated index
        for (int i = 0; i < answerButtons.Length; i++)
        {
            int capturedIndex = i;
            answerButtons[i].onClick.AddListener(() => SelectAnswer(capturedIndex));
        }

        currentModuleIndex = 0;

        SetInfoScreen();
    }

    void Update()
    {
        if (!isQuestionActive)
        {
            return;
        }

        // If any module except scenario module (last) => track timer and update timer text, Else => disable timer
        if (!isScenarioModule)
        {
            remainingTime -= Time.unscaledDeltaTime;
            timerText.text = $"Zeit: {Mathf.Max(remainingTime, 0f):0.0}";

            if (remainingTime <= 0f)
            {
                SelectAnswer(-1);
            }
        } else
        {
            timerText.gameObject.SetActive(false);
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

        // If index == last module in list => Now in scenario module
        if (currentModuleIndex == topic.modules.Count - 1)
        {
            isScenarioModule = true;
        }

        MicroModule currentModule = topic.modules[currentModuleIndex];

        // Initiate currentModuleResult in TrainingSessionManager
        session.BeginModule(currentModule);

        // Set text attributes in info screen
        moduleTitle.text = currentModule.moduleTitle;
        infoText.text = currentModule.moduleText;

        pointScreen.SetActive(false);
        infoScreen.SetActive(true);
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

        // Set text attributes in dialogue screen => display opening message in textbox
        dialogueTitle.text = currentDialogueScenario.module.moduleTitle;
        dialogueText.text = currentDialogueScenario.openingMessage;
        
        infoScreen.SetActive(false);
        dialogueScreen.SetActive(true);

        // For NON FIRST module start => enable start question button again
        startQuestionsButton.gameObject.SetActive(true);
    }

    private void StartQuestionSequence()
    {
        currentQuestionIndex = 0;

        // For FIRST module start => disable start question button to NOT interfere later with next question button
        startQuestionsButton.gameObject.SetActive(false);

        dialogueScreen.SetActive(false);
        questionScreen.SetActive(true);

        ShowCurrentQuestion();
    }

    private void ShowCurrentQuestion()
    {
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

        // Set timer
        remainingTime = currentQuestion.timeLimitSeconds;
        isQuestionActive = true;

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
    }

    private void SelectAnswer(int selectedAnswerIndex)
    {
        if (!isQuestionActive)
        {
            return;
        }

        MicroModule currentModule = topic.modules[currentModuleIndex];
        ModuleQuestion currentQuestion = currentModule.questions[currentQuestionIndex];

        float responseTime = currentQuestion.timeLimitSeconds - remainingTime;
        bool isCorrect = selectedAnswerIndex >= 0 && selectedAnswerIndex == currentQuestion.correctAnswerIndex;
        bool timedOut = selectedAnswerIndex < 0;
        int pointsAwarded = 0;

        // If answer correct => Calculate speed bonus points and add points to current module points
        if (isCorrect)
        {
            float speedRatio = remainingTime / currentQuestion.timeLimitSeconds;
            int speedBonus = Mathf.RoundToInt(currentQuestion.speedBonusPoints * Mathf.Clamp01(speedRatio));

            pointsAwarded = currentQuestion.basePoints + speedBonus;
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

        string selectedAnswer = question.answerOptions[selectedAnswerIndex];

        // Disable answer buttons interactable property
        SetAnswerButtonsInteractable(false);

        // Start coroutine of requestinf AI dialogue response based on current module, question, and selected answer data
        StartCoroutine(RequestDialogueResponse(module, question, selectedAnswer));
    }

    private IEnumerator RequestDialogueResponse(MicroModule module, ModuleQuestion question, string selectedAnswer)
    {
        questionScreen.SetActive(false);

        requestInProgress = true;
        loadingIndicator.SetActive(true);

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
        loadingIndicator.SetActive(false);

        // If web request result NOT successful => Failed and break coroutine
        if (webRequest.result != UnityWebRequest.Result.Success)
        {
            Debug.LogError($"Dialogue request failed: {webRequest.error}");
            dialogueText.text = "Dialogue currently unavailable.";
            
            SetAnswerButtonsInteractable(true);
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
            content = $"Current question dialogue context: {currentQuestionContext.dialogueContext}\n" +
                        $"Question: {question.questionText}\n" +
                        $"Selected answer: {selectedAnswer}"
        };
        currentConversationHistory.Add(currentUserAnswerMessage);

        // Create AI response to user answer
        DialogueMessage currentAIResponseMessage = new()
        {
            role = "assistant",
            content = responseData.replyText
        };
        currentConversationHistory.Add(currentAIResponseMessage);

        // Set text attribute in dialogue screen => display response message in textbox
        dialogueText.text = responseData.replyText;

        dialogueScreen.SetActive(true);

        // Enable next question button to continue
        nextQuestionButton.gameObject.SetActive(true);
    }

    private void ShowPointScreen()
    {
        MicroModule currentModule = topic.modules[currentModuleIndex];

        int currentModulePoints = session.GetCurrentModulePoints();
        int currentModuleCorrectAnswers = session.GetCurrentModuleCorrectAnswers();

        // Set text attributes in points screen
        moduleTitleEnd.text = currentModule.moduleTitle;
        pointsText.text = currentModulePoints.ToString();
        correctAnswersText.text = $"{currentModuleCorrectAnswers} / {currentModule.questions.Count}";

        questionScreen.SetActive(false);
        pointScreen.SetActive(true);
    }

    private void ShowFinalPointScreen()
    {
        int topicTotalPoints = session.GetCurrentTopicPoints();
        int topicCorrectAnswers = session.GetCurrentTopicCorrectAnswers();
        int topicTotalQuestions = topic.modules.Sum(m => m.questions.Count);
        float topicAvgResponseTime = session.GetCurrentTopicAvgResponseTime();

        // Set text attributes on final points screen (topic)
        topicCompletedText.text = topic.topicTitle + " geschafft!";
        finalPointsText.text = topicTotalPoints.ToString();
        topicCorrectAnsText.text = $"{topicCorrectAnswers} / {topicTotalQuestions}";
        topicAvgResponseTimeText.text = topicAvgResponseTime.ToString("0.0") + "s";

        questionScreen.SetActive(false);
        finalPointScreen.SetActive(true);
    }


    private void ContinueToNextQuestion()
    {
        currentQuestionIndex++;
        ShowCurrentQuestion();
    }

    private void ContinueToNextModule()
    {
        // Disable next question button to NOT intervene with start question button at the beginning of next module
        nextQuestionButton.gameObject.SetActive(false);

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

        // If scenario module (last) => Go to final points screen (topic), Else => Go to current points screen (module)
        if (isScenarioModule)
        {
            ShowFinalPointScreen();
        } else
        {
            ShowPointScreen();
        }
    }
    private void FinishTopic()
    {
        CleanUp();

        // Set currentTopicResult.completed to true in TrainingSessionManager
        session.CompleteCurrentTopic();

        // If all topics completed => End session, send respective values to local store and go to SessionResultsScene
        // Else => return to TopicMenuScene for next topic
        if (session.AllTopicsCompleted())
        {
            session.EndSession();
            SceneManager.LoadScene("SessionResultsScene");
            Debug.Log("Loading SessionResultsScene.");
        } else
        {
            SceneManager.LoadScene("TopicMenuScene");
            Debug.Log("Returning to TopicMenuScene.");
        }
    }

    private void SetAnswerButtonsInteractable(bool value)
    {
        foreach (Button button in answerButtons)
        {
            button.interactable = value;
        }
    }
}
