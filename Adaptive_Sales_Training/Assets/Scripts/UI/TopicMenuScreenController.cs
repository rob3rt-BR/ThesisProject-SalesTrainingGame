using UnityEngine;
using UnityEngine.SceneManagement;

public class TopicMenuScreenController : MonoBehaviour
{
    [Header("Menu Elements")]
    [SerializeField]
    private GameObject menuTitle;
    [SerializeField]
    private GameObject topicsBackground;
    [SerializeField]
    private GameObject topicsHolder;

    [Header("Animation Controller")]
    [SerializeField]
    private AnimationController animationController;

    private readonly TrainingSessionManager session = TrainingSessionManager.Instance;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        // Set topicsBackground canvas group alpha value to 0 before animation starts
        topicsBackground.GetComponent<CanvasGroup>().alpha = 0f;

        // Animate screen change: start of topic menu screen
        animationController.AnimateScreenChange_TopicMenuStart(menuTitle, topicsBackground, topicsHolder);
    }

    public void LoadAndStartTopic(TopicMenuItem menuItem)
    {
        SalesTopic topic = menuItem.GetTopic();

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
        session.SelectTopic(topic); // Set SelectedTopic as current TopicMenuItem.topic

        if (session.Version.Equals(TrainingVersion.Traditional))
        {            
            // Animate screen change: topic menu screen -> end of TopicMenuScene and load TrainingScene
            animationController.AnimateScreenChange_TopicMenuEnd(menuTitle, topicsBackground, topicsHolder, TrainingVersion.Traditional);
            Debug.Log($"Topic: \"{topic.topicTitle}\".");
        } 
        else if (session.Version.Equals(TrainingVersion.AI))
        {
            // Set TopicDialogueScenarios as current TopicMenuItem.dialogueScenarios (of selected topic)
            session.SetTopicDialogueScenarios(menuItem.GetDialogueScenarios());

            // Animate screen change: topic menu screen -> end of TopicMenuScene and load AITrainingScene
            animationController.AnimateScreenChange_TopicMenuEnd(menuTitle, topicsBackground, topicsHolder, TrainingVersion.AI);
            Debug.Log($"Topic: \"{topic.topicTitle}\".");
        } 
        else
        {
            Debug.LogError("Undefined TrainingVersion.");
            return;
        }

    }
}
