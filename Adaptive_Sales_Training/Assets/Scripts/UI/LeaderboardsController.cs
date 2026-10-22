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
    private List<LeaderboardItem> tradBoardItems;
    [SerializeField]
    private Button continueToAIButton;

    [Header("AI Screen")]
    [SerializeField]
    private TMP_Text aITitle;
    [SerializeField]
    private GameObject aIBackground;
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
    private List<LeaderboardItem> totalBoardItems;
    [SerializeField]
    private Button returnToMenuButton;
    
    [Header("Top Five Screen")]
    [SerializeField]
    private TMP_Text topFiveTitle;
    [SerializeField]
    private GameObject topFiveBackground;
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

    public enum LeaderboardType
    {
        None,
        Points,
        ResponseTime,
        TopFive
    }

    [HideInInspector]
    public LeaderboardType currentLeaderboard = LeaderboardType.None;

    void Start()
    {
        // On click => Go to points screens
        pointsButton.onClick.AddListener(StartPointsLeaderboards);

        // On click => Go to response time screens
        responseTimeButton.onClick.AddListener(StartResponseTimeLeaderboards);
        
        // On click => Go to top 5 screen
        topFiveButton.onClick.AddListener(StartTopFiveLeaderboard);

        // Initialize ElementAnimation objects of each element per UI screen
        InitializeElementAnimationGroups();

        // Animate scene start: start of menu screens
        animationController.AnimateSceneStart(menuScreenElements);
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

        tradScreenElements = new() {tradTitleElement, tradBackgroundElement, continueToAIButtonElement};
        
        // AI screen elements
        ElementAnimation aITitleElement = animationController.GetElementAnimation(aITitle.gameObject);
        ElementAnimation aIBackgroundElement = animationController.GetElementAnimation(aIBackground);
        ElementAnimation continueToTotalButtonElement = animationController.GetElementAnimation(continueToTotalButton.gameObject);

        aIScreenElements = new() {aITitleElement, aIBackgroundElement, continueToTotalButtonElement};
        
        // Total screen elements
        ElementAnimation totalTitleElement = animationController.GetElementAnimation(totalTitle.gameObject);
        ElementAnimation totalBackgroundElement = animationController.GetElementAnimation(totalBackground);
        ElementAnimation returnToMenuButtonElement = animationController.GetElementAnimation(returnToMenuButton.gameObject);

        totalScreenElements = new() {totalTitleElement, totalBackgroundElement, returnToMenuButtonElement};
        
        // Top Five screen elements
        ElementAnimation topFiveTitleElement = animationController.GetElementAnimation(topFiveTitle.gameObject);
        ElementAnimation topFiveBackgroundElement = animationController.GetElementAnimation(topFiveBackground);
        ElementAnimation returnToStartButtonElement = animationController.GetElementAnimation(returnToStartButton.gameObject);

        topFiveScreenElements = new() {topFiveTitleElement, topFiveBackgroundElement, returnToStartButtonElement};
    }

    private void StartPointsLeaderboards()
    {
        currentLeaderboard = LeaderboardType.Points;
        // animationController.AnimateScreenChange();
    }

    private void StartResponseTimeLeaderboards()
    {
        currentLeaderboard = LeaderboardType.ResponseTime;
        // animationController.AnimateScreenChange();
    }

    private void StartTopFiveLeaderboard()
    {
        currentLeaderboard = LeaderboardType.TopFive;
        // animationController.AnimateScreenChange();
    }
}
