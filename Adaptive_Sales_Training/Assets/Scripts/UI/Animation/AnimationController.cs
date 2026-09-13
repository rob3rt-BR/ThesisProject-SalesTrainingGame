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
    [Range(0, 1f)]
    [SerializeField]
    private float groupAnimationDuration = 0.5f;
    
    private static readonly WaitForSeconds waitForSeconds = new(1f);

    private enum TransitionPhase
    {
        None,
        Leaving,
        Entering,
        StartingScene,
        EndingScene,
        FadeOut,
        FadeIn
    }

    [Range(0.1f, 5f)]
    [SerializeField]
    private float groupAnimationDurationNoCoroutine = 0.5f;
    private float elapsedTimeNoCoroutine = 0f;
    private TransitionPhase transitionPhase = TransitionPhase.None;

    private List<ElementAnimation> currentTransitionElements;
    private List<ElementAnimation> nextTransitionElements;
    private GameObject currentTransitionScreen;
    private GameObject nextTransitionScreen;

    private ElementAnimation currentElementAnimation;
    private ElementAnimation nextElementAnimation;
    private GameObject currentElement;
    private GameObject nextElement;
    private bool enableCurrentElement;
    private bool postTransitionEnable;

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
        elapsedTimeNoCoroutine += Time.deltaTime;

        float normalizedTime = Mathf.Clamp01(elapsedTimeNoCoroutine / groupAnimationDurationNoCoroutine);
        float evaluationTime = groupEasingCurve.Evaluate(normalizedTime);

        if (transitionPhase == TransitionPhase.Leaving)
        {
            foreach (ElementAnimation element in currentTransitionElements)
            {
                AnimateElement(element, evaluationTime, false);
            }

            if (normalizedTime >= 1f)
            {
                FinishAnimation(currentTransitionElements, false);

                elapsedTimeNoCoroutine = 0f;
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

                transitionPhase = TransitionPhase.None;

                // IF afterTransitionEnable set (call to EnableElementInteractionAfterTransition(element))
                // => Run post transition element interaction update
                if (postTransitionEnable)
                {
                    postTransitionEnable = false;

                    UpdateElementInteractionPostAnimation(currentElementAnimation);
                }
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
        else if (transitionPhase == TransitionPhase.FadeOut)
        {
            AnimateElementFade(currentElementAnimation, evaluationTime, false);

            if (normalizedTime >= 1f)
            {
                FinishElementFade(currentElementAnimation, false);

                elapsedTimeNoCoroutine = 0f;
                transitionPhase = TransitionPhase.FadeIn;
            }
        } 
        else if (transitionPhase == TransitionPhase.FadeIn)
        {
            AnimateElementFade(currentElementAnimation, evaluationTime, true);

            if (normalizedTime >= 1f)
            {
                FinishElementFade(currentElementAnimation, true);

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

        elapsedTimeNoCoroutine = 0f;

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

    #region Post Animation Element Property Manipulation
    public void EnableElementInteractionPostAnimation(GameObject elementObject, bool enable)
    {
        postTransitionEnable = true;

        currentElementAnimation = GetElementAnimation(elementObject);
        enableCurrentElement = enable;
    }

    private void UpdateElementInteractionPostAnimation(ElementAnimation element)
    {
        element.canvasGroup.interactable = enableCurrentElement;
        element.canvasGroup.blocksRaycasts = enableCurrentElement;
    }
    #endregion

    #region Scene Change Animations

    public void AnimateSceneEnd(List<ElementAnimation> currentElements, string nextSceneName) 
    {
        PrepareAnimation(currentElements, false);

        elapsedTimeNoCoroutine = 0f;

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
    #endregion

    #region Fade Change Animation
    public void AnimateFadeChange(GameObject elementObject)
    {
        currentElementAnimation = GetElementAnimation(elementObject);
        elapsedTimeNoCoroutine = 0f;

        transitionPhase = TransitionPhase.FadeOut;

        /*
        currentElement = texts[currentTextIndex];
        currentTextIndex = forward? currentTextIndex + 1 : currentTextIndex - 1;
        nextElement = currentTextIndex > 0 || currentTextIndex < texts.Count - 1? texts[currentTextIndex] : null;

        return currentTextIndex;
        */
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

    #region StartScene - Explain Screen Texts Change
    public int AnimateExplainTextChange(GameObject explainTexts, List<GameObject> texts, int currentTextIndex, bool forward)
    {
        currentElementAnimation = GetElementAnimation(explainTexts);
        elapsedTimeNoCoroutine = 0f;

        transitionPhase = TransitionPhase.FadeOut;

        currentElement = texts[currentTextIndex];
        currentTextIndex = forward? currentTextIndex + 1 : currentTextIndex - 1;
        nextElement = currentTextIndex > 0 || currentTextIndex < texts.Count - 1? texts[currentTextIndex] : null;

        return currentTextIndex;
    }
    #endregion 

    #region TopicMenuScene - Element Animations

    public void AnimateScreenChange_TopicMenuStart(GameObject menuTitle, GameObject topicsBackground, GameObject topicsHolder)
    {
        StartCoroutine(OutsideInTopicMenu(menuTitle, topicsBackground, topicsHolder));
    }

    private IEnumerator OutsideInTopicMenu(GameObject menuTitle, GameObject topicsBackground, GameObject topicsHolder)
    {
        RectTransform menuTitleRect = menuTitle.GetComponent<RectTransform>();
        RectTransform topicsHolderRect = topicsHolder.GetComponent<RectTransform>();

        CanvasGroup menuTitleCanvasGrp = menuTitle.GetComponent<CanvasGroup>();
        CanvasGroup topicsBackgroundCanvasGrp = topicsBackground.GetComponent<CanvasGroup>();
        CanvasGroup topicsHolderCanvasGrp = topicsHolder.GetComponent<CanvasGroup>();

        Vector2 menuTitleCurrPos = menuTitleRect.anchoredPosition;
        Vector2 topicsHolderCurrPos = topicsHolderRect.anchoredPosition;
        
        // Elements entering screen
        Vector2 menuTitleTargetPos = menuTitleCurrPos + GetElementAnimation(menuTitle).GetEnterOffset();
        Vector2 topicsHolderTargetPos = topicsHolderCurrPos + GetElementAnimation(topicsHolder).GetEnterOffset();

        // Wait a couple of seconds after scene change before starting animations
        yield return waitForSeconds;

        float elapsedTime = 0f;

        while (elapsedTime < groupAnimationDuration)
        {
            float evaluationAtTime = groupEasingCurve.Evaluate(elapsedTime / groupAnimationDuration);

            menuTitleRect.anchoredPosition = Vector2.Lerp(menuTitleCurrPos, menuTitleTargetPos, evaluationAtTime);
            menuTitleCanvasGrp.alpha = Mathf.Lerp(0f, 1f, evaluationAtTime);
            
            topicsBackgroundCanvasGrp.alpha = Mathf.Lerp(0f, 1f, evaluationAtTime);

            topicsHolderRect.anchoredPosition = Vector2.Lerp(topicsHolderCurrPos, topicsHolderTargetPos, evaluationAtTime);
            topicsHolderCanvasGrp.alpha = Mathf.Lerp(0f, 1f, evaluationAtTime);

            elapsedTime += Time.deltaTime;
            yield return null;
        }

        menuTitleRect.anchoredPosition = menuTitleTargetPos;
        menuTitleCanvasGrp.alpha = 1;
        menuTitleCanvasGrp.interactable = true;
        menuTitleCanvasGrp.blocksRaycasts = true;

        topicsBackgroundCanvasGrp.alpha = 1;
        topicsBackgroundCanvasGrp.interactable = true;
        topicsBackgroundCanvasGrp.blocksRaycasts = true;
        
        topicsHolderRect.anchoredPosition = topicsHolderTargetPos;
        topicsHolderCanvasGrp.alpha = 1;
        topicsHolderCanvasGrp.interactable = true;
        topicsHolderCanvasGrp.blocksRaycasts = true;
    }
    public void AnimateScreenChange_TopicMenuEnd(GameObject menuTitle, GameObject topicsBackground, GameObject topicsHolder, TrainingVersion version)
    {
        StartCoroutine(InsideOutTopicMenu(menuTitle, topicsBackground, topicsHolder, version));
    }

    private IEnumerator InsideOutTopicMenu(GameObject menuTitle, GameObject topicsBackground, GameObject topicsHolder, TrainingVersion version)
    {
        RectTransform menuTitleRect = menuTitle.GetComponent<RectTransform>();
        RectTransform topicsHolderRect = topicsHolder.GetComponent<RectTransform>();

        CanvasGroup menuTitleCanvasGrp = menuTitle.GetComponent<CanvasGroup>();
        CanvasGroup topicsBackgroundCanvasGrp = topicsBackground.GetComponent<CanvasGroup>();
        CanvasGroup topicsHolderCanvasGrp = topicsHolder.GetComponent<CanvasGroup>();

        Vector2 menuTitleCurrPos = menuTitleRect.anchoredPosition;
        Vector2 topicsHolderCurrPos = topicsHolderRect.anchoredPosition;
        
        // Elements leaving screen
        Vector2 menuTitleTargetPos = menuTitleCurrPos + GetElementAnimation(menuTitle).GetLeaveOffset();
        Vector2 topicsHolderTargetPos = topicsHolderCurrPos + GetElementAnimation(topicsHolder).GetLeaveOffset();

        float elapsedTime = 0f;

        while (elapsedTime < groupAnimationDuration)
        {
            float evaluationAtTime = groupEasingCurve.Evaluate(elapsedTime / groupAnimationDuration);

            menuTitleRect.anchoredPosition = Vector2.Lerp(menuTitleCurrPos, menuTitleTargetPos, evaluationAtTime);
            menuTitleCanvasGrp.alpha = Mathf.Lerp(1f, 0f, evaluationAtTime);
            
            topicsBackgroundCanvasGrp.alpha = Mathf.Lerp(1f, 0f, evaluationAtTime);

            topicsHolderRect.anchoredPosition = Vector2.Lerp(topicsHolderCurrPos, topicsHolderTargetPos, evaluationAtTime);
            topicsHolderCanvasGrp.alpha = Mathf.Lerp(1f, 0f, evaluationAtTime);

            elapsedTime += Time.deltaTime;
            yield return null;
        }

        menuTitleRect.anchoredPosition = menuTitleTargetPos;
        menuTitleCanvasGrp.alpha = 0;
        menuTitleCanvasGrp.interactable = false;
        menuTitleCanvasGrp.blocksRaycasts = false;

        topicsBackgroundCanvasGrp.alpha = 0;
        topicsBackgroundCanvasGrp.interactable = false;
        topicsBackgroundCanvasGrp.blocksRaycasts = false;
        
        topicsHolderRect.anchoredPosition = topicsHolderTargetPos;
        topicsHolderCanvasGrp.alpha = 0;
        topicsHolderCanvasGrp.interactable = false;
        topicsHolderCanvasGrp.blocksRaycasts = false;

        // After ending animation => Load (AI)TrainingScene based on set TrainingVersion (First check in TopicMenuScreenController)
        if (version.Equals(TrainingVersion.Traditional))
        {
            SceneManager.LoadScene("TrainingScene");
            Debug.Log($"Loading TrainingScene.");
        } 
        else if (version.Equals(TrainingVersion.AI))
        {
            SceneManager.LoadScene("AITrainingScene");
            Debug.Log($"Loading AITrainingScene.");
        }
        else
        {
            Debug.LogError("Undefined TrainingVersion.");
            yield break;
        }
    }

    #endregion
}
