using UnityEngine;

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

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        // Set topicsBackground canvas group alpha value to 0 before animation starts
        topicsBackground.GetComponent<CanvasGroup>().alpha = 0f;

        // Animate screen change: start of topic menu screen
        animationController.AnimateScreenChange_TopicMenuStart(menuTitle, topicsBackground, topicsHolder);
    }
}
