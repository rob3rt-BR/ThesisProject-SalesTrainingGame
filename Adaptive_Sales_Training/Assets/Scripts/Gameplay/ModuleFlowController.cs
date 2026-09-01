using System.Linq;
using TMPro;
using UnityEngine;
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
    private GameObject pointScreen;
    [SerializeField]
    private GameObject finalPointScreen;

    [Header("Information UI")]
    [SerializeField]
    private TMP_Text moduleTitle;
    [SerializeField]
    private TMP_Text infoText;
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

    
    private readonly TrainingSessionManager session = TrainingSessionManager.Instance;

    private SalesTopic topic;
    private int currentModuleIndex;
    private int currentQuestionIndex;
    private float remainingTime;
    private bool isQuestionActive;

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

    // Update is called once per frame
    void Update()
    {
        if (!isQuestionActive)
        {
            return;
        }

        // Track timer and update timer text
        remainingTime -= Time.unscaledDeltaTime;
        timerText.text = $"Zeit: {Mathf.Max(remainingTime, 0f):0.0}";

        if (remainingTime <= 0f)
        {
            SelectAnswer(-1);
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

        MicroModule currentModule = topic.modules[currentModuleIndex];

        // Initiate currentModuleResult in TrainingSessionManager
        session.BeginModule(currentModule);

        // Set text attributes in info screen
        moduleTitle.text = currentModule.moduleTitle;
        infoText.text = currentModule.moduleText;

        pointScreen.SetActive(false);
        infoScreen.SetActive(true);
    }

    private void StartQuestionSequence()
    {
        currentQuestionIndex = 0;

        infoScreen.SetActive(false);
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

        isQuestionActive = false;

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
            pointsAwarded = pointsAwarded,
            aIResponse = null
        };

        // Record question result in currentModuleResult from TrainingSessionManager
        session.RecordQuestionResult(currentQuestionResult);
        
        currentQuestionIndex++;
        ShowCurrentQuestion();
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

        // If last module => Go to final points screen (topic), Else => Go to current points screen (module)
        if (currentModuleIndex == topic.modules.Count - 1)
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
            session.EndParticipantSession();
            SceneManager.LoadScene("SessionResultsScene");
            Debug.Log("Loading SessionResultsScene.");
        } else
        {
            SceneManager.LoadScene("TopicMenuScene");
            Debug.Log("Returning to TopicMenuScene.");
        }
    }
}
