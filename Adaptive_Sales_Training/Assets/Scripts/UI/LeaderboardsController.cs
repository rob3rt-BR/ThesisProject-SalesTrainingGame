using System;
using System.Collections.Generic;
using System.Linq;
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

    [Header("PlayerStats Data")]
    [SerializeField]
    private List<PlayerStats> statsData;
    
    private List<PlayerStats> totalPlayerStats;
    private List<PlayerStats> versionPlayerStats;

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

        totalPlayerStats = statsData;
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
        // Start points screen sequences with traditional board
        ShowTraditionalLeaderboard(LeaderboardType.Points);
    }

    private void StartResponseTimeLeaderboards()
    {
        // Start response time screen sequences with traditional board
        ShowTraditionalLeaderboard(LeaderboardType.ResponseTime);
    }

    private void StartTopFiveLeaderboard()
    {
        currentLeaderboard = LeaderboardType.TopFive;

        // Sort top five player stats (first by points, then by response time)
        SortTopFive();

        // Set sorted totalPlayerStats on top five leaderboard items
        SetLeaderboardInfo(topFiveBoardItems, false);
        
        // Animate screen change: menu screen -> top five screen
        animationController.AnimateScreenChange(menuScreenElements, topFiveScreenElements, menuScreen, topFiveScreen);

        // Animate top five board display
        animationController.AnimateLeaderboardDisplay(topFiveBoardItems);
    }

    private void ShowTraditionalLeaderboard(LeaderboardType type)
    {
        currentLeaderboard = type;
        twoLeaderboardsCompleted++;

        // Set traditional screen title based on current leaderboard type
        tradTitle.text = $"{currentLeaderboard} - Traditional";

        // Sort based on current leader board type (points or response time) for traditional version player stats
        SortByVersion(TrainingVersion.Traditional, currentLeaderboard);

        // Set sorted versionPlayerStats on traditional leaderboard items
        SetLeaderboardInfo(tradBoardItems, true);
        
        // Animate screen change: menu screen -> traditional screen
        animationController.AnimateScreenChange(menuScreenElements, tradScreenElements, menuScreen, traditionalScreen);

        // Animate traditional board display
        animationController.AnimateLeaderboardDisplay(tradBoardItems);
    }

    private void ShowAILeaderboard()
    {
        ResetMenuAndTotalScreens();

        // Set AI screen title based on current leaderboard type
        aITitle.text = $"{currentLeaderboard} - AI";

        // Sort based on current leader board type (points or response time) for AI version player stats
        SortByVersion(TrainingVersion.AI, currentLeaderboard);

        // Set sorted versionPlayerStats on AI leaderboard items
        SetLeaderboardInfo(aIBoardItems, true);

        // Animate traditional board fadeout before changing screen
        animationController.AnimateLeaderboardFadeOut(tradBoardHolderElement);
        
        // Animate screen change: traditional screen -> AI screen
        animationController.AnimateScreenChange(tradScreenElements, aIScreenElements, traditionalScreen, aIScreen);

        // Animate AI board display
        animationController.AnimateLeaderboardDisplay(aIBoardItems);
    }

    private void ShowTotalLeaderboard()
    {
        // Sort based on current leader board type (points or response time) for all player stats
        SortTotal(currentLeaderboard);

        // Set total screen title based on current leaderboard type
        totalTitle.text = $"{currentLeaderboard} - Total";

        // Set sorted totalBoardStats on total leaderboard items
        SetLeaderboardInfo(totalBoardItems, false);

        // Animate AI board fadeout before changing screen
        animationController.AnimateLeaderboardFadeOut(aIBoardHolderElement);

        // Animate screen change: AI screen -> total screen
        animationController.AnimateScreenChange(aIScreenElements, totalScreenElements, aIScreen, totalScreen);

        // Animate total board display
        animationController.AnimateLeaderboardDisplay(totalBoardItems);
    }

    private void SetLeaderboardInfo(List<LeaderboardItem> leaderboardItems, bool versionBoard)
    {
        List<PlayerStats> currentStats;

        // IF version leaderboard -> 9 iterations and use versionPlayerStats
        // ELSE (total leaderboard) -> 18 iterations and use totalPlayerStats
        if (versionBoard)
        {
            currentStats = versionPlayerStats;
        }
        else
        {
            currentStats = totalPlayerStats;
        }

        int maxItems = currentStats.Count;
        int placeCounter = maxItems;

        for (int i = 0; i < maxItems; i++)
        {
            string playerName = $"{placeCounter}. {currentStats[i].playerName}";
            string playerStats = PlayerStatsText(currentLeaderboard, currentStats[i]);
            leaderboardItems[i].SetPlayerTexts(playerName, playerStats);

            placeCounter--;
        }
    }

    private string PlayerStatsText(LeaderboardType type, PlayerStats playerStats)
    {
        return type switch
        {
            LeaderboardType.Points => $"{playerStats.points}xp ({playerStats.correctAnswers} / 35)",
            LeaderboardType.ResponseTime => $"{playerStats.avgResponseTime:0.00}s",
            LeaderboardType.TopFive => $"{playerStats.points}xp | {playerStats.avgResponseTime:0.00}s",
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

    private List<PlayerStats> SortByPoints(List<PlayerStats> playerStats)
    {
        return playerStats.OrderBy(p => p.points).ToList();
    }

    private List<PlayerStats> SortByResponseTime(List<PlayerStats> playerStats)
    {
        return playerStats.OrderByDescending(p => p.avgResponseTime).ToList();
    }

    private void SortByVersion(TrainingVersion version, LeaderboardType leaderboardType)
    {
        if (leaderboardType == LeaderboardType.Points)
        {
            versionPlayerStats = SortByPoints(statsData.FindAll(p => p.trainingVersion == version));
            return;
        }
        if (leaderboardType == LeaderboardType.ResponseTime)
        {
            versionPlayerStats = SortByResponseTime(statsData.FindAll(p => p.trainingVersion == version));
            return;
        }
    }

    private void SortTotal(LeaderboardType leaderboardType)
    {
        if (leaderboardType == LeaderboardType.Points)
        {
            totalPlayerStats = SortByPoints(statsData);
            return;
        }
        if (leaderboardType == LeaderboardType.ResponseTime)
        {
            totalPlayerStats = SortByResponseTime(statsData);
            return;
        }
    }

    private void SortTopFive()
    {
        // Sort first by points and limit it to the first 5 player stats, then sort those 5 by response time
        totalPlayerStats = SortByPoints(statsData).GetRange(13, 5); // Index 13 since the leaderboard items are ordered in ascending order
        totalPlayerStats = SortByResponseTime(totalPlayerStats);
    }
}
