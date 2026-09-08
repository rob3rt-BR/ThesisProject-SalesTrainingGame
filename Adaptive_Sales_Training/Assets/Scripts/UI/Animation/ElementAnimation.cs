using System.Collections;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class ElementAnimation : MonoBehaviour
{
    private RectTransform elementRectTransform;

    [Header("Element Setup")]
    [Space]
    [SerializeField]
    private bool defineInitPosition;
    [SerializeField]
    private Vector2 extraInitialPosition = new(0, 0);
    
    [Header("Animation Setup")]
    [Space]
    [SerializeField]
    private Vector2 distanceToAnimate = new(100, 100);
    public AnimationCurve easingCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);
    [Range(0, 1f)]
    public float animationDuration = 0.5f;

    private Vector2 topOffset;
    private Vector2 bottomOffset;
    private Vector2 leftOffset;
    private Vector2 rightOffset;


    public void InitializeRectTransformPosition()
    {
        elementRectTransform = gameObject.GetComponent<RectTransform>();
        if (defineInitPosition)
        {
            elementRectTransform.anchoredPosition += extraInitialPosition;
        }
    }

    public void InitializeOffsetPosition()
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

    // |------------ Reference Code ------------|

    private CanvasGroup elementCanvasGroup;
    private AnimateToDirection startDirection;
    private AnimateToDirection endDirection;
    private Vector2 initialPosition;
    private Vector2 currentPosition;
    private bool isMoving;
    private Coroutine animateElementCoroutine;

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
