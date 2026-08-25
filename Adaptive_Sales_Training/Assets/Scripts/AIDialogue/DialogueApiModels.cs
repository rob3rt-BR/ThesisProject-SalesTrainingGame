using System;
using UnityEngine;

/*
[Serializable]
public class DialogueApiModels
{
}
*/

[Serializable]
public class DialogueRequest
{
    public string scenarioId;
    public string moduleId;
    public string questionId;
    public string scenarioContext;
    public string customerPersona;
    public string questionText;
    public string selectedAnswer;
    public string previousResponseId;
    
}

[Serializable]
public class DialogueResponse
{
    public string responseId;
    public string replyText;
}
