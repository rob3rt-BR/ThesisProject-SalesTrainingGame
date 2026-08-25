using System;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "AIDialogueScenario", menuName = "Scriptable Objects/AIDialogueScenario")]

public class AIDialogueScenario : ScriptableObject
{
    public MicroModule module;
    [TextArea(3, 8)]
    public string customerPersona;
    [TextArea(3, 8)]
    public string scenarioContext;
    [TextArea(3, 8)]
    public string openingMessage;
    
    public List<AIQuestionContext> questionContexts = new();
}

[Serializable]
public class AIQuestionContext
{
    public string questionId;
    [TextArea(3, 8)]
    public string dialogueContext;
}
