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
    public string moduleId;
    public int points;
    public bool completed;
    public List<QuestionResult> questionResults = new();
}

public enum TrainingVersion
{
    Traditional,
    AI
}
