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

    private ElementAnimation GetElementAnimation(GameObject element)
    {
        if (animations == null || animations[element] == null)
        {
            Debug.LogError("No animation entry for this element name");
            return null;
        }
        return animations[element];
    }

    public void AnimateScreenChange_StartToAssignTrainingMode(GameObject welcomeTitle, GameObject startButton, GameObject lowerButtons, GameObject passwordBox, GameObject backButton, GameObject startScreen, GameObject assignTrainingModeScreen)
    {
        StartCoroutine(InsideOutStart_OutsideInAssignTrainingMode(welcomeTitle, startButton, lowerButtons, passwordBox, backButton, startScreen, assignTrainingModeScreen));
    }

    private IEnumerator InsideOutStart_OutsideInAssignTrainingMode(GameObject welcomeTitle, GameObject startButton, GameObject lowerButtons, GameObject passwordBox, GameObject backButton, GameObject startScreen, GameObject assignTrainingModeScreen)
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
            startButtonRect.anchoredPosition = Vector2.Lerp(startButtonCurrPos, startButtonTargetPos, evaluationAtTime);
            lowerButtonsRect.anchoredPosition = Vector2.Lerp(lowerButtonsCurrPos, lowerButtonsTargetPos, evaluationAtTime);

            welcomeTitleCanvasGrp.alpha = Mathf.Lerp(1f, 0f, evaluationAtTime);
            startButtonCanvasGrp.alpha = Mathf.Lerp(1f, 0f, evaluationAtTime);
            lowerButtonsCanvasGrp.alpha = Mathf.Lerp(1f, 0f, evaluationAtTime);

            elapsedTime += Time.deltaTime;
            yield return null;
        }

        welcomeTitleRect.anchoredPosition = welcomeTitleTargetPos;
        startButtonRect.anchoredPosition = startButtonTargetPos;
        lowerButtonsRect.anchoredPosition = lowerButtonsTargetPos;

        welcomeTitleCanvasGrp.alpha = 0;
        startButtonCanvasGrp.alpha = 0;
        lowerButtonsCanvasGrp.alpha = 0;

        welcomeTitleCanvasGrp.interactable = false;
        startButtonCanvasGrp.interactable = false;
        lowerButtonsCanvasGrp.interactable = false;

        welcomeTitleCanvasGrp.blocksRaycasts = false;
        startButtonCanvasGrp.blocksRaycasts = false;
        lowerButtonsCanvasGrp.blocksRaycasts = false;

        startScreen.SetActive(false);
        assignTrainingModeScreen.SetActive(true);

        elapsedTime = 0f;

        while (elapsedTime < groupAnimationDuration)
        {
            float evaluationAtTime = groupEasingCurve.Evaluate(elapsedTime / groupAnimationDuration);
            
            passwordBoxRect.anchoredPosition = Vector2.Lerp(passwordBoxCurrPos, passwordBoxTargetPos, evaluationAtTime);
            backButtonRect.anchoredPosition = Vector2.Lerp(backButtonCurrPos, backButtonTargetPos, evaluationAtTime);

            passwordBoxCanvasGrp.alpha = Mathf.Lerp(0f, 1f, evaluationAtTime);
            backButtonCanvasGrp.alpha = Mathf.Lerp(0f, 1f, evaluationAtTime);

            elapsedTime += Time.deltaTime;
            yield return null;
        }

        passwordBoxRect.anchoredPosition = passwordBoxTargetPos;
        backButtonRect.anchoredPosition = backButtonTargetPos;

        passwordBoxCanvasGrp.alpha = 1;
        backButtonCanvasGrp.alpha = 1;

        passwordBoxCanvasGrp.interactable = true;
        backButtonCanvasGrp.interactable = true;

        passwordBoxCanvasGrp.blocksRaycasts = true;
        backButtonCanvasGrp.blocksRaycasts = true;
    }

    public void AnimateScreenChange_AssignTrainingModeToStart(GameObject welcomeTitle, GameObject startButton, GameObject lowerButtons, GameObject passwordBox, GameObject backButton, GameObject startScreen, GameObject assignTrainingModeScreen)
    {
        StartCoroutine(InsideOutAssignTrainingMode_OutsideInStart(welcomeTitle, startButton, lowerButtons, passwordBox, backButton, startScreen, assignTrainingModeScreen));
    }

    private IEnumerator InsideOutAssignTrainingMode_OutsideInStart(GameObject welcomeTitle, GameObject startButton, GameObject lowerButtons, GameObject passwordBox, GameObject backButton, GameObject startScreen, GameObject assignTrainingModeScreen)
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
            backButtonRect.anchoredPosition = Vector2.Lerp(backButtonCurrPos, backButtonTargetPos, evaluationAtTime);

            passwordBoxCanvasGrp.alpha = Mathf.Lerp(1f, 0f, evaluationAtTime);
            backButtonCanvasGrp.alpha = Mathf.Lerp(1f, 0f, evaluationAtTime);

            elapsedTime += Time.deltaTime;
            yield return null;
        }

        passwordBoxRect.anchoredPosition = passwordBoxTargetPos;
        backButtonRect.anchoredPosition = backButtonTargetPos;

        passwordBoxCanvasGrp.alpha = 0;
        backButtonCanvasGrp.alpha = 0;

        passwordBoxCanvasGrp.interactable = false;
        backButtonCanvasGrp.interactable = false;

        passwordBoxCanvasGrp.blocksRaycasts = false;
        backButtonCanvasGrp.blocksRaycasts = false;

        assignTrainingModeScreen.SetActive(false);
        startScreen.SetActive(true);

        elapsedTime = 0f;

        while (elapsedTime < groupAnimationDuration)
        {
            float evaluationAtTime = groupEasingCurve.Evaluate(elapsedTime / groupAnimationDuration);

            welcomeTitleRect.anchoredPosition = Vector2.Lerp(welcomeTitleCurrPos, welcomeTitleTargetPos, evaluationAtTime);
            startButtonRect.anchoredPosition = Vector2.Lerp(startButtonCurrPos, startButtonTargetPos, evaluationAtTime);
            lowerButtonsRect.anchoredPosition = Vector2.Lerp(lowerButtonsCurrPos, lowerButtonsTargetPos, evaluationAtTime);

            welcomeTitleCanvasGrp.alpha = Mathf.Lerp(0f, 1f, evaluationAtTime);
            startButtonCanvasGrp.alpha = Mathf.Lerp(0f, 1f, evaluationAtTime);
            lowerButtonsCanvasGrp.alpha = Mathf.Lerp(0f, 1f, evaluationAtTime);

            elapsedTime += Time.deltaTime;
            yield return null;
        }

        welcomeTitleRect.anchoredPosition = welcomeTitleTargetPos;
        startButtonRect.anchoredPosition = startButtonTargetPos;
        lowerButtonsRect.anchoredPosition = lowerButtonsTargetPos;

        welcomeTitleCanvasGrp.alpha = 1;
        startButtonCanvasGrp.alpha = 1;
        lowerButtonsCanvasGrp.alpha = 1;

        welcomeTitleCanvasGrp.interactable = true;
        startButtonCanvasGrp.interactable = true;
        lowerButtonsCanvasGrp.interactable =true;

        welcomeTitleCanvasGrp.blocksRaycasts = true;
        startButtonCanvasGrp.blocksRaycasts = true;
        lowerButtonsCanvasGrp.blocksRaycasts = true;
    }
}
