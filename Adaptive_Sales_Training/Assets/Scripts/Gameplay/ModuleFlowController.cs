using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class ModuleFlowController : MonoBehaviour
{
    private SalesTopic topic;

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
    private TMP_Text pointsText;
    [SerializeField]
    private TMP_Text pointsDetailsText;
    [SerializeField]
    private Button continueButton;

    [Header("Final Points UI")]
    [SerializeField]
    private TMP_Text finalPointsText;
    [SerializeField]
    private TMP_Text topicCompletedText;
    [SerializeField]
    private Button finishTopicButton;
    
    private int currentModuleIndex;
    private int currentQuestionIndex;
    private float remainingTime;
    private bool isQuestionActive;
    private bool isScenarioModule;

    private int totalTopicPoints;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        topic = TrainingSessionManager.Instance.SelectedTopic;

        if (topic == null)
        {
            Debug.LogError("No topic was selected.");
            return;
        }

        // On click of module start button (info screen) => Start module question sequence
        startQuestionsButton.onClick.AddListener(StartQuestionSequence);
        // On click of continue button (point screen) => Go to next module info screen
        continueButton.onClick.AddListener(ContinueToNextModule);
        // On click (just 1 time at the end) of finish button (final point screen)=> Go to SetInfoScreen() edge case => FinishTopic()
        finishTopicButton.onClick.AddListener(ContinueToNextModule);

        for (int i = 0; i < answerButtons.Length; i++)
        {
            int capturedIndex = i;
            answerButtons[i].onClick.AddListener(() => SelectAnswer(capturedIndex));
        }

        currentModuleIndex = 0;
        totalTopicPoints = 0;

        SetInfoScreen();
    }

    // Update is called once per frame
    void Update()
    {
        if (!isQuestionActive)
        {
            return;
        }

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

        // If index outside module list range => Finish
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
        TrainingSessionManager.Instance.BeginModule(currentModule);

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

        if (currentQuestionIndex >= currentModule.questions.Count)
        {
            if (isScenarioModule)
            {
                ShowFinalPointScreen();
            } else
            {
                ShowPointScreen();
            }
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
            totalTopicPoints += pointsAwarded;
        }

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

        TrainingSessionManager.Instance.RecordQuestionResult(currentQuestionResult);

        currentQuestionIndex++;
        ShowCurrentQuestion();
    }

    private void ShowPointScreen()
    {
        TrainingSessionManager.Instance.CompleteCurrentModule();

        isQuestionActive = false; // TODO: Testear con o sin esta asignacion para ver si es necesaria

        MicroModule currentModule = topic.modules[currentModuleIndex];
        int currentModulePoints = TrainingSessionManager.Instance.GetCurrentModulePoints();

        pointsText.text = $"Punkte:\n{currentModulePoints}";
        pointsDetailsText.text = $"{currentModule.moduleTitle}\nFragerunde geschafft!";

        questionScreen.SetActive(false);
        pointScreen.SetActive(true);
    }

    private void ShowFinalPointScreen()
    {
        TrainingSessionManager.Instance.SetCurrentTopicTotalPoints(totalTopicPoints);

        finalPointsText.text = $"{totalTopicPoints}";
        topicCompletedText.text = $"Thema \"{topic.topicTitle}\" geschafft!";

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
        startQuestionsButton.onClick.RemoveListener(StartQuestionSequence);
        continueButton.onClick.RemoveListener(ContinueToNextModule);
        finishTopicButton.onClick.RemoveListener(ContinueToNextModule);

        for (int i = 0; i < answerButtons.Length; i++)
        {
            int capturedIndex = i;
            answerButtons[i].onClick.RemoveListener(() => SelectAnswer(capturedIndex));
        }
    }

    private void FinishTopic()
    {
        CleanUp();

        TrainingSessionManager session = TrainingSessionManager.Instance;
        session.CompleteSelectedTopic();

        if (session.AllTopicsCompleted())
        {
            session.EndSession();
            // SceneManager.LoadScene(TrainingResultsScene);
        } else
        {
            SceneManager.LoadScene("TopicMenuScene");
        }
    }
}
