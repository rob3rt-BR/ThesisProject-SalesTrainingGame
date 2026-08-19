using System;
using System.Collections.Generic;
using UnityEngine;

//public class TrainingResultData
//{
    
//}

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

public enum TrainingVersion
{
    Traditional,
    AI
}
