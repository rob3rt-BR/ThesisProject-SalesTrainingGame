using System.Collections;
using System.Collections.Generic;
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
    private static readonly WaitForSeconds waitForSeconds = new(1.25f);

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

    private List<ElementAnimation> complementTransitionElements;
    private bool displayStats;

    private ElementAnimation currentElementAnimation;
    private ElementAnimation nextElementAnimation;
    private GameObject currentElement;
    private GameObject nextElement;

    private ElementAnimation complementElementAnimation;
    private bool preEnableComplementElement;
    private bool postEnableComplementElement;
    private bool preAnimationEnable;
    private bool postAnimationEnable;

    private string sceneToLoadName;

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

                // IF displayStats set (call to AnimateStatsDisplay()) => Start coroutine to display one line of text at a time
                if (displayStats)
                {
                    displayStats = false;

                    StartCoroutine(DisplayStats(complementTransitionElements));
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

        StartCoroutine(WaitTimeInSceneStart());
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
        yield return waitForSeconds;

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

        /*
        currentElement = texts[currentTextIndex];
        currentTextIndex = forward? currentTextIndex + 1 : currentTextIndex - 1;
        nextElement = currentTextIndex > 0 || currentTextIndex < texts.Count - 1? texts[currentTextIndex] : null;

        return currentTextIndex;
        */
    }

    // StartScene - Explain screen texts change animation call
    public int AnimateExplainTextChange(GameObject explainTexts, List<GameObject> texts, int currentTextIndex, bool forward)
    {
        currentElementAnimation = GetElementAnimation(explainTexts);
        elapsedTime = 0f;

        transitionPhase = TransitionPhase.FadeOut;

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
        if (!fadeIn)
        {
            currentElement.SetActive(false);
            nextElement.SetActive(true);
        }
    }
    #endregion

    public void AnimateStatsDisplay(List<ElementAnimation> statsElements)
    {
        PrepareAnimation(statsElements, true);

        displayStats = true;

        complementTransitionElements = statsElements;
    }

    private IEnumerator DisplayStats(List<ElementAnimation> statsElements)
    {
        yield return waitForSeconds;

        
        foreach (ElementAnimation element in statsElements)
        {
            float elapsedTimeLocal = 0f;
            while (elapsedTimeLocal < groupAnimationDuration)
            {
                float normalizedTime = Mathf.Clamp01(elapsedTimeLocal / groupAnimationDuration);
                float evaluationTimeLocal = groupEasingCurve.Evaluate(normalizedTime);

                AnimateElement(element, evaluationTimeLocal, true);
                elapsedTimeLocal += Time.deltaTime;
                yield return null;
            }
            // FinishAnimation(statsElements, true);
        }
    }

    public List<ElementAnimation> CombinePointScreenElementsLists(List<ElementAnimation> pointScreenElements, List<ElementAnimation> pointStatsElements)
    {
        List<ElementAnimation> combinedList = new(pointScreenElements);
        foreach (ElementAnimation element in pointStatsElements)
        {
            combinedList.Add(element);
        }
        return combinedList;
    }

    public void RestartPointStatsElementsPosition(List<ElementAnimation> pointStatsElements)
    {
        foreach (ElementAnimation element in pointStatsElements)
        {
            element.RestartToOriginalPosition();
        }
    }
}
