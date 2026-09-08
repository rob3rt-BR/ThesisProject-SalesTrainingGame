using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public enum AnimateToDirection
{
    Top,
    Bottom,
    Left,
    Right
}

public class AnimationController : MonoBehaviour
{
    [SerializeField]
    private Dictionary<GameObject, ElementAnimation> animations;
    [SerializeField]
    private AnimationCurve groupEasingCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);
    [Range(0, 1f)]
    [SerializeField]
    private float groupAnimationDuration = 0.5f;

    void Start()
    {
        foreach (ElementAnimation animation in animations.Values)
        {
            animation.InitializeRectTransformPosition();
            animation.InitializeOffsetPosition();
        }
    }

    private ElementAnimation GetElementAnimation(GameObject element)
    {
        if (animations == null || animations[element] == null)
        {
            Debug.LogError("No animation entry for this element name");
            return null;
        }
        return animations[element];
    }

    #region StartScene Animation Coroutines
    public void AnimateScreenChange_StartToPasswordBox(GameObject welcomeTitle, GameObject startButton, GameObject lowerButtons, GameObject passwordBox, GameObject backButton, GameObject startScreen, GameObject assignTrainingModeScreen)
    {
        StartCoroutine(InsideOutStart_OutsideInPasswordBox(welcomeTitle, startButton, lowerButtons, passwordBox, backButton, startScreen, assignTrainingModeScreen));
    }

    private IEnumerator InsideOutStart_OutsideInPasswordBox(GameObject welcomeTitle, GameObject startButton, GameObject lowerButtons, GameObject passwordBox, GameObject backButton, GameObject startScreen, GameObject assignTrainingModeScreen)
    {
        RectTransform welcomeTitleRect = welcomeTitle.GetComponent<RectTransform>();
        RectTransform startButtonRect = startButton.GetComponent<RectTransform>();
        RectTransform lowerButtonsRect = lowerButtons.GetComponent<RectTransform>();
        RectTransform passwordBoxRect = passwordBox.GetComponent<RectTransform>();
        RectTransform backButtonRect = backButton.GetComponent<RectTransform>();

        CanvasGroup welcomeTitleCanvasGrp = welcomeTitle.GetComponent<CanvasGroup>();
        CanvasGroup startButtonCanvasGrp = startButton.GetComponent<CanvasGroup>();
        CanvasGroup lowerButtonsCanvasGrp = lowerButtons.GetComponent<CanvasGroup>();
        CanvasGroup passwordBoxCanvasGrp = passwordBox.GetComponent<CanvasGroup>();
        CanvasGroup backButtonCanvasGrp = backButton.GetComponent<CanvasGroup>();
        
        Vector2 welcomeTitleCurrPos = welcomeTitleRect.anchoredPosition; 
        Vector2 startButtonCurrPos = startButtonRect.anchoredPosition; 
        Vector2 lowerButtonsCurrPos = lowerButtonsRect.anchoredPosition; 
        Vector2 passwordBoxCurrPos = passwordBoxRect.anchoredPosition; 
        Vector2 backButtonCurrPos = backButtonRect.anchoredPosition; 

        Vector2 welcomeTitleTargetPos = welcomeTitleCurrPos + GetElementAnimation(welcomeTitle).GetOffset(AnimateToDirection.Top);
        Vector2 startButtonTargetPos = startButtonCurrPos + GetElementAnimation(startButton).GetOffset(AnimateToDirection.Right);
        Vector2 lowerButtonsTargetPos = lowerButtonsCurrPos + GetElementAnimation(lowerButtons).GetOffset(AnimateToDirection.Bottom);
        Vector2 passwordBoxTargetPos = passwordBoxCurrPos + GetElementAnimation(passwordBox).GetOffset(AnimateToDirection.Bottom);
        Vector2 backButtonTargetPos = backButtonCurrPos + GetElementAnimation(backButton).GetOffset(AnimateToDirection.Top);

        float elapsedTime = 0f;

        while (elapsedTime < groupAnimationDuration)
        {
            float evaluationAtTime = groupEasingCurve.Evaluate(elapsedTime / groupAnimationDuration);

            welcomeTitleRect.anchoredPosition = Vector2.Lerp(welcomeTitleCurrPos, welcomeTitleTargetPos, evaluationAtTime);
            welcomeTitleCanvasGrp.alpha = Mathf.Lerp(1f, 0f, evaluationAtTime);
            
            startButtonRect.anchoredPosition = Vector2.Lerp(startButtonCurrPos, startButtonTargetPos, evaluationAtTime);
            startButtonCanvasGrp.alpha = Mathf.Lerp(1f, 0f, evaluationAtTime);

            lowerButtonsRect.anchoredPosition = Vector2.Lerp(lowerButtonsCurrPos, lowerButtonsTargetPos, evaluationAtTime);
            lowerButtonsCanvasGrp.alpha = Mathf.Lerp(1f, 0f, evaluationAtTime);

            elapsedTime += Time.deltaTime;
            yield return null;
        }

        welcomeTitleRect.anchoredPosition = welcomeTitleTargetPos;
        welcomeTitleCanvasGrp.alpha = 0;
        welcomeTitleCanvasGrp.interactable = false;
        welcomeTitleCanvasGrp.blocksRaycasts = false;
        
        startButtonRect.anchoredPosition = startButtonTargetPos;
        startButtonCanvasGrp.alpha = 0;
        startButtonCanvasGrp.interactable = false;
        startButtonCanvasGrp.blocksRaycasts = false;
        
        lowerButtonsRect.anchoredPosition = lowerButtonsTargetPos;
        lowerButtonsCanvasGrp.alpha = 0;
        lowerButtonsCanvasGrp.interactable = false;
        lowerButtonsCanvasGrp.blocksRaycasts = false;

        startScreen.SetActive(false);
        assignTrainingModeScreen.SetActive(true);
        passwordBox.SetActive(true);

        elapsedTime = 0f;

        while (elapsedTime < groupAnimationDuration)
        {
            float evaluationAtTime = groupEasingCurve.Evaluate(elapsedTime / groupAnimationDuration);
            
            passwordBoxRect.anchoredPosition = Vector2.Lerp(passwordBoxCurrPos, passwordBoxTargetPos, evaluationAtTime);
            passwordBoxCanvasGrp.alpha = Mathf.Lerp(0f, 1f, evaluationAtTime);
            
            backButtonRect.anchoredPosition = Vector2.Lerp(backButtonCurrPos, backButtonTargetPos, evaluationAtTime);
            backButtonCanvasGrp.alpha = Mathf.Lerp(0f, 1f, evaluationAtTime);

            elapsedTime += Time.deltaTime;
            yield return null;
        }

        passwordBoxRect.anchoredPosition = passwordBoxTargetPos;
        passwordBoxCanvasGrp.alpha = 1;
        passwordBoxCanvasGrp.interactable = true;
        passwordBoxCanvasGrp.blocksRaycasts = true;
        
        backButtonRect.anchoredPosition = backButtonTargetPos;
        backButtonCanvasGrp.alpha = 1;
        backButtonCanvasGrp.interactable = true;
        backButtonCanvasGrp.blocksRaycasts = true;
    }

    public void AnimateScreenChange_PasswordBoxToStart(GameObject welcomeTitle, GameObject startButton, GameObject lowerButtons, GameObject passwordBox, GameObject backButton, GameObject startScreen, GameObject assignTrainingModeScreen)
    {
        StartCoroutine(InsideOutPasswordBox_OutsideInStart(welcomeTitle, startButton, lowerButtons, passwordBox, backButton, startScreen, assignTrainingModeScreen));
    }

    private IEnumerator InsideOutPasswordBox_OutsideInStart(GameObject welcomeTitle, GameObject startButton, GameObject lowerButtons, GameObject passwordBox, GameObject backButton, GameObject startScreen, GameObject assignTrainingModeScreen)
    {
        RectTransform welcomeTitleRect = welcomeTitle.GetComponent<RectTransform>();
        RectTransform startButtonRect = startButton.GetComponent<RectTransform>();
        RectTransform lowerButtonsRect = lowerButtons.GetComponent<RectTransform>();
        RectTransform passwordBoxRect = passwordBox.GetComponent<RectTransform>();
        RectTransform backButtonRect = backButton.GetComponent<RectTransform>();

        CanvasGroup welcomeTitleCanvasGrp = welcomeTitle.GetComponent<CanvasGroup>();
        CanvasGroup startButtonCanvasGrp = startButton.GetComponent<CanvasGroup>();
        CanvasGroup lowerButtonsCanvasGrp = lowerButtons.GetComponent<CanvasGroup>();
        CanvasGroup passwordBoxCanvasGrp = passwordBox.GetComponent<CanvasGroup>();
        CanvasGroup backButtonCanvasGrp = backButton.GetComponent<CanvasGroup>();
        
        Vector2 welcomeTitleCurrPos = welcomeTitleRect.anchoredPosition; 
        Vector2 startButtonCurrPos = startButtonRect.anchoredPosition; 
        Vector2 lowerButtonsCurrPos = lowerButtonsRect.anchoredPosition; 
        Vector2 passwordBoxCurrPos = passwordBoxRect.anchoredPosition; 
        Vector2 backButtonCurrPos = backButtonRect.anchoredPosition; 

        Vector2 welcomeTitleTargetPos = welcomeTitleCurrPos + GetElementAnimation(welcomeTitle).GetOffset(AnimateToDirection.Bottom);
        Vector2 startButtonTargetPos = startButtonCurrPos + GetElementAnimation(startButton).GetOffset(AnimateToDirection.Left);
        Vector2 lowerButtonsTargetPos = lowerButtonsCurrPos + GetElementAnimation(lowerButtons).GetOffset(AnimateToDirection.Top);
        Vector2 passwordBoxTargetPos = passwordBoxCurrPos + GetElementAnimation(passwordBox).GetOffset(AnimateToDirection.Top);
        Vector2 backButtonTargetPos = backButtonCurrPos + GetElementAnimation(backButton).GetOffset(AnimateToDirection.Bottom);

        float elapsedTime = 0f;

        while (elapsedTime < groupAnimationDuration)
        {
            float evaluationAtTime = groupEasingCurve.Evaluate(elapsedTime / groupAnimationDuration);

            passwordBoxRect.anchoredPosition = Vector2.Lerp(passwordBoxCurrPos, passwordBoxTargetPos, evaluationAtTime);
            passwordBoxCanvasGrp.alpha = Mathf.Lerp(1f, 0f, evaluationAtTime);
            
            backButtonRect.anchoredPosition = Vector2.Lerp(backButtonCurrPos, backButtonTargetPos, evaluationAtTime);
            backButtonCanvasGrp.alpha = Mathf.Lerp(1f, 0f, evaluationAtTime);

            elapsedTime += Time.deltaTime;
            yield return null;
        }

        passwordBoxRect.anchoredPosition = passwordBoxTargetPos;
        passwordBoxCanvasGrp.alpha = 0;
        passwordBoxCanvasGrp.interactable = false;
        backButtonCanvasGrp.blocksRaycasts = false;

        backButtonRect.anchoredPosition = backButtonTargetPos;
        backButtonCanvasGrp.alpha = 0;
        backButtonCanvasGrp.interactable = false;
        passwordBoxCanvasGrp.blocksRaycasts = false;

        assignTrainingModeScreen.SetActive(false);
        startScreen.SetActive(true);

        elapsedTime = 0f;

        while (elapsedTime < groupAnimationDuration)
        {
            float evaluationAtTime = groupEasingCurve.Evaluate(elapsedTime / groupAnimationDuration);

            welcomeTitleRect.anchoredPosition = Vector2.Lerp(welcomeTitleCurrPos, welcomeTitleTargetPos, evaluationAtTime);
            welcomeTitleCanvasGrp.alpha = Mathf.Lerp(0f, 1f, evaluationAtTime);
            
            startButtonRect.anchoredPosition = Vector2.Lerp(startButtonCurrPos, startButtonTargetPos, evaluationAtTime);
            startButtonCanvasGrp.alpha = Mathf.Lerp(0f, 1f, evaluationAtTime);
            
            lowerButtonsRect.anchoredPosition = Vector2.Lerp(lowerButtonsCurrPos, lowerButtonsTargetPos, evaluationAtTime);
            lowerButtonsCanvasGrp.alpha = Mathf.Lerp(0f, 1f, evaluationAtTime);

            elapsedTime += Time.deltaTime;
            yield return null;
        }

        welcomeTitleRect.anchoredPosition = welcomeTitleTargetPos;
        welcomeTitleCanvasGrp.alpha = 1;
        welcomeTitleCanvasGrp.interactable = true;
        welcomeTitleCanvasGrp.blocksRaycasts = true;
        
        startButtonRect.anchoredPosition = startButtonTargetPos;
        startButtonCanvasGrp.alpha = 1;
        startButtonCanvasGrp.interactable = true;
        startButtonCanvasGrp.blocksRaycasts = true;
        
        lowerButtonsRect.anchoredPosition = lowerButtonsTargetPos;
        lowerButtonsCanvasGrp.alpha = 1;
        lowerButtonsCanvasGrp.interactable =true;
        lowerButtonsCanvasGrp.blocksRaycasts = true;
    }

    public void AnimateElementEnable_CorrectPassword(GameObject passwordBox, GameObject backButton, GameObject trainingModeElements)
    {
        StartCoroutine(InsideOutPasswordBox_OutsideInTrainingModeElements(passwordBox, backButton, trainingModeElements));
    }

    private IEnumerator InsideOutPasswordBox_OutsideInTrainingModeElements(GameObject passwordBox, GameObject backButton, GameObject trainingModeElements)
    {
        RectTransform passwordBoxRect = passwordBox.GetComponent<RectTransform>();
        RectTransform trainingModeElementsRect = trainingModeElements.GetComponent<RectTransform>();

        CanvasGroup passwordBoxCanvasGrp = passwordBox.GetComponent<CanvasGroup>();
        CanvasGroup backButtonCanvasGrp = backButton.GetComponent<CanvasGroup>();
        CanvasGroup trainingModeElementsCanvasGrp = trainingModeElements.GetComponent<CanvasGroup>();

        Vector2 passwordBoxCurrPos = passwordBoxRect.anchoredPosition; 
        Vector2 trainingModeElementsCurrPos = trainingModeElementsRect.anchoredPosition; 

        Vector2 passwordBoxTargetPos = passwordBoxCurrPos + GetElementAnimation(passwordBox).GetOffset(AnimateToDirection.Top);
        Vector2 trainingModeElementsTargetPos = trainingModeElementsCurrPos + GetElementAnimation(trainingModeElements).GetOffset(AnimateToDirection.Bottom);

        float elapsedTime = 0f;

        while (elapsedTime < groupAnimationDuration)
        {
            float evaluationAtTime = groupEasingCurve.Evaluate(elapsedTime / groupAnimationDuration);
            
            passwordBoxRect.anchoredPosition = Vector2.Lerp(passwordBoxCurrPos, passwordBoxTargetPos, evaluationAtTime);
            passwordBoxCanvasGrp.alpha = Mathf.Lerp(1f, 0f, evaluationAtTime);

            elapsedTime += Time.deltaTime;
            yield return null;
        }

        passwordBoxRect.anchoredPosition = passwordBoxTargetPos;
        passwordBoxCanvasGrp.alpha = 0;
        passwordBoxCanvasGrp.interactable = false;
        passwordBoxCanvasGrp.blocksRaycasts = false;
        
        backButtonCanvasGrp.interactable = false;
        backButtonCanvasGrp.blocksRaycasts = false;

        passwordBox.SetActive(false);
        trainingModeElements.SetActive(true);

        elapsedTime = 0f;

        while (elapsedTime < groupAnimationDuration)
        {
            float evaluationAtTime = groupEasingCurve.Evaluate(elapsedTime / groupAnimationDuration);
            
            trainingModeElementsRect.anchoredPosition = Vector2.Lerp(trainingModeElementsCurrPos, trainingModeElementsTargetPos, evaluationAtTime);
            trainingModeElementsCanvasGrp.alpha = Mathf.Lerp(0f, 1f, evaluationAtTime);

            elapsedTime += Time.deltaTime;
            yield return null;
        }

        trainingModeElementsRect.anchoredPosition = trainingModeElementsTargetPos;
        trainingModeElementsCanvasGrp.alpha = 1;
        trainingModeElementsCanvasGrp.interactable = true;
        trainingModeElementsCanvasGrp.blocksRaycasts = true;
    }

    public void AnimateScreenChange_TrainingModeElementsToStart(GameObject welcomeTitle, GameObject startButton, GameObject lowerButtons, GameObject backButton, GameObject trainingModeElements, GameObject startScreen, GameObject assignTrainingModeScreen)
    {
        StartCoroutine(InsideOutTrainingModeElements_OutsideInStart(welcomeTitle, startButton, lowerButtons, backButton, trainingModeElements, startScreen, assignTrainingModeScreen));
    }

    private IEnumerator InsideOutTrainingModeElements_OutsideInStart(GameObject welcomeTitle, GameObject startButton, GameObject lowerButtons, GameObject backButton, GameObject trainingModeElements, GameObject startScreen, GameObject assignTrainingModeScreen)
    {
        RectTransform welcomeTitleRect = welcomeTitle.GetComponent<RectTransform>();
        RectTransform startButtonRect = startButton.GetComponent<RectTransform>();
        RectTransform lowerButtonsRect = lowerButtons.GetComponent<RectTransform>();
        RectTransform backButtonRect = backButton.GetComponent<RectTransform>();
        RectTransform trainingModeElementsRect = trainingModeElements.GetComponent<RectTransform>();

        CanvasGroup welcomeTitleCanvasGrp = welcomeTitle.GetComponent<CanvasGroup>();
        CanvasGroup startButtonCanvasGrp = startButton.GetComponent<CanvasGroup>();
        CanvasGroup lowerButtonsCanvasGrp = lowerButtons.GetComponent<CanvasGroup>();
        CanvasGroup backButtonCanvasGrp = backButton.GetComponent<CanvasGroup>();
        CanvasGroup trainingModeElementsCanvasGrp = trainingModeElements.GetComponent<CanvasGroup>();
        
        Vector2 welcomeTitleCurrPos = welcomeTitleRect.anchoredPosition; 
        Vector2 startButtonCurrPos = startButtonRect.anchoredPosition; 
        Vector2 lowerButtonsCurrPos = lowerButtonsRect.anchoredPosition; 
        Vector2 backButtonCurrPos = backButtonRect.anchoredPosition; 
        Vector2 trainingModeElementsCurrPos = trainingModeElementsRect.anchoredPosition; 

        Vector2 welcomeTitleTargetPos = welcomeTitleCurrPos + GetElementAnimation(welcomeTitle).GetOffset(AnimateToDirection.Bottom);
        Vector2 startButtonTargetPos = startButtonCurrPos + GetElementAnimation(startButton).GetOffset(AnimateToDirection.Left);
        Vector2 lowerButtonsTargetPos = lowerButtonsCurrPos + GetElementAnimation(lowerButtons).GetOffset(AnimateToDirection.Top);
        Vector2 backButtonTargetPos = backButtonCurrPos + GetElementAnimation(backButton).GetOffset(AnimateToDirection.Bottom);
        Vector2 trainingModeElementsTargetPos = trainingModeElementsCurrPos + GetElementAnimation(trainingModeElements).GetOffset(AnimateToDirection.Top);

        float elapsedTime = 0f;

        while (elapsedTime < groupAnimationDuration)
        {
            float evaluationAtTime = groupEasingCurve.Evaluate(elapsedTime / groupAnimationDuration);
            
            backButtonRect.anchoredPosition = Vector2.Lerp(backButtonCurrPos, backButtonTargetPos, evaluationAtTime);
            backButtonCanvasGrp.alpha = Mathf.Lerp(1f, 0f, evaluationAtTime);

            trainingModeElementsRect.anchoredPosition = Vector2.Lerp(trainingModeElementsCurrPos, trainingModeElementsTargetPos, evaluationAtTime);
            trainingModeElementsCanvasGrp.alpha = Mathf.Lerp(1f, 0f, evaluationAtTime);

            elapsedTime += Time.deltaTime;
            yield return null;
        }

        trainingModeElementsRect.anchoredPosition = trainingModeElementsTargetPos;
        trainingModeElementsCanvasGrp.alpha = 0;
        trainingModeElementsCanvasGrp.interactable = false;
        trainingModeElementsCanvasGrp.blocksRaycasts = false;

        backButtonRect.anchoredPosition = backButtonTargetPos;
        backButtonCanvasGrp.alpha = 0;
        backButtonCanvasGrp.interactable = false;
        backButtonCanvasGrp.blocksRaycasts = false;

        trainingModeElements.SetActive(false);
        assignTrainingModeScreen.SetActive(false);
        startScreen.SetActive(true);

        elapsedTime = 0f;

        while (elapsedTime < groupAnimationDuration)
        {
            float evaluationAtTime = groupEasingCurve.Evaluate(elapsedTime / groupAnimationDuration);

            welcomeTitleRect.anchoredPosition = Vector2.Lerp(welcomeTitleCurrPos, welcomeTitleTargetPos, evaluationAtTime);
            welcomeTitleCanvasGrp.alpha = Mathf.Lerp(0f, 1f, evaluationAtTime);
            
            startButtonRect.anchoredPosition = Vector2.Lerp(startButtonCurrPos, startButtonTargetPos, evaluationAtTime);
            startButtonCanvasGrp.alpha = Mathf.Lerp(0f, 1f, evaluationAtTime);
            
            lowerButtonsRect.anchoredPosition = Vector2.Lerp(lowerButtonsCurrPos, lowerButtonsTargetPos, evaluationAtTime);
            lowerButtonsCanvasGrp.alpha = Mathf.Lerp(0f, 1f, evaluationAtTime);

            elapsedTime += Time.deltaTime;
            yield return null;
        }

        welcomeTitleRect.anchoredPosition = welcomeTitleTargetPos;
        welcomeTitleCanvasGrp.alpha = 1;
        welcomeTitleCanvasGrp.interactable = true;
        welcomeTitleCanvasGrp.blocksRaycasts = true;
        
        startButtonRect.anchoredPosition = startButtonTargetPos;
        startButtonCanvasGrp.alpha = 1;
        startButtonCanvasGrp.interactable = true;
        startButtonCanvasGrp.blocksRaycasts = true;
        
        lowerButtonsRect.anchoredPosition = lowerButtonsTargetPos;
        lowerButtonsCanvasGrp.alpha = 1;
        lowerButtonsCanvasGrp.interactable =true;
        lowerButtonsCanvasGrp.blocksRaycasts = true;
    }

    #endregion
}
