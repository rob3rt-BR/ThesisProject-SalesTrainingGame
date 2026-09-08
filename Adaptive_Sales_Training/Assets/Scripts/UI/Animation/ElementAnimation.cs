using System.Collections;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class ElementAnimation : MonoBehaviour
{
    private RectTransform elementRectTransform;
    private CanvasGroup elementCanvasGroup;

    [Header("Element Setup")]
    [SerializeField]
    private bool defineInitPosition;
    [SerializeField]
    private Vector2 extraInitialPosition = new(0, 0);
    
    [Header("Animation Setup")]
    [SerializeField]
    private Vector2 distanceToAnimate = new(100, 100);
    public AnimationCurve easingCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);
    [Range(0, 1f)]
    public float animationDuration = 0.5f;

    private bool isMovingInsideOut;
    private Vector2 initialPosition;
    private Vector2 currentPosition;

    private Vector2 topOffset;
    private Vector2 bottomOffset;
    private Vector2 leftOffset;
    private Vector2 rightOffset;
    private Coroutine animateElementCoroutine;

    private AnimateToDirection startDirection;
    private AnimateToDirection endDirection;
    private bool isMoving;


    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        InitializeRectTransformPosition();
        InitializeOffsetPosition();
    }

    private void InitializeRectTransformPosition()
    {
        elementRectTransform = gameObject.GetComponent<RectTransform>();
        if (defineInitPosition)
        {
            elementRectTransform.anchoredPosition += extraInitialPosition;
        }
    }

    private void InitializeOffsetPosition()
    {
        topOffset = new (0, distanceToAnimate.y);
        bottomOffset = new (0, -distanceToAnimate.y);
        leftOffset = new (-distanceToAnimate.x , 0);
        rightOffset = new (distanceToAnimate.x, 0);
    }

    public Vector2 GetOffset(AnimateToDirection direction)
    {
        return direction switch
        {
            AnimateToDirection.Top => topOffset,
            AnimateToDirection.Bottom => bottomOffset,
            AnimateToDirection.Left => leftOffset,
            AnimateToDirection.Right => rightOffset,
            _ => Vector2.zero,
        };
    }

    public void MoveElementInsideOut(AnimateToDirection direction)
    {
        if (isMovingInsideOut)
        {
            return;
        }

        isMovingInsideOut = true;

        if (animateElementCoroutine != null)
        {
            StopCoroutine(animateElementCoroutine);
        }

        animateElementCoroutine = StartCoroutine(AnimateElementMovement(true, direction));
    }

    public void MoveElementInsideOutAndChangeScreen(AnimateToDirection direction, GameObject currentScreen, GameObject nextScreen)
    {
        if (isMovingInsideOut)
        {
            return;
        }

        isMovingInsideOut = true;

        if (animateElementCoroutine != null)
        {
            StopCoroutine(animateElementCoroutine);
        }

        animateElementCoroutine = StartCoroutine(AnimateElementMovementAndChangeScreen(true, direction, currentScreen, nextScreen));
    }

    public void MoveElementOutsideIn(AnimateToDirection direction)
    {
        if (!isMovingInsideOut)
        {
            return;
        }

        isMovingInsideOut = false;

        gameObject.SetActive(true);

        if (animateElementCoroutine != null)
        {
            StopCoroutine(animateElementCoroutine);
        }

        animateElementCoroutine = StartCoroutine(AnimateElementMovement(false, direction));
    }

    public void MoveElementOutsideInAndChangeScreen(AnimateToDirection direction, GameObject currentScreen, GameObject nextScreen)
    {
        if (!isMovingInsideOut)
        {
            return;
        }

        isMovingInsideOut = false;

        currentScreen.SetActive(false);
        nextScreen.SetActive(true);

        gameObject.SetActive(true);

        if (animateElementCoroutine != null)
        {
            StopCoroutine(animateElementCoroutine);
        }

        animateElementCoroutine = StartCoroutine(AnimateElementMovementAndChangeScreen(false, direction, currentScreen, nextScreen));
    }

    private IEnumerator AnimateElementMovement(bool insideOut, AnimateToDirection direction)
    {
        currentPosition = initialPosition; 

        Vector2 targetPosition = currentPosition + GetOffset(direction);

        float elapsedTime = 0f;

        while (elapsedTime < animationDuration)
        {
            float evaluationAtTime = easingCurve.Evaluate(elapsedTime / animationDuration);

            elementRectTransform.anchoredPosition = Vector2.Lerp(currentPosition, targetPosition, evaluationAtTime);

            elementCanvasGroup.alpha = insideOut ? Mathf.Lerp(1f, 0f, evaluationAtTime) : Mathf.Lerp(0f, 1f, evaluationAtTime);

            elapsedTime += Time.deltaTime;
            yield return null;
        }

        elementRectTransform.anchoredPosition = targetPosition;
        initialPosition = targetPosition;

        elementCanvasGroup.alpha = insideOut ? 0 : 1;
        elementCanvasGroup.interactable = !insideOut;
        elementCanvasGroup.blocksRaycasts = !insideOut;

        if (insideOut)
        {
            gameObject.SetActive(false);
        }
    }

    private IEnumerator AnimateElementMovementAndChangeScreen(bool insideOut, AnimateToDirection direction, GameObject currentScreen, GameObject nextScreen)
    {
        currentPosition = initialPosition; 

        Vector2 targetPosition = currentPosition + GetOffset(direction);

        float elapsedTime = 0f;

        while (elapsedTime < animationDuration)
        {
            float evaluationAtTime = easingCurve.Evaluate(elapsedTime / animationDuration);

            elementRectTransform.anchoredPosition = Vector2.Lerp(currentPosition, targetPosition, evaluationAtTime);

            elementCanvasGroup.alpha = insideOut ? Mathf.Lerp(1f, 0f, evaluationAtTime) : Mathf.Lerp(0f, 1f, evaluationAtTime);

            elapsedTime += Time.deltaTime;
            yield return null;
        }

        elementRectTransform.anchoredPosition = targetPosition;
        initialPosition = targetPosition;

        elementCanvasGroup.alpha = insideOut ? 0 : 1;
        elementCanvasGroup.interactable = !insideOut;
        elementCanvasGroup.blocksRaycasts = !insideOut;

        if (insideOut)
        {
            gameObject.SetActive(false);

            currentScreen.SetActive(false);
            nextScreen.SetActive(true);
        }
    }

    // |------------ Reference Code ------------|

    public void StartElementMovement()
    {
        if (isMoving)
        {
            return;
        }

        isMoving = true;

        if (animateElementCoroutine != null)
        {
            StopCoroutine(animateElementCoroutine);
        }

        animateElementCoroutine = StartCoroutine(AnimateElementMovementDef(true));
    }

    public void EndElementMovement()
    {
        if (!isMoving)
        {
            return;
        }

        isMoving = false;

        if (animateElementCoroutine != null)
        {
            StopCoroutine(animateElementCoroutine);
        }

        animateElementCoroutine = StartCoroutine(AnimateElementMovementDef(false));
    }

    private IEnumerator AnimateElementMovementDef(bool start)
    {
        if (start)
        {
            gameObject.SetActive(true);
        }

        currentPosition = elementRectTransform.anchoredPosition;

        Vector2 targetPosition;
        if (start)
        {
            targetPosition = currentPosition + GetOffset(startDirection);
        } else
        {
            targetPosition = currentPosition + GetOffset(endDirection);
        }

        float elapsedTime = 0f;

        while (elapsedTime < animationDuration)
        {
            float evaluationAtTime = easingCurve.Evaluate(elapsedTime / animationDuration);

            elementRectTransform.anchoredPosition = Vector2.Lerp(currentPosition, targetPosition, evaluationAtTime);

            elementCanvasGroup.alpha = start ? Mathf.Lerp(0f, 1f, evaluationAtTime) : Mathf.Lerp(1f, 0f, evaluationAtTime);

            elapsedTime += Time.deltaTime;
            yield return null;
        }

        elementRectTransform.anchoredPosition = targetPosition;

        elementCanvasGroup.alpha = start ? 1 : 0;
        elementCanvasGroup.interactable = start;
        elementCanvasGroup.blocksRaycasts = start;

        if (!start)
        {
            gameObject.SetActive(false);
            elementRectTransform.anchoredPosition = initialPosition;
        }
    }
}
