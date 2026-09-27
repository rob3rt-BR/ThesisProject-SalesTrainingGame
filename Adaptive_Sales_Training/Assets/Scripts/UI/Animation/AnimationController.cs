using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class AnimationController : MonoBehaviour
{
    [SerializeField]
    private Dictionary<GameObject, ElementAnimation> animations;
    [SerializeField]
    private AnimationCurve groupEasingCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);
    [Range(0.1f, 5f)]
    [SerializeField]
    private float groupAnimationDuration = 0.5f;
    
    private float elapsedTime = 0f;
    private float complementElapsedTime = 0f;

    private static readonly WaitForSeconds waitForSecondsStats = new(1.25f);
    private static readonly WaitForSeconds waitForSecondsDialogue = new(4f);

    private enum TransitionPhase
    {
        None,
        Leaving,
        Entering,
        StartingScene,
        EndingScene,
        FadeOut,
        FadeIn,
    }

    private TransitionPhase transitionPhase = TransitionPhase.None;

    private List<ElementAnimation> currentTransitionElements;
    private List<ElementAnimation> nextTransitionElements;
    private GameObject currentTransitionScreen;
    private GameObject nextTransitionScreen;

    private List<ElementAnimation> complementAnimationElements;
    private enum ComplementAnimationType
    {
        None,
        Stats,
        Dialogue
    }
    private ComplementAnimationType complementAnimationType = ComplementAnimationType.None;

    private enum ComplementAnimation
    {
        None,
        DisplayStats,
        DialogueTexts,
        AnswerIndicatorFade,
        LoadIndicatorLeave,
    }
    private ComplementAnimation complementAnimation = ComplementAnimation.None;
    private bool isOpeningDialogue;

    private enum StatsType
    {
        None,
        Module,
        Topic,
        FinalResults
    }
    private StatsType statsType = StatsType.None;

    private ModuleResult currentModuleResult;
    private TopicResult currentTopicResult;
    private TrainingResultData finalResultData;

    private ElementAnimation currentElementAnimation;
    private ElementAnimation nextElementAnimation;
    private GameObject currentElement;
    private GameObject nextElement;
    private bool isExplainText;

    private ElementAnimation complementElementAnimation;
    private bool preEnableComplementElement;
    private bool postEnableComplementElement;
    private bool preAnimationEnable;
    private bool postAnimationEnable;

    private string sceneToLoadName;
    private Coroutine activeCoroutine;

    void Awake()
    {
        foreach (ElementAnimation animation in animations.Values)
        {
            animation.InitializeElement();
            animation.InitializeOffsetPosition();
        }
    }

    void Update()
    {
        if (transitionPhase != TransitionPhase.None)
        {
            UpdateTransition();
        }
        if (complementAnimation != ComplementAnimation.None)
        {
            UpdateComplementAnimation();
        }
    }

    public ElementAnimation GetElementAnimation(GameObject element)
    {
        if (animations == null || animations[element] == null)
        {
            Debug.LogError("No animation entry for this element name");
            return null;
        }
        return animations[element];
    }


    private void UpdateTransition()
    {
        elapsedTime += Time.deltaTime;

        float normalizedTime = Mathf.Clamp01(elapsedTime / groupAnimationDuration);
        float evaluationTime = groupEasingCurve.Evaluate(normalizedTime);

        if (transitionPhase == TransitionPhase.StartingScene)
        {
            foreach (ElementAnimation element in currentTransitionElements)
            {
                AnimateElement(element, evaluationTime, true);
            }

            if (normalizedTime >= 1f)
            {
                FinishSceneChange(currentTransitionElements, true);

                // IF displayStats set (call to AnimateFinalStatsDisplay()) => Start coroutine to display one line of text at a time
                if (complementAnimationType == ComplementAnimationType.Stats)
                {
                    complementAnimation = ComplementAnimation.DisplayStats;
                    complementAnimationType = ComplementAnimationType.None;
                }

                transitionPhase = TransitionPhase.None;
            }
        }
        else if (transitionPhase == TransitionPhase.EndingScene)
        {
            foreach (ElementAnimation element in currentTransitionElements)
            {
                AnimateElement(element, evaluationTime, false);
            }

            if (normalizedTime >= 1f)
            {
                FinishSceneChange(currentTransitionElements, false);

                transitionPhase = TransitionPhase.None;
            }
        }
        else if (transitionPhase == TransitionPhase.Leaving)
        {
            // IF preAnimationEnable set (call to EnableElementInteraction(element, preAnimation = true))
            // => Update element interaction properties
            if (preAnimationEnable)
            {
                preAnimationEnable = false;

                UpdateElementInteraction(complementElementAnimation, true);
            }

            foreach (ElementAnimation element in currentTransitionElements)
            {
                AnimateElement(element, evaluationTime, false);
            }

            if (normalizedTime >= 1f)
            {
                FinishAnimation(currentTransitionElements, false);

                elapsedTime = 0f;
                transitionPhase = TransitionPhase.Entering;
            }
        } 
        else if (transitionPhase == TransitionPhase.Entering)
        {
            foreach (ElementAnimation element in nextTransitionElements)
            {
                AnimateElement(element, evaluationTime, true);
            }

            if (normalizedTime >= 1f)
            {
                FinishAnimation(nextTransitionElements, true);

                // IF postAnimationEnable set (call to EnableElementInteraction(element, preAnimation = false))
                // => Update element interaction properties
                if (postAnimationEnable)
                {
                    postAnimationEnable = false;

                    UpdateElementInteraction(complementElementAnimation, false);
                }

                // IF displayStats set (call to Animate[Module/Topic]StatsDisplay()) => Start coroutine to display one line of text at a time
                switch (complementAnimationType)
                {
                    case ComplementAnimationType.Stats:
                        {
                            complementAnimation = ComplementAnimation.DisplayStats;
                            complementAnimationType = ComplementAnimationType.None;
                        }
                    break;
                    case ComplementAnimationType.Dialogue:
                        {
                            complementAnimation = ComplementAnimation.DialogueTexts;
                            complementAnimationType = ComplementAnimationType.None;
                        }
                    break;
                    default:
                    break;
                }

                transitionPhase = TransitionPhase.None;
            }
        }
        else if (transitionPhase == TransitionPhase.FadeOut)
        {
            // IF preAnimationEnable set (call to EnableElementInteraction(element, preAnimation = true))
            // => Update element interaction properties
            if (preAnimationEnable)
            {
                preAnimationEnable = false;

                UpdateElementInteraction(complementElementAnimation, true);
            }

            AnimateElementFade(currentElementAnimation, evaluationTime, false);

            if (normalizedTime >= 1f)
            {
                FinishElementFade(currentElementAnimation, false);

                elapsedTime = 0f;
                transitionPhase = TransitionPhase.FadeIn;
            }
        } 
        else if (transitionPhase == TransitionPhase.FadeIn)
        {
            AnimateElementFade(currentElementAnimation, evaluationTime, true);

            if (normalizedTime >= 1f)
            {
                FinishElementFade(currentElementAnimation, true);

                // IF postAnimationEnable set (call to EnableElementInteraction(element, preAnimation = false))
                // => Update element interaction properties
                if (postAnimationEnable)
                {
                    postAnimationEnable = false;

                    UpdateElementInteraction(complementElementAnimation, false);
                }

                transitionPhase = TransitionPhase.None;
                isExplainText = false;
            }
        }
    }

    private void UpdateComplementAnimation()
    {
        complementElapsedTime += Time.deltaTime;

        float normalizedTime = Mathf.Clamp01(complementElapsedTime / groupAnimationDuration);
        float evaluationTime = groupEasingCurve.Evaluate(normalizedTime);

        if (complementAnimation == ComplementAnimation.DisplayStats)
        {
            if (activeCoroutine != null)
            {
                StopCoroutine(activeCoroutine);
            }

            activeCoroutine = StartCoroutine(DisplayStats(complementAnimationElements));

            complementAnimation = ComplementAnimation.None;
        }
        else if (complementAnimation == ComplementAnimation.DialogueTexts)
        {
            if (isOpeningDialogue)
            {
                if (activeCoroutine != null)
                {
                    StopCoroutine(activeCoroutine);
                }

                activeCoroutine = StartCoroutine(OpeningDialogueTextsDisplay(complementAnimationElements));
            }
            else
            {
                DialogueTextDisplay(complementElementAnimation);
            }

            isOpeningDialogue = false;
            complementAnimation = ComplementAnimation.None;
        }
        else if (complementAnimation == ComplementAnimation.LoadIndicatorLeave)
        {
            AnimateElementFade(complementElementAnimation, evaluationTime, false);

            if (normalizedTime >= 1f)
            {
                FinishLoadingIndicatorLeave(complementElementAnimation);

                complementAnimation = ComplementAnimation.None;
            }
        }
        else if (complementAnimation == ComplementAnimation.AnswerIndicatorFade)
        {
            AnimateElementFade(complementElementAnimation, evaluationTime, false);

            if (normalizedTime >= 1f)
            {
                FinishAnswerIndicatorFade(complementElementAnimation);

                complementAnimation = ComplementAnimation.None;
            }
        }
    }

    #region Screen Change Animation
    public void AnimateScreenChange(List<ElementAnimation> currentElements, List<ElementAnimation> nextElements, GameObject currentScreen, GameObject nextScreen)
    {
        PrepareAnimation(currentElements, false);
        PrepareAnimation(nextElements, true);

        nextScreen.SetActive(true);

        elapsedTime = 0f;

        currentTransitionElements = currentElements;
        nextTransitionElements = nextElements;

        currentTransitionScreen = currentScreen;
        nextTransitionScreen = nextScreen;

        transitionPhase = TransitionPhase.Leaving;
    }

    private void AnimateElement(ElementAnimation element, float evaluationTime, bool entering)
    {
        Vector2 currentPosition = element.currentPosition;
        Vector2 targetPosition = element.targetPosition;

        element.rectTransform.anchoredPosition = Vector2.Lerp(currentPosition, targetPosition, evaluationTime);

        element.canvasGroup.alpha = entering? Mathf.Lerp(0f, 1f, evaluationTime) : Mathf.Lerp(1f, 0f, evaluationTime);
    }

    private void PrepareAnimation(List<ElementAnimation> elements, bool entering)
    {
        foreach (ElementAnimation element in elements)
        {
            element.currentPosition = element.rectTransform.anchoredPosition;

            Vector2 offSetType = entering? element.GetEnterOffset() : element.GetLeaveOffset();
            element.targetPosition = element.currentPosition + offSetType;

            if (entering)
            {
                element.canvasGroup.alpha = 0f;
            }
        }
    }

    private void FinishAnimation(List<ElementAnimation> elements, bool entering)
    {
        foreach (ElementAnimation element in elements)
        {
            element.rectTransform.anchoredPosition = element.targetPosition;

            element.canvasGroup.alpha = entering? 1f: 0f;
            element.canvasGroup.interactable = entering;
            element.canvasGroup.blocksRaycasts = entering;
        }

        if (!entering)
        {
            currentTransitionScreen.SetActive(false);
        }
    }
    #endregion

    #region Scene Change Animations

    public void AnimateSceneStart(List<ElementAnimation> currentElements)
    {
        PrepareAnimation(currentElements, true);

        elapsedTime = 0f;

        currentTransitionElements = currentElements;

        activeCoroutine = StartCoroutine(WaitTimeInSceneStart());
    }

    public void AnimateSceneEnd(List<ElementAnimation> currentElements, string nextSceneName) 
    {
        PrepareAnimation(currentElements, false);

        elapsedTime = 0f;

        currentTransitionElements = currentElements;
        sceneToLoadName = nextSceneName;

        transitionPhase = TransitionPhase.EndingScene;
    }

    private void FinishSceneChange(List<ElementAnimation> elements, bool start)
    {
        foreach (ElementAnimation element in elements)
        {
            element.rectTransform.anchoredPosition = element.targetPosition;

            element.canvasGroup.alpha = start? 1f: 0f;
            element.canvasGroup.interactable = start;
            element.canvasGroup.blocksRaycasts = start;
        }

        // IF ending scene => Load next scene based on passed name parameter in AnimateSceneEnd()
        if (!start)
        {
            SceneManager.LoadScene(sceneToLoadName);
            Debug.Log($"Loading {sceneToLoadName}.");
        }
    }

    private IEnumerator WaitTimeInSceneStart()
    {
        yield return waitForSecondsStats;

        transitionPhase = TransitionPhase.StartingScene;
    }
    #endregion

    #region Post Animation Element Property Manipulation

    public void EnableElementInteraction(GameObject elementObject, bool enableStart, bool preAnimation = false, bool postAnimation = false)
    {
        if (preAnimation)
        {
            preAnimationEnable = true;
            preEnableComplementElement = enableStart;
        } 
        if (postAnimation)
        {
            postAnimationEnable = true;
            postEnableComplementElement = !enableStart;
        }

        complementElementAnimation = GetElementAnimation(elementObject);
    }

    private void UpdateElementInteraction(ElementAnimation element, bool preAnimation)
    {
        element.canvasGroup.interactable = preAnimation? preEnableComplementElement : postEnableComplementElement;
        element.canvasGroup.blocksRaycasts = preAnimation? preEnableComplementElement : postEnableComplementElement;
    }
    #endregion

    #region Fade Change Animation
    public void AnimateFadeChange(GameObject elementObject)
    {
        currentElementAnimation = GetElementAnimation(elementObject);
        elapsedTime = 0f;

        transitionPhase = TransitionPhase.FadeOut;
    }

    // StartScene - Explain screen texts change animation call
    public int AnimateExplainTextChange(GameObject explainTexts, List<GameObject> texts, int currentTextIndex, bool forward)
    {
        currentElementAnimation = GetElementAnimation(explainTexts);
        elapsedTime = 0f;

        transitionPhase = TransitionPhase.FadeOut;
        isExplainText = true;

        currentElement = texts[currentTextIndex];
        currentTextIndex = forward? currentTextIndex + 1 : currentTextIndex - 1;
        nextElement = currentTextIndex > 0 || currentTextIndex < texts.Count - 1? texts[currentTextIndex] : null;

        return currentTextIndex;
    }

    private void AnimateElementFade(ElementAnimation element, float evaluationTime, bool fadeIn)
    {
        element.canvasGroup.alpha = fadeIn? Mathf.Lerp(0f, 1f, evaluationTime) : Mathf.Lerp(1f, 0f, evaluationTime);
    }

    private void FinishElementFade(ElementAnimation element, bool fadeIn)
    {
        element.canvasGroup.alpha = fadeIn? 1 : 0;

        if (isExplainText && !fadeIn)
        {
            currentElement.SetActive(false);
            nextElement.SetActive(true);
        }
    }
    #endregion

    #region Correct / Wrong Answer Indicator Fade Animation
    public void AnimateAnswerIndicator(GameObject answerIndicator)
    {
        complementElementAnimation = GetElementAnimation(answerIndicator);
        complementElementAnimation.canvasGroup.alpha = 1f;

        complementElapsedTime = 0f;

        complementAnimation = ComplementAnimation.AnswerIndicatorFade;
    }

    private void FinishAnswerIndicatorFade(ElementAnimation answerIndicatorElement)
    {
        answerIndicatorElement.canvasGroup.alpha = 0f;
    }
    #endregion

    #region Stats Display Animations

    public void AnimateModuleStatsDisplay(List<ElementAnimation> statsElements, ModuleResult moduleResult)
    {
        PrepareStatsDisplayAnimation(statsElements);

        complementAnimationType = ComplementAnimationType.Stats;
        statsType = StatsType.Module;

        complementAnimationElements = statsElements;

        currentModuleResult = moduleResult;
    }

    public void AnimateTopicStatsDisplay(List<ElementAnimation> statsElements, TopicResult topicResult)
    {
        PrepareStatsDisplayAnimation(statsElements);

        complementAnimationType = ComplementAnimationType.Stats;
        statsType = StatsType.Topic;

        complementAnimationElements = statsElements;

        currentTopicResult = topicResult;
    }

    public void AnimateFinalStatsDisplay(List<ElementAnimation> statsElements, TrainingResultData trainingResultData)
    {
        PrepareStatsDisplayAnimation(statsElements);

        complementAnimationType = ComplementAnimationType.Stats;
        statsType = StatsType.FinalResults;

        complementAnimationElements = statsElements;

        finalResultData = trainingResultData;
    }

    private void PrepareStatsDisplayAnimation(List<ElementAnimation> elements)
    {
        foreach (ElementAnimation element in elements)
        {
            if (element.HasInteractiveText())
            {
                element.textComponent.text = "";
                element.canvasGroup.alpha = 1f;
            } else
            {
                element.currentPosition = element.rectTransform.anchoredPosition;

                Vector2 offSetType = element.GetEnterOffset();
                element.targetPosition = element.currentPosition + offSetType;

                element.canvasGroup.alpha = 0f;
            }
        }
    }

    private IEnumerator DisplayStats(List<ElementAnimation> statsElements)
    {
        yield return waitForSecondsStats;

        int currentTopicCounter = 0; // Just for StatsType.FinalResults

        foreach (ElementAnimation element in statsElements)
        {
            float elapsedTimeLocal = 0f;
            while (elapsedTimeLocal < groupAnimationDuration)
            {
                float normalizedTime = Mathf.Clamp01(elapsedTimeLocal / groupAnimationDuration);
                float evaluationTimeLocal = groupEasingCurve.Evaluate(normalizedTime);

                if (element.HasInteractiveText())
                {
                    switch (statsType)
                    {
                        case StatsType.Module:
                            {
                                AnimateModuleStatsElement(element, evaluationTimeLocal);
                            }
                        break;
                        case StatsType.Topic:
                            {
                                AnimateTopicStatsElement(element, evaluationTimeLocal);
                            }
                        break;
                        case StatsType.FinalResults:
                            {
                                AnimateFinalStatsElement(element, evaluationTimeLocal, currentTopicCounter);
                            }
                        break;
                    }
                }
                else
                {
                    AnimateElement(element, evaluationTimeLocal, true);
                }

                elapsedTimeLocal += Time.deltaTime;
                yield return null;
            }

            if (element.HasInteractiveText())
            {
                switch (statsType)
                {
                    case StatsType.Module:
                        {
                            FinishDisplayModuleStats(element);
                        }
                    break;
                    case StatsType.Topic:
                        {
                            FinishDisplayTopicStats(element);
                        }
                    break;
                    case StatsType.FinalResults:
                        {
                            FinishDisplayFinalStats(element, currentTopicCounter);
                        }
                    break;
                }
            }
            else
            {
                FinishDisplayStatsElement(element);
            }

            if (element.HasTextVisualFX())
            {
                element.textVisualFX.Play();
            }

            currentTopicCounter++;

            yield return waitForSecondsStats;
        }

        statsType = StatsType.None;
    }

    private void AnimateModuleStatsElement(ElementAnimation element, float evaluationTime)
    {
        string textToDisplay = "";
        switch (element.GetValueTextType())
        {
            case ElementAnimation.ValueTextType.Points:
                {
                    textToDisplay = Mathf.RoundToInt(Mathf.Lerp(0f, currentModuleResult.points, evaluationTime)).ToString();
                } 
            break;
            case ElementAnimation.ValueTextType.CorrectAnswers:
                {
                    int totalCorrectAnswers = currentModuleResult.questionResults.Count;
                    textToDisplay = $"{Mathf.RoundToInt(Mathf.Lerp(0f, currentModuleResult.correctAnswers, evaluationTime))} / {totalCorrectAnswers}";
                } 
            break;
        }
        element.textComponent.text = textToDisplay;
    }

    private void FinishDisplayModuleStats(ElementAnimation element)
    {
        string textToDisplay = "";
        switch (element.GetValueTextType())
        {
            case ElementAnimation.ValueTextType.Points:
                {
                    textToDisplay = currentModuleResult.points.ToString();
                } 
            break;
            case ElementAnimation.ValueTextType.CorrectAnswers:
                {
                    int totalCorrectAnswers = currentModuleResult.questionResults.Count;
                    textToDisplay = $"{currentModuleResult.correctAnswers} / {totalCorrectAnswers}";
                } 
            break;
        }
        element.textComponent.text = textToDisplay;
    }

    private void AnimateTopicStatsElement(ElementAnimation element, float evaluationTime)
    {
        string textToDisplay = "";
        switch (element.GetValueTextType())
        {
            case ElementAnimation.ValueTextType.Points:
                {
                    textToDisplay = Mathf.RoundToInt(Mathf.Lerp(0f, currentTopicResult.points, evaluationTime)).ToString();
                } 
            break;
            case ElementAnimation.ValueTextType.CorrectAnswers:
                {
                    int totalCorrectAnswers = currentTopicResult.moduleResults.Sum(m => m.questionResults.Count);
                    textToDisplay = $"{Mathf.RoundToInt(Mathf.Lerp(0f, currentTopicResult.correctAnswers, evaluationTime))} / {totalCorrectAnswers}";
                } 
            break;
            case ElementAnimation.ValueTextType.AvgResponseTime:
                {
                    textToDisplay = Mathf.Lerp(0f, currentTopicResult.avgResponseTime, evaluationTime).ToString("0.0") + "s";
                }
            break;
        }
        element.textComponent.text = textToDisplay;
    }

    private void FinishDisplayTopicStats(ElementAnimation element)
    {
        string textToDisplay = "";
        switch (element.GetValueTextType())
        {
            case ElementAnimation.ValueTextType.Points:
                {
                    textToDisplay = currentTopicResult.points.ToString();
                } 
            break;
            case ElementAnimation.ValueTextType.CorrectAnswers:
                {
                    int totalCorrectAnswers = currentTopicResult.moduleResults.Sum(m => m.questionResults.Count);
                    textToDisplay = $"{currentTopicResult.correctAnswers} / {totalCorrectAnswers}";
                } 
            break;
            case ElementAnimation.ValueTextType.AvgResponseTime:
                {
                   textToDisplay = currentTopicResult.avgResponseTime.ToString("0.0") + "s";
                }
            break;
        }
        element.textComponent.text = textToDisplay;
    }

    private void AnimateFinalStatsElement(ElementAnimation element, float evaluationTime, int topicIndex)
    {
        string textToDisplay = "";
        switch (element.GetValueTextType())
        {
            case ElementAnimation.ValueTextType.ResultsTopicPoints:
                {
                    TopicResult currentTopic = finalResultData.topicResults[topicIndex];
                    textToDisplay = $"{currentTopic.topicTitle} Punkte: {Mathf.RoundToInt(Mathf.Lerp(0f, currentTopic.points, evaluationTime))}";
                } 
            break;
            case ElementAnimation.ValueTextType.ResultsTotalPoints:
                {
                    textToDisplay = $"Gesamtpunktzahl: {Mathf.RoundToInt(Mathf.Lerp(0f, finalResultData.totalPoints, evaluationTime))}";
                } 
            break;
            case ElementAnimation.ValueTextType.CorrectAnswers:
                {
                    static int moduleTotalQuestion(ModuleResult m) => m.questionResults.Count;
                    int totalCorrectAnswers = finalResultData.topicResults.Sum(t => t.moduleResults.Sum(moduleTotalQuestion));

                    textToDisplay = $"{Mathf.RoundToInt(Mathf.Lerp(0f, finalResultData.totalCorrectAnswers, evaluationTime))} / {totalCorrectAnswers}";
                } 
            break;
            case ElementAnimation.ValueTextType.AvgResponseTime:
                {
                    textToDisplay = Mathf.Lerp(0f, finalResultData.finalAvgResponseTime, evaluationTime).ToString("0.0") + "s";
                }
            break;
        }
        element.textComponent.text = textToDisplay;
    }

    private void FinishDisplayFinalStats(ElementAnimation element, int topicIndex)
    {
        string textToDisplay = "";
        switch (element.GetValueTextType())
        {
            case ElementAnimation.ValueTextType.ResultsTopicPoints:
                {
                    TopicResult currentTopic = finalResultData.topicResults[topicIndex];
                    textToDisplay = $"{currentTopic.topicTitle} Punkte: {currentTopic.points}";
                } 
            break;
            case ElementAnimation.ValueTextType.ResultsTotalPoints:
                {
                    textToDisplay = $"Gesamtpunktzahl: {finalResultData.totalPoints}";
                } 
            break;
            case ElementAnimation.ValueTextType.CorrectAnswers:
                {
                    static int moduleTotalQuestion(ModuleResult m) => m.questionResults.Count;
                    int totalCorrectAnswers = finalResultData.topicResults.Sum(t => t.moduleResults.Sum(moduleTotalQuestion));

                    textToDisplay = $"{finalResultData.totalCorrectAnswers} / {totalCorrectAnswers}";
                } 
            break;
            case ElementAnimation.ValueTextType.AvgResponseTime:
                {
                   textToDisplay = finalResultData.finalAvgResponseTime.ToString("0.0") + "s";
                }
            break;
        }
        element.textComponent.text = textToDisplay;
    }

    private void FinishDisplayStatsElement(ElementAnimation element)
    {
        element.rectTransform.anchoredPosition = element.targetPosition;

        element.canvasGroup.alpha = 1f;
        element.canvasGroup.interactable = true;
        element.canvasGroup.blocksRaycasts = true;
    }
    #endregion

    #region AITrainingScene Animations

    public void AnimateDialogueTextsDisplay(List<ElementAnimation> dialogueTexts, bool opening)
    {
        complementElapsedTime = 0f;

        if (opening)
        {
            complementAnimationElements = dialogueTexts;
        }
        else
        {
            complementElementAnimation = dialogueTexts[0];
        }
        isOpeningDialogue = opening;

        complementAnimationType = ComplementAnimationType.Dialogue;
    }

    private IEnumerator OpeningDialogueTextsDisplay(List<ElementAnimation> elements)
    {
        foreach (ElementAnimation element in elements)
        {
            element.gameObject.SetActive(true);

            yield return waitForSecondsDialogue;
        }
    }

    private void DialogueTextDisplay(ElementAnimation dialogueTextElement)
    {
        dialogueTextElement.gameObject.SetActive(true);
    }

    public void AnimateLoadingIndicatorLeave(GameObject loadingIndicator)
    {
        complementElapsedTime = 0f;

        complementElementAnimation = GetElementAnimation(loadingIndicator);

        complementAnimation = ComplementAnimation.LoadIndicatorLeave;
    }

    private void FinishLoadingIndicatorLeave(ElementAnimation loadingIndicatorElement)
    {
        loadingIndicatorElement.canvasGroup.alpha = 0f;
        loadingIndicatorElement.canvasGroup.interactable = false;
        loadingIndicatorElement.canvasGroup.blocksRaycasts = false;
    }

    #endregion

    #region External Helper functions
    public List<ElementAnimation> CombinePointScreenElementsLists(List<ElementAnimation> pointScreenElements, List<ElementAnimation> pointStatsElements)
    {
        List<ElementAnimation> combinedList = new(pointScreenElements);
        foreach (ElementAnimation element in pointStatsElements)
        {
            combinedList.Add(element);
        }
        return combinedList;
    }

    public bool IsCurrentTransitionPhaseNone()
    {
        return transitionPhase == TransitionPhase.None;
    }

    public void RestartElementToOriginalPosition(GameObject elementObj)
    {
        GetElementAnimation(elementObj).RestartToOriginalPosition();
    }

    public void RestartPointStatsElementsPosition(List<ElementAnimation> pointStatsElements)
    {
        foreach (ElementAnimation element in pointStatsElements)
        {
            element.RestartToOriginalPosition();
        }
    }
    #endregion
}
