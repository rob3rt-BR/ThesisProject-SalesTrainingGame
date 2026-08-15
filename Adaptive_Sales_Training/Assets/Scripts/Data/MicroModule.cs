using System;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "MicroModule", menuName = "Scriptable Objects/MicroModule")]
public class MicroModule : ScriptableObject
{
    public string moduleId;
    public string moduleTitle;
    [TextArea(5, 12)]
    public string moduleText;
    public List<ModuleQuestion> questions = new();

}

[Serializable]
public class ModuleQuestion
{
    public string questionId;
    [TextArea(2, 5)]
    public string questionText;
    public List<string> answerOptions = new();
    public int correctAnswerIndex;

    [Min(15)]
    public float timeLimitSeconds;
    public int basePoints;
    public int speedBonusPoints;
}
