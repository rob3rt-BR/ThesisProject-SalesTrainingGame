using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;
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

    public int CurrentTextIndex {get; private set;}
    
    private static readonly WaitForSeconds waitForSeconds = new(1f);

    void Awake()
    {
        foreach (ElementAnimation animation in animations.Values)
        {
            animation.InitializeRectTransformPosition();
            animation.InitializeOffsetPosition();
        }
    }

    void Start()
    {
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

    #region StartScene - Start Screen and AssignTrainingValues Screen Tranisition Animations
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

        // Elements leaving screen
        Vector2 welcomeTitleTargetPos = welcomeTitleCurrPos + GetElementAnimation(welcomeTitle).GetLeaveOffset();
        Vector2 startButtonTargetPos = startButtonCurrPos + GetElementAnimation(startButton).GetLeaveOffset();
        Vector2 lowerButtonsTargetPos = lowerButtonsCurrPos + GetElementAnimation(lowerButtons).GetLeaveOffset();

        // Elements entering screen
        Vector2 passwordBoxTargetPos = passwordBoxCurrPos + GetElementAnimation(passwordBox).GetEnterOffset();
        Vector2 backButtonTargetPos = backButtonCurrPos + GetElementAnimation(backButton).GetEnterOffset();

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

        // Elements entering screen
        Vector2 welcomeTitleTargetPos = welcomeTitleCurrPos + GetElementAnimation(welcomeTitle).GetEnterOffset();
        Vector2 startButtonTargetPos = startButtonCurrPos + GetElementAnimation(startButton).GetEnterOffset();
        Vector2 lowerButtonsTargetPos = lowerButtonsCurrPos + GetElementAnimation(lowerButtons).GetEnterOffset();

        // Elements leaving screen
        Vector2 passwordBoxTargetPos = passwordBoxCurrPos + GetElementAnimation(passwordBox).GetLeaveOffset();
        Vector2 backButtonTargetPos = backButtonCurrPos + GetElementAnimation(backButton).GetLeaveOffset();

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

    public void AnimateElementEnable_CorrectPassword(GameObject passwordBox, GameObject trainingModeElements)
    {
        StartCoroutine(InsideOutPasswordBox_OutsideInTrainingModeElements(passwordBox, trainingModeElements));
    }

    private IEnumerator InsideOutPasswordBox_OutsideInTrainingModeElements(GameObject passwordBox, GameObject trainingModeElements)
    {
        RectTransform passwordBoxRect = passwordBox.GetComponent<RectTransform>();
        RectTransform trainingModeElementsRect = trainingModeElements.GetComponent<RectTransform>();

        CanvasGroup passwordBoxCanvasGrp = passwordBox.GetComponent<CanvasGroup>();
        CanvasGroup trainingModeElementsCanvasGrp = trainingModeElements.GetComponent<CanvasGroup>();

        Vector2 passwordBoxCurrPos = passwordBoxRect.anchoredPosition; 
        Vector2 trainingModeElementsCurrPos = trainingModeElementsRect.anchoredPosition; 

        // Elements leaving screen
        Vector2 passwordBoxTargetPos = passwordBoxCurrPos + GetElementAnimation(passwordBox).GetLeaveOffset();

        // Elements entering screen
        Vector2 trainingModeElementsTargetPos = trainingModeElementsCurrPos + GetElementAnimation(trainingModeElements).GetEnterOffset();

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

        // Elements entering screen
        Vector2 welcomeTitleTargetPos = welcomeTitleCurrPos + GetElementAnimation(welcomeTitle).GetEnterOffset();
        Vector2 startButtonTargetPos = startButtonCurrPos + GetElementAnimation(startButton).GetEnterOffset();
        Vector2 lowerButtonsTargetPos = lowerButtonsCurrPos + GetElementAnimation(lowerButtons).GetEnterOffset();

        // Elements leaving screen
        Vector2 backButtonTargetPos = backButtonCurrPos + GetElementAnimation(backButton).GetLeaveOffset();
        Vector2 trainingModeElementsTargetPos = trainingModeElementsCurrPos + GetElementAnimation(trainingModeElements).GetLeaveOffset();

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

    #region StartScene - Start Screen and Explain Screen Transition Animation

    public void AnimateScreenChange_StartToExplain(GameObject welcomeTitle, GameObject startButton, GameObject lowerButtons, GameObject explainTitle, GameObject explainTexts, GameObject textNavButtons, GameObject startScreen, GameObject explainScreen)
    {
        StartCoroutine(InsideOutStart_OutsideInExplain(welcomeTitle, startButton, lowerButtons, explainTitle, explainTexts, textNavButtons, startScreen, explainScreen));
    }

    public IEnumerator InsideOutStart_OutsideInExplain(GameObject welcomeTitle, GameObject startButton, GameObject lowerButtons, GameObject explainTitle, GameObject explainTexts, GameObject textNavButtons, GameObject startScreen, GameObject explainScreen)
    {
        RectTransform welcomeTitleRect = welcomeTitle.GetComponent<RectTransform>();
        RectTransform startButtonRect = startButton.GetComponent<RectTransform>();
        RectTransform lowerButtonsRect = lowerButtons.GetComponent<RectTransform>();
        RectTransform explainTitleRect = explainTitle.GetComponent<RectTransform>();
        RectTransform explainTextsRect = explainTexts.GetComponent<RectTransform>();
        RectTransform textNavButtonsRect = textNavButtons.GetComponent<RectTransform>();

        CanvasGroup welcomeTitleCanvasGrp = welcomeTitle.GetComponent<CanvasGroup>();
        CanvasGroup startButtonCanvasGrp = startButton.GetComponent<CanvasGroup>();
        CanvasGroup lowerButtonsCanvasGrp = lowerButtons.GetComponent<CanvasGroup>();
        CanvasGroup explainTitleCanvasGrp = explainTitle.GetComponent<CanvasGroup>();
        CanvasGroup explainTextsCanvasGrp = explainTexts.GetComponent<CanvasGroup>();
        CanvasGroup textNavButtonsCanvasGrp = textNavButtons.GetComponent<CanvasGroup>();
        
        Vector2 welcomeTitleCurrPos = welcomeTitleRect.anchoredPosition; 
        Vector2 startButtonCurrPos = startButtonRect.anchoredPosition; 
        Vector2 lowerButtonsCurrPos = lowerButtonsRect.anchoredPosition; 
        Vector2 explainTitleCurrPos = explainTitleRect.anchoredPosition; 
        Vector2 explainTextsCurrPos = explainTextsRect.anchoredPosition; 
        Vector2 textNavButtonsCurrPos = textNavButtonsRect.anchoredPosition; 

        // Elements leaving screen
        Vector2 welcomeTitleTargetPos = welcomeTitleCurrPos + GetElementAnimation(welcomeTitle).GetLeaveOffset();
        Vector2 startButtonTargetPos = startButtonCurrPos + GetElementAnimation(startButton).GetLeaveOffset();
        Vector2 lowerButtonsTargetPos = lowerButtonsCurrPos + GetElementAnimation(lowerButtons).GetLeaveOffset();

        // Elements entering screen
        Vector2 explainTitleTargetPos = explainTitleCurrPos + GetElementAnimation(explainTitle).GetEnterOffset();
        Vector2 explainTextsTargetPos = explainTextsCurrPos + GetElementAnimation(explainTexts).GetEnterOffset();
        Vector2 textNavButtonsTargetPos = textNavButtonsCurrPos + GetElementAnimation(textNavButtons).GetEnterOffset();
        
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
        explainScreen.SetActive(true);

        elapsedTime = 0f;

        while (elapsedTime < groupAnimationDuration)
        {
            float evaluationAtTime = groupEasingCurve.Evaluate(elapsedTime / groupAnimationDuration);

            explainTitleRect.anchoredPosition = Vector2.Lerp(explainTitleCurrPos, explainTitleTargetPos, evaluationAtTime);
            explainTitleCanvasGrp.alpha = Mathf.Lerp(0f, 1f, evaluationAtTime);
            
            explainTextsRect.anchoredPosition = Vector2.Lerp(explainTextsCurrPos, explainTextsTargetPos, evaluationAtTime);
            explainTextsCanvasGrp.alpha = Mathf.Lerp(0f, 1f, evaluationAtTime);

            textNavButtonsRect.anchoredPosition = Vector2.Lerp(textNavButtonsCurrPos, textNavButtonsTargetPos, evaluationAtTime);
            textNavButtonsCanvasGrp.alpha = Mathf.Lerp(0f, 1f, evaluationAtTime);

            elapsedTime += Time.deltaTime;
            yield return null;
        }

        explainTitleRect.anchoredPosition = explainTitleTargetPos;
        explainTitleCanvasGrp.alpha = 1;
        explainTitleCanvasGrp.interactable = true;
        explainTitleCanvasGrp.blocksRaycasts = true;
        
        explainTextsRect.anchoredPosition = explainTextsTargetPos;
        explainTextsCanvasGrp.alpha = 1;
        explainTextsCanvasGrp.interactable = true;
        explainTextsCanvasGrp.blocksRaycasts = true;
        
        textNavButtonsRect.anchoredPosition = textNavButtonsTargetPos;
        textNavButtonsCanvasGrp.alpha = 1;
        textNavButtonsCanvasGrp.interactable = true;
        textNavButtonsCanvasGrp.blocksRaycasts = true;
    }

    #endregion

    #region StartScene - Explain Screen Transition Animations
    public void AnimateScreenChange_ExplainEnd(GameObject explainTitle, GameObject explainTexts, GameObject textNavButtons, GameObject explainScreen)
    {
        StartCoroutine(InsideoutExplain(explainTitle, explainTexts, textNavButtons, explainScreen));
    }

    private IEnumerator InsideoutExplain(GameObject explainTitle, GameObject explainTexts, GameObject textNavButtons, GameObject explainScreen)
    {
        RectTransform explainTitleRect = explainTitle.GetComponent<RectTransform>();
        RectTransform explainTextsRect = explainTexts.GetComponent<RectTransform>();
        RectTransform textNavButtonsRect = textNavButtons.GetComponent<RectTransform>();

        CanvasGroup explainTitleCanvasGrp = explainTitle.GetComponent<CanvasGroup>();
        CanvasGroup explainTextsCanvasGrp = explainTexts.GetComponent<CanvasGroup>();
        CanvasGroup textNavButtonsCanvasGrp = textNavButtons.GetComponent<CanvasGroup>();
        
        Vector2 explainTitleCurrPos = explainTitleRect.anchoredPosition; 
        Vector2 explainTextsCurrPos = explainTextsRect.anchoredPosition; 
        Vector2 textNavButtonsCurrPos = textNavButtonsRect.anchoredPosition; 

        // Elements leaving screen
        Vector2 explainTitleTargetPos = explainTitleCurrPos + GetElementAnimation(explainTitle).GetLeaveOffset();
        Vector2 explainTextsTargetPos = explainTextsCurrPos + GetElementAnimation(explainTexts).GetLeaveOffset();
        Vector2 textNavButtonsTargetPos = textNavButtonsCurrPos + GetElementAnimation(textNavButtons).GetLeaveOffset();

        float elapsedTime = 0f;

        while (elapsedTime < groupAnimationDuration)
        {
            float evaluationAtTime = groupEasingCurve.Evaluate(elapsedTime / groupAnimationDuration);

            explainTitleRect.anchoredPosition = Vector2.Lerp(explainTitleCurrPos, explainTitleTargetPos, evaluationAtTime);
            explainTitleCanvasGrp.alpha = Mathf.Lerp(1f, 0f, evaluationAtTime);
            
            explainTextsRect.anchoredPosition = Vector2.Lerp(explainTextsCurrPos, explainTextsTargetPos, evaluationAtTime);
            explainTextsCanvasGrp.alpha = Mathf.Lerp(1f, 0f, evaluationAtTime);

            textNavButtonsRect.anchoredPosition = Vector2.Lerp(textNavButtonsCurrPos, textNavButtonsTargetPos, evaluationAtTime);
            textNavButtonsCanvasGrp.alpha = Mathf.Lerp(1f, 0f, evaluationAtTime);

            elapsedTime += Time.deltaTime;
            yield return null;
        }

        explainTitleRect.anchoredPosition = explainTitleTargetPos;
        explainTitleCanvasGrp.alpha = 0;
        explainTitleCanvasGrp.interactable = false;
        explainTitleCanvasGrp.blocksRaycasts = false;
        
        explainTextsRect.anchoredPosition = explainTextsTargetPos;
        explainTextsCanvasGrp.alpha = 0;
        explainTextsCanvasGrp.interactable = false;
        explainTextsCanvasGrp.blocksRaycasts = false;
        
        textNavButtonsRect.anchoredPosition = textNavButtonsTargetPos;
        textNavButtonsCanvasGrp.alpha = 0;
        textNavButtonsCanvasGrp.interactable = false;
        textNavButtonsCanvasGrp.blocksRaycasts = false;

        explainScreen.SetActive(false);

        SceneManager.LoadScene("TopicMenuScene");
        Debug.Log("Loading TopicMenuScene.");
    }

    public void AnimateElementChange_TextFadeForward(GameObject explainTexts, List<GameObject> texts,  GameObject textNavButtons)
    {
        StartCoroutine(TextFadeForward(explainTexts, texts, textNavButtons));
    }

    private IEnumerator TextFadeForward(GameObject explainTexts, List<GameObject> texts, GameObject textNavButtons)
    {
        if (CurrentTextIndex == texts.Count - 1)
        {
            Debug.Log("First exiting Coroutine");
            yield break;
        }

        CanvasGroup explainTextsCanvasGrp = explainTexts.GetComponent<CanvasGroup>();
        CanvasGroup textNavButtonsCanvasGrp = textNavButtons.GetComponent<CanvasGroup>();

        GameObject continueButton = textNavButtons.transform.Find("ContinueButton").gameObject;
        GameObject backButton = textNavButtons.transform.Find("BackButton").gameObject;

        textNavButtonsCanvasGrp.interactable = false;
        textNavButtonsCanvasGrp.blocksRaycasts = false;
        
        float elapsedTime = 0f;

        while (elapsedTime < groupAnimationDuration)
        {
            float evaluationAtTime = groupEasingCurve.Evaluate(elapsedTime / groupAnimationDuration);

            explainTextsCanvasGrp.alpha = Mathf.Lerp(1f, 0f, evaluationAtTime);

            elapsedTime += Time.deltaTime;
            yield return null;
        }
        explainTextsCanvasGrp.alpha = 0;

        texts[CurrentTextIndex].SetActive(false);

        CurrentTextIndex++;

        // If on second text => Translate continueButton to the right and enable backButton
        if (CurrentTextIndex == 1)
        {
            var newPos = continueButton.GetComponent<RectTransform>().anchoredPosition;
            newPos.x += 60;
            continueButton.GetComponent<RectTransform>().anchoredPosition = newPos;
            backButton.SetActive(true);
        }
        
        texts[CurrentTextIndex].SetActive(true);

        elapsedTime = 0f;

        while (elapsedTime < groupAnimationDuration)
        {
            float evaluationAtTime = groupEasingCurve.Evaluate(elapsedTime / groupAnimationDuration);

            explainTextsCanvasGrp.alpha = Mathf.Lerp(0f, 1f, evaluationAtTime);

            elapsedTime += Time.deltaTime;
            yield return null;
        }
        explainTextsCanvasGrp.alpha = 1;

        textNavButtonsCanvasGrp.interactable = true;
        textNavButtonsCanvasGrp.blocksRaycasts = true;

    }

    public void AnimateElementChange_TextFadeBackward(GameObject explainTexts, List<GameObject> texts,  GameObject textNavButtons)
    {
        StartCoroutine(TextFadeBackward(explainTexts, texts, textNavButtons));
    }

    private IEnumerator TextFadeBackward(GameObject explainTexts, List<GameObject> texts, GameObject textNavButtons)
    {
        CanvasGroup explainTextsCanvasGrp = explainTexts.GetComponent<CanvasGroup>();
        CanvasGroup textNavButtonsCanvasGrp = textNavButtons.GetComponent<CanvasGroup>();

        GameObject continueButton = textNavButtons.transform.Find("ContinueButton").gameObject;
        GameObject backButton = textNavButtons.transform.Find("BackButton").gameObject;

        textNavButtonsCanvasGrp.interactable = false;
        textNavButtonsCanvasGrp.blocksRaycasts = false;

        float elapsedTime = 0f;

        while (elapsedTime < groupAnimationDuration)
        {
            float evaluationAtTime = groupEasingCurve.Evaluate(elapsedTime / groupAnimationDuration);

            explainTextsCanvasGrp.alpha = Mathf.Lerp(1f, 0f, evaluationAtTime);

            elapsedTime += Time.deltaTime;
            yield return null;
        }
        explainTextsCanvasGrp.alpha = 0;

        texts[CurrentTextIndex].SetActive(false);

        CurrentTextIndex--;

        // If on first text again => Reset continueButton to center and disable backButton
        if (CurrentTextIndex == 0)
        {
            var newPos = continueButton.GetComponent<RectTransform>().anchoredPosition;
            newPos.x = 0;
            continueButton.GetComponent<RectTransform>().anchoredPosition = newPos;
            backButton.SetActive(false);
        }
        
        texts[CurrentTextIndex].SetActive(true);

        elapsedTime = 0f;

        while (elapsedTime < groupAnimationDuration)
        {
            float evaluationAtTime = groupEasingCurve.Evaluate(elapsedTime / groupAnimationDuration);

            explainTextsCanvasGrp.alpha = Mathf.Lerp(0f, 1f, evaluationAtTime);

            elapsedTime += Time.deltaTime;
            yield return null;
        }
        explainTextsCanvasGrp.alpha = 1;

        textNavButtonsCanvasGrp.interactable = true;
        textNavButtonsCanvasGrp.blocksRaycasts = true;
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

    #endregion
}
