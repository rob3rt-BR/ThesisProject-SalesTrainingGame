using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

public class TrainingSessionManager : MonoBehaviour
{
    public static TrainingSessionManager Instance { get; private set; }

    public string ParticipantId {get; private set;}
    public string SessionId {get; private set;}
    public TrainingVersion Version {get; private set;}
    
    public string SessionStartCEST {get; private set;} // CEST (Central European Summer Time, UTC+2)

    public List<TopicResult> TopicResults {get; private set;} = new();
    public SalesTopic SelectedTopic {get; private set;}
    
    private List<SalesTopic> allTopics;
    private TopicResult currentTopicResult;
    private ModuleResult currentModuleResult;

    private int totalPoints;
    private int totalCorrectAnswers;
    private float finalAvgResponseTime;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            Debug.Log("TrainingSessionManager destroyed.");
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);
        Debug.Log("TrainingSessionManager created.");
    }

    #region Session Functions
    public void StartSession(string participantId, TrainingVersion version, List<SalesTopic> requiredTopics)
    {
        allTopics = requiredTopics;

        ParticipantId = participantId;
        Version = version;
        SessionId = Guid.NewGuid().ToString(); // TODO: Check alternatives para SessionId

        SessionStartCEST = DateTime.UtcNow.ToString("O"); // TODO: Check funcionalidad

        // Reset important manager information trackers
        TopicResults.Clear();
        SelectedTopic = null;
        currentTopicResult = null;
        currentModuleResult = null;

        totalPoints = 0;
        totalCorrectAnswers = 0;
        finalAvgResponseTime = 0f;
    }

    public void EndSession()
    {
        if (!AllTopicsCompleted())
        {
            Debug.LogWarning("Session cannot end - not all topics completed.");
            return;
        }
        totalPoints = GetTotalPoints();
        totalCorrectAnswers = GetTotalCorrectAnswers();
        finalAvgResponseTime = GetFinalAvgResponseTime();

        Debug.Log($"Session {SessionId} completed. \nTotal points: {totalPoints}. \n");


        
        // Aqui export / upload final session data a database
        
        // Tal vez agregar lo de arriba aca?
    }
    #endregion

    #region Topic Functions
    public void BeginTopic(SalesTopic topic)
    {
        if (topic == null)
        {
            Debug.LogError("No topic available.");
            return;
        }
        currentTopicResult = new()
        {
            topicId = topic.topicId,
            topicTitle = topic.topicTitle,
            points = 0,
            correctAnswers = 0,
            avgResponseTime = 0f,
            completed = false,
        };
        TopicResults.Add(currentTopicResult); // Add current newly created topic result to list of topic results
    }

    public void SelectTopic(SalesTopic topic)
    {
        if (topic == null)
        {
            Debug.LogError("Topic is Null.");
            return;
        }
        if (IsTopicCompleted(topic))
        {
            Debug.LogWarning("Topic is already completed.");
            return;
        }
        SelectedTopic = topic; // Set selected topic (on TopicMenuItem[topic]) => Pass later to ModuleFlowContoller
    }

    public bool AllTopicsCompleted()
    {
        return TopicResults.Count > 0 && TopicResults.Count == allTopics.Count && TopicResults.All(t => t.completed);
    }

    public bool IsTopicCompleted(SalesTopic topic)
    {
        return topic != null && TopicResults.Exists(t => t.topicId == topic.topicId) && TopicResults.Find(t => t.topicId == topic.topicId).completed;
    }

    public void CompleteCurrentTopic()
    {
        if (currentTopicResult == null)
        {
            return;
        }
        currentTopicResult.completed = true;
    }

    public int GetCurrentTopicPoints()
    {
        if (currentTopicResult == null)
        {
            Debug.LogError("No current topic active.");
            return 0;
        }
        return currentTopicResult.points;
    }

    public int GetCurrentTopicCorrectAnswers()
    {
        if (currentTopicResult == null)
        {
            Debug.LogError("No current topic active.");
            return 0;
        }
        return currentTopicResult.correctAnswers;
    }

    public float GetCurrentTopicAvgResponseTime()
    {
        if (currentTopicResult == null)
        {
            Debug.LogError("No current topic active.");
            return 0f;
        }

        // Sum all average response times of each module result from current topic
        foreach (ModuleResult moduleResult in currentTopicResult.moduleResults)
        {
            currentTopicResult.avgResponseTime += moduleResult.questionResults.Average(q => q.responseTime);
        }
        return currentTopicResult.avgResponseTime;
    }
    #endregion

    #region Module Functions
    public void BeginModule(MicroModule module)
    {
        if (module == null)
        {
            Debug.LogError("No module available.");
            return;
        }

        currentModuleResult = new()
        {
            topicId = SelectedTopic.topicId,
            moduleId = module.moduleId,
            moduleTitle = module.moduleTitle,
            points = 0,
            correctAnswers = 0,
            completed = false
        };
        currentTopicResult.moduleResults.Add(currentModuleResult); // Add current newly created module result to list of results from current topic
    }

    public void CompleteCurrentModule()
    {
        if (currentModuleResult == null)
        {
            return;
        }
        currentModuleResult.completed = true;
    }

    public void RecordCurrentModuleResult()
    {
        if (currentTopicResult == null)
        {
            Debug.LogError("No active topic for current module.");
            return;
        }
        currentTopicResult.points += currentModuleResult.points; // Add current module points (final) to current topic points
        currentTopicResult.correctAnswers += currentModuleResult.correctAnswers; // Add current module correct answers (final) to current topic correct answers
    }

    public int GetCurrentModulePoints()
    {
        if (currentModuleResult == null)
        {
            Debug.LogError("No current module active.");
            return 0;
        }
        return currentModuleResult.points;
    }

    public int GetCurrentModuleCorrectAnswers()
    {
        if (currentModuleResult == null)
        {
            Debug.LogError("No current module active.");
            return 0;
        }
        return currentModuleResult.correctAnswers;
    }
    #endregion

    #region Question Functions
    public void RecordQuestionResult(QuestionResult result)
    {
        if (currentModuleResult == null)
        {
            Debug.LogError("No active module for current question.");
            return;
        }
        currentModuleResult.questionResults.Add(result); // Add current question result to list of results from current module
        currentModuleResult.points += result.pointsAwarded; // Add the points of current question result to the points of current module
        if (result.wasCorrect)
        {
            currentModuleResult.correctAnswers++; // If current question result correct => increment correct answers of current module
        }
    }
    #endregion

    #region Final Results Functions

    public int GetTotalPoints()
    {
        if (TopicResults.Count == 0)
        {
            Debug.LogError("No topic results available.");
            return 0;
        } 
        return TopicResults.Sum(t => t.points);
    }

    public int GetTotalCorrectAnswers()
    {
        if (TopicResults.Count == 0)
        {
            Debug.LogError("No topic results available.");
            return 0;
        }
        static int moduleCorrectAnswers(ModuleResult m) => m.correctAnswers;
        return TopicResults.Sum(t => t.moduleResults.Sum(moduleCorrectAnswers));
    }

    public float GetFinalAvgResponseTime()
    {
        if (TopicResults.Count == 0)
        {
            Debug.LogError("No topic results available.");
            return 0f;
        }
        return TopicResults.Average(t => t.avgResponseTime);
    }

    public string TotalPointsText()
    {
        StringBuilder statsText = new();
        TopicResults.ForEach(t => statsText.AppendLine($"\"{t.topicTitle}\" Punkte: {t.points}"));
        statsText.AppendLine($"Gesamtpunktzahl: {totalPoints}");
        return statsText.ToString();
    }

    public string TotalCorrectAnswersText()
    {
        static int moduleTotalQuestion(ModuleResult m) => m.questionResults.Count;
        int totalQuestions = TopicResults.Sum(t => t.moduleResults.Sum(moduleTotalQuestion));
        return $"{totalCorrectAnswers} / {totalQuestions}";
    }

    public string FinalAvgResponseTimeText()
    {
        return $"{finalAvgResponseTime:0.0}s";
    }
    #endregion
}
