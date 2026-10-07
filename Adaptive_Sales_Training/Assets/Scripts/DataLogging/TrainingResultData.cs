using System;
using System.IO;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class TrainingResultData
{
    public string participantId;
    public string version;
    public string participantStart;
    public string participantEnd;
    public int totalPoints;
    public int totalCorrectAnswers;
    public float finalAvgResponseTime;
    public List<TopicResult> topicResults = new();
}

[Serializable]
public class TopicResult
{
    public string topicId;
    public string topicTitle;
    public int points;
    public int correctAnswers;
    public float avgResponseTime;
    public bool completed;
    public List<ModuleResult> moduleResults = new();
}

[Serializable]
public class ModuleResult
{
    public string topicId;
    public string moduleId;
    public string moduleTitle;
    public int points;
    public int correctAnswers;
    public bool completed;
    public List<QuestionResult> questionResults = new();
}

[Serializable]
public class QuestionResult
{
    public string moduleId;
    public string questionId;
    public int selectedAnswerIndex;
    public bool wasCorrect;
    public bool timedOut;
    public float responseTime;
    public int pointsAwarded;

    public AIResponseLog aIResponse;
}

[Serializable]
public class AIResponseLog
{
    public string responseId;
    public string model;
    public long totalResponseTimeMilliseconds;

    public string replyText;
}

public enum TrainingVersion
{
    Undefined,
    Traditional,
    AI
}
