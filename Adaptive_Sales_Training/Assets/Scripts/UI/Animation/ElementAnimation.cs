using System.Collections;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class ElementAnimation : MonoBehaviour
{
    [Header("Element Setup")]
    [Space]
    [SerializeField]
    private bool defineInitPosition;
    [SerializeField]
    private Vector2 initialOffsetPosition = new(0, 0);

    [HideInInspector]
    public RectTransform rectTransform;
    [HideInInspector]
    public CanvasGroup canvasGroup;
    [HideInInspector]
    public Vector2 enterPosition;
    [HideInInspector]
    public Vector2 leavePosition;
    [HideInInspector]
    public Vector2 screenPosition;

    private enum AnimateToDirection
    {
        Top,
        Bottom,
        Left,
        Right
    }
    
    [Header("Animation Setup")]
    [Space]
    [SerializeField]
    private AnimateToDirection enterDirection;
    [SerializeField]
    private AnimateToDirection leaveDirection;
    [SerializeField]
    private Vector2 distanceToAnimate = new(100, 100);
    public AnimationCurve easingCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);
    [Range(0, 1f)]
    public float animationDuration = 0.5f;

    private Vector2 topOffset;
    private Vector2 bottomOffset;
    private Vector2 leftOffset;
    private Vector2 rightOffset;


    public void InitializeElement()
    {
        rectTransform = gameObject.GetComponent<RectTransform>();

        /*
        if (defineInitPosition)
        {
            rectTransform.anchoredPosition += initialOffsetPosition;
        }*/

        canvasGroup = gameObject.GetComponent<CanvasGroup>();
    }

    public void InitializeOffsetPosition()
    {
        topOffset = new (0, distanceToAnimate.y);
        bottomOffset = new (0, -distanceToAnimate.y);
        leftOffset = new (-distanceToAnimate.x , 0);
        rightOffset = new (distanceToAnimate.x, 0);
    }

    public Vector2 GetEnterOffset()
    {
        return enterDirection switch
        {
            AnimateToDirection.Top => topOffset,
            AnimateToDirection.Bottom => bottomOffset,
            AnimateToDirection.Left => leftOffset,
            AnimateToDirection.Right => rightOffset,
            _ => Vector2.zero,
        };
    }
    public Vector2 GetLeaveOffset()
    {
        return leaveDirection switch
        {
            AnimateToDirection.Top => topOffset,
            AnimateToDirection.Bottom => bottomOffset,
            AnimateToDirection.Left => leftOffset,
            AnimateToDirection.Right => rightOffset,
            _ => Vector2.zero,
        };
    }
}
