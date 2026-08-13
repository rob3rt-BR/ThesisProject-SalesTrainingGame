using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.UIElements;

public class ExplainScreenButton : MonoBehaviour
{
    [SerializeField]
    private GameObject[] texts;
    [SerializeField]
    private GameObject continueButton;
    [SerializeField]
    private GameObject backButton;
    private int index = 0;
    private RectTransform continueButtonTrans;

    void Start()
    {
        continueButtonTrans = continueButton.GetComponent<RectTransform>();
    }

    public void ContinuePressed()
    {
        texts[index].SetActive(false);
        index++;
        if (index == 1)
        {
            var newPos = continueButtonTrans.anchoredPosition;
            newPos.x += 130;
            continueButtonTrans.anchoredPosition = newPos;
            backButton.SetActive(true);
        }
        if (index == texts.Length)
        {
            continueButton.SetActive(false);
            backButton.SetActive(false);
            Debug.Log("Load TopicMenuScene.");
            return;
        }
        texts[index].SetActive(true);
    }

    public void BackPressed()
    {
        texts[index].SetActive(false);
        index--;
        if (index == 0)
        {
            var newPos = continueButtonTrans.anchoredPosition;
            newPos.x = 0;
            continueButtonTrans.anchoredPosition = newPos;
            backButton.SetActive(false);
        }
        texts[index].SetActive(true);
    }

}
