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

    public List<ModuleResult> ModuleResults {get; private set;} = new();

    public string SessionStartCEST {get; private set;} // CEST (Central European Summer Time, UTC+2)
    
    private List<SalesTopic> allTopics;
    private readonly HashSet<string> completedTopicsIds = new();
    private readonly HashSet<string> requiredTopicsIds = new();
    
    private ModuleResult currentModuleResult;
    private readonly Dictionary<string, int> totalPointsPerTopic = new();
    private int totalCorrectAnswers;
    private float avgResponseTime;

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
        ModuleResults.Clear();
        currentModuleResult = null;

        totalPointsPerTopic.Clear();

        allTopics = requiredTopics;

        totalCorrectAnswers = 0;
        avgResponseTime = 0f;

        foreach (SalesTopic topic in requiredTopics)
        {
            if (topic != null)
            {
                requiredTopicsIds.Add(topic.topicId);
                // totalPointsPerTopic.Add(topic.topicId, 0);
            }
        }
    }

    public void EndSession()
    {
        if (!AllTopicsCompleted())
        {
            Debug.LogWarning("Session cannot end - not all topics completed.");
            return;
        }

        Debug.Log($"Session {SessionId} completed. \nTotal points: {GetTotalPoints()}.");

        foreach (ModuleResult module in ModuleResults)
        {
            totalCorrectAnswers += module.questionResults.FindAll(question => question.wasCorrect == true).Count;    
            avgResponseTime += module.questionResults.Average(q => q.responseTime);
        }
        
        // Aqui export / upload final session data a database
        
        // Tal vez agregar lo de arriba aca?
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
        return requiredTopicsIds.Count > 0 && requiredTopicsIds.All(topicId => completedTopicsIds.Contains(topicId));
    }

    public bool IsTopicCompleted(SalesTopic topic)
    {
        return topic != null && completedTopicsIds.Contains(topic.topicId);
    }

    public void CompleteSelectedTopic()
    {
        if (SelectedTopic == null)
        {
            Debug.LogError("No topic selected.");
            return;
        }

        completedTopicsIds.Add(SelectedTopic.topicId);
    }

    public int GetCompletedTopicCount()
    {
        return completedTopicsIds.Count;
    }

    public void SetCurrentTopicTotalPoints(int totalPoints)
    {
        if (SelectedTopic == null)
        {
            Debug.LogError("No topic selected.");
            return;
        }
        // totalPointsPerTopic[SelectedTopic.topicId] = totalPoints;
        totalPointsPerTopic.Add(SelectedTopic.topicId, totalPoints);
    }

    public string FinalPointsText()
    {
        StringBuilder statsText = new();

        foreach (SalesTopic topic in allTopics)
        {
            statsText.AppendLine($"\"{topic.name}\" Punkte: {totalPointsPerTopic.GetValueOrDefault(topic.topicId, 0)}");
        }
        statsText.AppendLine($"Gesamtpunktzahl: {GetTotalPoints()}");
        return statsText.ToString();
    }

    public string TotalCorrectAnswersText()
    {
        int totalQuestions = 0;
        foreach (ModuleResult module in ModuleResults)
        {
            totalQuestions += module.questionResults.Count;
        }
        return $"{totalCorrectAnswers} / {totalQuestions}";
    }

    public string AverageResponseTimeText()
    {
        return $"{avgResponseTime}s";
    }

    public void BeginModule(MicroModule module)
    {
        if (module == null)
        {
            Debug.LogError("No module available.");
            return;
        }

        currentModuleResult = new()
        {
            moduleId = module.moduleId,
            points = 0,
            completed = false
        };

        ModuleResults.Add(currentModuleResult);
    }

    public void CompleteCurrentModule()
    {
        if (currentModuleResult == null)
        {
            return;
        }

        currentModuleResult.completed = true;
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
            return currentModuleResult.questionResults.FindAll(q => q.wasCorrect == true).Count;
        } else
        {
            return 0;
        }
    }

    public int GetTotalPoints()
    {
        int total = 0;

        foreach (ModuleResult result in ModuleResults)
        {
            total += result.points;
        }
        return total;
    }

    public void RecordQuestionResult(QuestionResult result)
    {
        if (currentModuleResult == null)
        {
            Debug.LogError("No active module for current question.");
            return;
        }

        currentModuleResult.questionResults.Add(result);
        currentModuleResult.points += result.pointsAwarded;
    }
}
