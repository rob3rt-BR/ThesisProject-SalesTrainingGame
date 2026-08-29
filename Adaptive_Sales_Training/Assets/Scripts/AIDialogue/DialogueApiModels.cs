using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class DialogueRequest
{
    public string sessionId;
    public string scenarioId;
    public string moduleId;
    public string questionId;

    public string scenarioContext;
    public string customerPersona;
    public string questionDialogueContext;

    public string questionText;
    public string selectedAnswer;

    public string traineeDialogueText;

    public List<DialogueMessage> conversationHistory = new();    
}

[Serializable]
public class DialogueMessage
{
    public string role;
    public string content;
}

[Serializable]
public class DialogueResponse
{
    public string responseId;
    public string model;
    public int promptTokens;
    public int completionTokens;
    public long responseTimeMilliseconds;

    public string replyText;
}
