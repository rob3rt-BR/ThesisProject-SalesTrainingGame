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

    public SalesTopic SelectedTopic {get; private set;}

    //public List<ModuleResult> ModuleResults {get; private set;} = new();
    public List<TopicResult> TopicResults {get; private set;} = new();

    public string SessionStartCEST {get; private set;} // CEST (Central European Summer Time, UTC+2)
    
    // private List<SalesTopic> allTopics;

    // TODO: Check utilidad de hashSets despues de refactor
    // private readonly HashSet<string> completedTopicsIds = new();
    // private readonly HashSet<string> requiredTopicsIds = new();
    
    private TopicResult currentTopicResult;
    private ModuleResult currentModuleResult;

    // private readonly Dictionary<string, int> totalPointsPerTopic = new();
    private int totalCorrectAnswers;
    private float finalAvgResponseTime;

    // private int topicCorrectAnswers;
    // private float topicAvgResponseTime;

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

    public void StartSession(string participantId, TrainingVersion version, List<SalesTopic> requiredTopics)
    {
        ParticipantId = participantId;
        Version = version;
        SessionId = Guid.NewGuid().ToString(); // TODO: Check alternatives para SessionId

        SessionStartCEST = DateTime.UtcNow.ToString("O"); // TODO: Check funcionalidad

        // TODO: Incluir vaciar atributos en EndSession() => Testear primero y recoleactar datos
        SelectedTopic = null;
        // ModuleResults.Clear();
        TopicResults.Clear();
        currentModuleResult = null;

        // totalPointsPerTopic.Clear();

        // allTopics = requiredTopics;

        totalCorrectAnswers = 0;
        finalAvgResponseTime = 0f;

        // topicCorrectAnswers = 0;
        // topicAvgResponseTime = 0f;

        /*foreach (SalesTopic topic in requiredTopics)
        {
            if (topic != null)
            {
                requiredTopicsIds.Add(topic.topicId);
                // totalPointsPerTopic.Add(topic.topicId, 0);
            }
        }*/
    }

    public void EndSession()
    {
        if (!AllTopicsCompleted())
        {
            Debug.LogWarning("Session cannot end - not all topics completed.");
            return;
        }

        Debug.Log($"Session {SessionId} completed. \nTotal points: {GetTotalPoints()}.");

        foreach (TopicResult topic in TopicResults)
        {
            foreach (ModuleResult module in topic.moduleResults)
            {
                totalCorrectAnswers += module.correctAnswers;    
            }
            finalAvgResponseTime += topic.avgResponseTime;
        }

        
        // Aqui export / upload final session data a database
        
        // Tal vez agregar lo de arriba aca?
    }

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
        TopicResults.Add(currentTopicResult);
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
        SelectedTopic = topic;
    }

    public bool AllTopicsCompleted()
    {
        return TopicResults.All(t => t.completed == true);
    }

    /*
    public bool AllTopicsCompleted()
    {
        return requiredTopicsIds.Count > 0 && requiredTopicsIds.All(topicId => completedTopicsIds.Contains(topicId));
    }*/

    public bool IsTopicCompleted(SalesTopic topic)
    {
        return topic != null && currentTopicResult.topicId.Equals(topic.topicId) && currentTopicResult.completed == true;
    }

    public void CompleteCurrentTopic()
    {
        if (currentTopicResult == null)
        {
            return;
        }
        foreach (ModuleResult moduleResult in currentTopicResult.moduleResults)
        {
            currentTopicResult.avgResponseTime += moduleResult.questionResults.Average(q => q.responseTime);
        }
        currentTopicResult.completed = true;
        // completedTopicsIds.Add(SelectedTopic.topicId);
    }

    public void SetCurrentTopicTotalPoints(int totalPoints)
    {
        if (SelectedTopic == null)
        {
            Debug.LogError("No topic selected.");
            return;
        }
        // totalPointsPerTopic.Add(SelectedTopic.topicId, totalPoints);
    }

    public int GetCurrentTopicCorrectAnswers()
    {
        if (currentTopicResult != null)
        {
            return currentTopicResult.correctAnswers;
        } else
        {
            return 0;
        }
    }

    public float GetCurrentTopicAvgResponseTime()
    {
        if (currentTopicResult != null)
        {
            return currentTopicResult.avgResponseTime;
        } else
        {
            return 0f;
        }
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
        currentTopicResult.moduleResults.Add(currentModuleResult);
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
        currentTopicResult.points += currentModuleResult.points;
        currentTopicResult.correctAnswers += currentModuleResult.correctAnswers;
    }

    public int GetCurrentModulePoints()
    {
        if (currentModuleResult != null)
        {
            return currentModuleResult.points;
        } else
        {
            return 0;
        }
    }

    public int GetCurrentModuleCorrectAnswers()
    {
        if (currentModuleResult != null)
        {
            return currentModuleResult.correctAnswers;
        } else
        {
            return 0;
        }
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
        currentModuleResult.questionResults.Add(result);
        currentModuleResult.points += result.pointsAwarded;
        if (result.wasCorrect)
        {
            currentModuleResult.correctAnswers++;
        }
    }
    #endregion

    #region Final Results Functions
    public string FinalPointsText()
    {
        StringBuilder statsText = new();

        foreach (TopicResult topic in TopicResults)
        {
            // TODO: Cambiar esto
            statsText.AppendLine($"\"{topic.topicTitle}\" Punkte: {topic.points}");
        }
        statsText.AppendLine($"Gesamtpunktzahl: {GetTotalPoints()}");
        return statsText.ToString();
    }

    public string TotalCorrectAnswersText()
    {
        int totalQuestions = 0;
        foreach (TopicResult topic in TopicResults)
        {
            foreach (ModuleResult module in topic.moduleResults)
            {
                totalQuestions += module.questionResults.Count;
            }
        }
        return $"{totalCorrectAnswers} / {totalQuestions}";
    }

    public string FinalAverageResponseTimeText()
    {
        return $"{finalAvgResponseTime}s";
    }

    public int GetTotalPoints()
    {
        int total = 0;

        foreach (TopicResult topic in TopicResults)
        {
            total += topic.points;
        }
        return total;
    }
    #endregion
}
