using UnityEngine;

public class StartScreenButton : MonoBehaviour
{
    [SerializeField]
    private GameObject startScreen;
    [SerializeField]
    private GameObject explainScreen;

    [SerializeField]
    private ParticipantSetupController participantController; // Ahorita solo para testing

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        participantController.BeginParticipantSession("P_115");
    }
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
