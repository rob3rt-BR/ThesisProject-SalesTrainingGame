using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class LeaderboardsController : MonoBehaviour
{
    [Header("Screens")]
    [SerializeField]
    private GameObject menuScreen;
    [SerializeField]
    private GameObject traditionalScreen;
    [SerializeField]
    private GameObject aIScreen;
    [SerializeField]
    private GameObject totalScreen;
    [SerializeField]
    private GameObject topFiveScreen;

    [Header("Leaderboards Menu Screen")]
    [SerializeField]
    private GameObject menuTitle;
    [SerializeField]
    private GameObject boardsBackground;
    [SerializeField]
    private GameObject boardsHolder;
    [SerializeField]
    private Button pointsButton;
    [SerializeField]
    private Button responseTimeButton;
    [SerializeField]
    private Button topFiveButton;

    [Header("Traditional Screen")]
    [SerializeField]
    private TMP_Text tradTitle;
    [SerializeField]
    private GameObject tradBackground;
    [SerializeField]
    private GameObject tradBoardHolder;
    [SerializeField]
    private List<LeaderboardItem> tradBoardItems;
    [SerializeField]
    private Button continueToAIButton;

    [Header("AI Screen")]
    [SerializeField]
    private TMP_Text aITitle;
    [SerializeField]
    private GameObject aIBackground;
    [SerializeField]
    private GameObject aIBoardHolder;
    [SerializeField]
    private List<LeaderboardItem> aIBoardItems;
    [SerializeField]
    private Button continueToTotalButton;
    
    [Header("Total Screen")]
    [SerializeField]
    private TMP_Text totalTitle;
    [SerializeField]
    private GameObject totalBackground;
    [SerializeField]
    private GameObject totalBoardHolder;
    [SerializeField]
    private List<LeaderboardItem> totalBoardItems;
    [SerializeField]
    private Button returnToMenuButton;
    
    [Header("Top Five Screen")]
    [SerializeField]
    private TMP_Text topFiveTitle;
    [SerializeField]
    private GameObject topFiveBackground;
    [SerializeField]
    private GameObject topFiveBoardHolder;
    [SerializeField]
    private List<LeaderboardItem> topFiveBoardItems;
    [SerializeField]
    private Button returnToStartButton;
    
    [Header("Animation Controller")]
    [SerializeField]
    private AnimationController animationController;

    private List<ElementAnimation> menuScreenElements;
    private List<ElementAnimation> tradScreenElements;
    private List<ElementAnimation> aIScreenElements;
    private List<ElementAnimation> totalScreenElements;
    private List<ElementAnimation> topFiveScreenElements;

    ElementAnimation tradBoardHolderElement;
    ElementAnimation aIBoardHolderElement;
    ElementAnimation totalBoardHolderElement;
    ElementAnimation topFiveBoardHolderElement;

    public enum LeaderboardType
    {
        None,
        Points,
        ResponseTime,
        TopFive
    }

    [HideInInspector]
    public LeaderboardType currentLeaderboard = LeaderboardType.None;

    private int twoLeaderboardsCompleted = 0;

    void Start()
    {
        // On click => Go to points screens
        pointsButton.onClick.AddListener(StartPointsLeaderboards);

        // On click => Go to response time screens
        responseTimeButton.onClick.AddListener(StartResponseTimeLeaderboards);
        
        // On click => Go to top 5 screen
        topFiveButton.onClick.AddListener(StartTopFiveLeaderboard);
        topFiveButton.interactable = false;

        // On click => Go from traditional screen to AI screen (points and response time)
        continueToAIButton.onClick.AddListener(ShowAILeaderboard);

        // On click => Go from AI screen to total screen (points and response time)
        continueToTotalButton.onClick.AddListener(ShowTotalLeaderboard);

        // On click => Return to leaderboards menu screen from total screen (points and response time)
        returnToMenuButton.onClick.AddListener(ReturnToMenu);

        // On click => Return to StartScene from top five screen
        returnToStartButton.onClick.AddListener(ReturnToStart);

        // Initialize ElementAnimation objects of each element per UI screen
        InitializeElementAnimationGroups();

        // Animate scene start: start of menu screens
        animationController.AnimateSceneStart(menuScreenElements);

        // Disable all leaderboard items before starting display for the first time
        //DisableLeaderboardsItems(tradBoardItems);
        //DisableLeaderboardsItems(aIBoardItems);
        //DisableLeaderboardsItems(totalBoardItems);
        //DisableLeaderboardsItems(topFiveBoardItems);
    }

    private void InitializeElementAnimationGroups()
    {
        // Menu screen elements
        ElementAnimation menuTitleElement = animationController.GetElementAnimation(menuTitle);
        ElementAnimation boardsBackgroundElement = animationController.GetElementAnimation(boardsBackground);
        ElementAnimation boardsHolderElement = animationController.GetElementAnimation(boardsHolder);

        menuScreenElements = new() {menuTitleElement, boardsBackgroundElement, boardsHolderElement};

        // Traditional screen elements
        ElementAnimation tradTitleElement = animationController.GetElementAnimation(tradTitle.gameObject);
        ElementAnimation tradBackgroundElement = animationController.GetElementAnimation(tradBackground);
        ElementAnimation continueToAIButtonElement = animationController.GetElementAnimation(continueToAIButton.gameObject);
        tradBoardHolderElement = animationController.GetElementAnimation(tradBoardHolder);

        tradScreenElements = new() {tradTitleElement, tradBackgroundElement, continueToAIButtonElement};
        
        // AI screen elements
        ElementAnimation aITitleElement = animationController.GetElementAnimation(aITitle.gameObject);
        ElementAnimation aIBackgroundElement = animationController.GetElementAnimation(aIBackground);
        ElementAnimation continueToTotalButtonElement = animationController.GetElementAnimation(continueToTotalButton.gameObject);
        aIBoardHolderElement = animationController.GetElementAnimation(aIBoardHolder);

        aIScreenElements = new() {aITitleElement, aIBackgroundElement, continueToTotalButtonElement};
        
        // Total screen elements
        ElementAnimation totalTitleElement = animationController.GetElementAnimation(totalTitle.gameObject);
        ElementAnimation totalBackgroundElement = animationController.GetElementAnimation(totalBackground);
        ElementAnimation returnToMenuButtonElement = animationController.GetElementAnimation(returnToMenuButton.gameObject);
        totalBoardHolderElement = animationController.GetElementAnimation(totalBoardHolder);

        totalScreenElements = new() {totalTitleElement, totalBackgroundElement, returnToMenuButtonElement};
        
        // Top Five screen elements
        ElementAnimation topFiveTitleElement = animationController.GetElementAnimation(topFiveTitle.gameObject);
        ElementAnimation topFiveBackgroundElement = animationController.GetElementAnimation(topFiveBackground);
        ElementAnimation returnToStartButtonElement = animationController.GetElementAnimation(returnToStartButton.gameObject);
        topFiveBoardHolderElement = animationController.GetElementAnimation(topFiveBoardHolder);

        topFiveScreenElements = new() {topFiveTitleElement, topFiveBackgroundElement, returnToStartButtonElement};
    }

    private void StartPointsLeaderboards()
    {
        currentLeaderboard = LeaderboardType.Points;
        twoLeaderboardsCompleted++;
        
        // Animate screen change: menu screen -> traditional screen (points mode)
        animationController.AnimateScreenChange(menuScreenElements, tradScreenElements, menuScreen, traditionalScreen);

        // Animate traditional board display (points mode)
        animationController.AnimateLeaderboardDisplay(tradBoardItems);
    }

    private void StartResponseTimeLeaderboards()
    {
        currentLeaderboard = LeaderboardType.ResponseTime;
        twoLeaderboardsCompleted++;

        // Animate screen change: menu screen -> traditional screen (response time mode)
        animationController.AnimateScreenChange(menuScreenElements, tradScreenElements, menuScreen, traditionalScreen);

        // Animate traditional board display (response time mode)
        animationController.AnimateLeaderboardDisplay(tradBoardItems);
    }

    private void StartTopFiveLeaderboard()
    {
        currentLeaderboard = LeaderboardType.TopFive;
        
        // Animate screen change: menu screen -> top five screen
        animationController.AnimateScreenChange(menuScreenElements, topFiveScreenElements, menuScreen, topFiveScreen);

        // Animate top five board display
        animationController.AnimateLeaderboardDisplay(topFiveBoardItems);
    }

    private void ShowAILeaderboard()
    {
        ResetMenuAndTotalScreens();

        // Animate traditional board fadeout before changing screen
        animationController.AnimateLeaderboardFadeOut(tradBoardHolderElement);
        
        // Animate screen change: traditional screen -> AI screen
        animationController.AnimateScreenChange(tradScreenElements, aIScreenElements, traditionalScreen, aIScreen);

        // Animate AI board display
        animationController.AnimateLeaderboardDisplay(aIBoardItems);

        if (currentLeaderboard == LeaderboardType.Points)
        {

            return;
        }
        if (currentLeaderboard == LeaderboardType.ResponseTime)
        {
            return;
        }
    }

    private void ShowTotalLeaderboard()
    {
        // Animate AI board fadeout before changing screen
        animationController.AnimateLeaderboardFadeOut(aIBoardHolderElement);

        // Animate screen change: AI screen -> total screen
        animationController.AnimateScreenChange(aIScreenElements, totalScreenElements, aIScreen, totalScreen);

        // Animate total board display
        animationController.AnimateLeaderboardDisplay(totalBoardItems);
        
        if (currentLeaderboard == LeaderboardType.Points)
        {
            return;
        }
        if (currentLeaderboard == LeaderboardType.ResponseTime)
        {
            return;
        }
    }

    private string PlayerStatsText(LeaderboardType type)
    {
        return type switch
        {
            LeaderboardType.Points => "",
            LeaderboardType.ResponseTime => "",
            _ => ""
        };
    }

    private void ReturnToMenu()
    {
        ResetVersionScreens();

        // Animate total board fadeout before changing screen
        animationController.AnimateLeaderboardFadeOut(totalBoardHolderElement);

        // Animate screen change: total screen -> menu screen
        animationController.AnimateScreenChange(totalScreenElements, menuScreenElements, totalScreen, menuScreen);

        // IF leaderboards counter == 2: both leaderboards completed -> enable top five leaderboards
        if (twoLeaderboardsCompleted == 2)
        {
            topFiveButton.interactable = true;
            twoLeaderboardsCompleted = 0;
        }
    }

    private void ReturnToStart()
    {
        // Animate top five board fadeout before changing screen
        animationController.AnimateLeaderboardFadeOut(topFiveBoardHolderElement);

        // Animate screen change: top five screen -> end of LeaderboardsScene and load StartScene
        animationController.AnimateSceneEnd(topFiveScreenElements, "StartScene");
    }

    private void ResetVersionScreens()
    {
        // Restart version boards background from traditional and AI screens to original position
        // AND reset canvas group alpha value for later display before animating again
        animationController.RestartElementToOriginalPosition(tradBackground);
        tradBoardHolderElement.canvasGroup.alpha = 1f;
        animationController.RestartElementToOriginalPosition(aIBackground);
        aIBoardHolderElement.canvasGroup.alpha = 1f;
    }

    private void ResetMenuAndTotalScreens()
    {
        // Reset menu boards and boards background from menu screen to original position before animating again
        animationController.RestartElementToOriginalPosition(boardsBackground);
        animationController.RestartElementToOriginalPosition(boardsHolder);

        // Reset board background from total screen to original position
        // AND reset canvas group alpha value for later display before animating again 
        animationController.RestartElementToOriginalPosition(totalBackground);
        totalBoardHolderElement.canvasGroup.alpha = 1f;
    }

    private void DisableLeaderboardsItems(List<LeaderboardItem> leaderboardItems)
    {
        foreach (LeaderboardItem item in leaderboardItems)
        {
            item.gameObject.SetActive(false);
        }
    }
}
