using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class TopicMenuItem : MonoBehaviour
{
    [Header("Topic")]
    [SerializeField]
    private SalesTopic topic;
    [SerializeField]
    private TMP_Text topicText;
    [SerializeField]
    private Button startButton;
    [SerializeField]
    private GameObject completedLabel;

    [Header("AI Dialogue Scenarios (pro Topic Module)")]
    [SerializeField]
    private List<AIDialogueScenario> dialogueScenarios;

    private readonly TrainingSessionManager session = TrainingSessionManager.Instance;

    void Start()
    {
        topicText.text = topic.topicTitle;
        
        bool topicCompleted = session.IsTopicCompleted(topic);

        startButton.interactable = !topicCompleted;

        if (completedLabel != null)
        {
            completedLabel.SetActive(topicCompleted);
        }
    }

    public void LoadAndStartTopic()
    {
        if (topic == null)
        {
            Debug.Log("No topic assigned.");
            return;
        }

        if (session.IsTopicCompleted(topic))
        {
            Debug.Log("Topic already completed.");
            return;
        }
        
        session.BeginTopic(topic); // Initiate currentTopicResult in TraniningSessionManager
        session.SelectTopic(topic); // Set SelectedTopic as thisTopicMenuItem.topic

        if (session.Version.Equals(TrainingVersion.Traditional))
        {
            SceneManager.LoadScene("TrainingScene");
            Debug.Log($"Loading TrainingScene with topic: \"{topic.topicTitle}\".");
        } 
        else if (session.Version.Equals(TrainingVersion.AI))
        {
            session.SetTopicDialogueScenarios(dialogueScenarios); // Set TopicDialogueScenarios as thisTopicMenuItem.dialogueScenarios (of selected topic)

            SceneManager.LoadScene("AITrainingScene");
            Debug.Log($"Loading AITrainingScene with topic: \"{topic.topicTitle}\".");
        } 
        else
        {
            Debug.LogError("Undefined TrainingVersion.");
            return;
        }

    }
}
