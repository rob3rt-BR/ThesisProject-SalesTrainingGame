using UnityEngine;

[CreateAssetMenu(fileName = "PlayerStats", menuName = "Scriptable Objects/PlayerStats")]
public class PlayerStats : ScriptableObject
{
    public string playerName;
    public TrainingVersion trainingVersion;
    public int points;
    public int correctAnswers;
    public float avgResponseTime;
}
