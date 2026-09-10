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

    public SalesTopic GetTopic()
    {
        return topic;
    }

    public List<AIDialogueScenario> GetDialogueScenarios()
    {
        return dialogueScenarios;
    }
}
