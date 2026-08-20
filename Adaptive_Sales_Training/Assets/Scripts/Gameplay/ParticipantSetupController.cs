using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class ParticipantSetupController : MonoBehaviour
{
    [SerializeField]
    private List<SalesTopic> allTopics;

    private readonly TrainingSessionManager session = TrainingSessionManager.Instance;

    // TODO: Configurar donde en secuencia de escenas hay que set id y training version
    public void BeginParticipantSession(string participantId)
    {
        // Ahorita solo para testing - Initiate session and respective values
        session.StartSession(participantId, TrainingVersion.Traditional, allTopics);

        // SceneManager.LoadScene("TopicMenuScene");
    }
}
