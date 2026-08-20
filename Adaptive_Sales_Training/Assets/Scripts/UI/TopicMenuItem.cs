using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class TopicMenuItem : MonoBehaviour
{
    [SerializeField]
    private SalesTopic topic;
    [SerializeField]
    private Button startButton;
    [SerializeField]
    private GameObject completedLabel;

    private readonly TrainingSessionManager session = TrainingSessionManager.Instance;

    void Start()
    {
        bool topicCompleted = session.IsTopicCompleted(topic);

        startButton.interactable = !topicCompleted;

        if (completedLabel != null)
        {
            completedLabel.SetActive(!topicCompleted);
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

        SceneManager.LoadScene("TrainingScene");
        Debug.Log($"Loading TrainingScene with topic: \"{topic.topicTitle}\".");
    }
}
