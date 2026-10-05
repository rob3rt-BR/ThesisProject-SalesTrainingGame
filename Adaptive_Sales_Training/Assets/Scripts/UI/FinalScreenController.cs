using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class FinalScreenController : MonoBehaviour
{
    [Header("Screens")]
    [SerializeField]
    private GameObject finalStatsScreen;
    [SerializeField]
    private GameObject thanksScreen;

    [Header("Final Results UI")]
    [SerializeField]
    private GameObject finalResultsTitle;
    [SerializeField]
    private GameObject finalResultsBackground;
    [SerializeField]
    private GameObject finalPointsTitle;
    //[SerializeField]
    //private TMP_Text topic1FinalPointsText;
    //[SerializeField]
    //private TMP_Text topic2FinalPointsText;
    [SerializeField]
    private TMP_Text finalPointsText;
    [SerializeField]
    private GameObject finalCorrectAnswersTitle;
    [SerializeField]
    private TMP_Text finalCorrectAnswersText;
    [SerializeField]
    private GameObject finalAvgResponseTimeTitle;
    [SerializeField]
    private TMP_Text finalAvgResponseTimeText;
    [SerializeField]
    private GameObject continueButton;

    [Header("Thanks UI")]
    [SerializeField]
    private GameObject thxText;
    [SerializeField]
    private GameObject finishTrainingButton;

    [Header("Animation Controller")]
    [SerializeField]
    private AnimationController animationController;

    private List<ElementAnimation> finalResultsScreenElements;
    private List<ElementAnimation> finalStatsElements;
    private List<ElementAnimation> thanksScreenElements;

    private readonly TrainingSessionManager session = TrainingSessionManager.Instance;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        // Initialize ElementAnimation objects of each element per UI screen
        InitializeElementAnimationGroups();

        // Animate scene start: start of SessionResultsScene
        animationController.AnimateSceneStart(finalResultsScreenElements);

        // Animate final results stats display (one line of text at a time)
        animationController.AnimateFinalStatsDisplay(finalStatsElements, session.FinalTrainingData);
        //animationController.AnimateFinalStatsDisplay(finalStatsElements, TestFinalScreenResults());
    }

    private void InitializeElementAnimationGroups()
    {
        // Final results screen elements
        ElementAnimation finalResultsTitleElement = animationController.GetElementAnimation(finalResultsTitle);
        ElementAnimation finalResultsBackgroundElement = animationController.GetElementAnimation(finalResultsBackground);
        ElementAnimation continueButtonElement = animationController.GetElementAnimation(continueButton);

        finalResultsScreenElements = new() {finalResultsTitleElement, finalResultsBackgroundElement, continueButtonElement};

        // Final result screen stats elements
        ElementAnimation finalPointsTitleElement = animationController.GetElementAnimation(finalPointsTitle);
        //ElementAnimation topic1FinalPointsTextElement = animationController.GetElementAnimation(topic1FinalPointsText.gameObject);
        //ElementAnimation topic2FinalointsTextElement = animationController.GetElementAnimation(topic2FinalPointsText.gameObject);
        ElementAnimation finalPointsTextElement = animationController.GetElementAnimation(finalPointsText.gameObject);
        ElementAnimation finalCorrectAnsTitleElement = animationController.GetElementAnimation(finalCorrectAnswersTitle);
        ElementAnimation finalCorrectAnsTextElement = animationController.GetElementAnimation(finalCorrectAnswersText.gameObject);
        ElementAnimation finalAvgRespTimeTitleElement = animationController.GetElementAnimation(finalAvgResponseTimeTitle);
        ElementAnimation finalAvgRespTimeTextElement = animationController.GetElementAnimation(finalAvgResponseTimeText.gameObject);

        finalStatsElements = new() {finalPointsTitleElement, finalPointsTextElement, finalCorrectAnsTitleElement, finalCorrectAnsTextElement, finalAvgRespTimeTitleElement, finalAvgRespTimeTextElement};
    
        // Thanks screen elemens
        ElementAnimation thxTextElement = animationController.GetElementAnimation(thxText);
        ElementAnimation finishTrainingElement = animationController.GetElementAnimation(finishTrainingButton);

        thanksScreenElements = new() {thxTextElement, finishTrainingElement};
    }

    private TrainingResultData TestFinalScreenResults()
    {
        List<TopicResult> testTopicResults = new()
        {
            new TopicResult()
            {
                topicTitle = "Cold Outreach",
                points = 15000
            },
        };
        return new()
        {
            totalPoints = 30000,
            totalCorrectAnswers = 60,
            finalAvgResponseTime = 2.5f,
            topicResults = testTopicResults
        };
    }

    public void ContinueToThanksScreen()
    {
        // Export final participant training data to predefined local storage directory
        session.ExportFinalTrainingData();

        List<ElementAnimation> allFinalResultsScreenElements = animationController.CombinePointScreenElementsLists(finalResultsScreenElements, finalStatsElements);

        // Animate sceeen change: final results screen -> thanks screen
        animationController.AnimateScreenChange(allFinalResultsScreenElements, thanksScreenElements, finalStatsScreen, thanksScreen);
    }

    public void EndTrainingSession()
    {
        // Animate screen change: thanks screen -> end of SessionResultsScene and load StartScene
        animationController.AnimateSceneEnd(thanksScreenElements, "StartScene");
        Debug.Log("Returning to StartScene.");
    }
}
