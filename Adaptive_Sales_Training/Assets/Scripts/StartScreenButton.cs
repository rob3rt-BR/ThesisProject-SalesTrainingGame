using UnityEngine;

public class StartScreenButton : MonoBehaviour
{
    [SerializeField]
    private GameObject startScreen;
    [SerializeField]
    private GameObject explainScreen;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    
    public void StartGameNormal()
    {
        startScreen.SetActive(false);
        explainScreen.SetActive(true);
    }

    public void QuitGame()
    {
        Debug.Log("Game exited.");
        Application.Quit();
    }
}
