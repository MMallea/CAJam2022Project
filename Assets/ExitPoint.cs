using FishNet.Object;
using FishNet.Object.Synchronizing;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class ExitPoint : NetworkBehaviour
{
    [SyncVar]
    public bool exitActive;
    public bool exitActivePrev;

    public Light exitLight;
    public ParticleSystem exitParticles;
    public CapsuleCollider exitCollider;
    public Canvas exitCanvas;
    public TMPro.TextMeshProUGUI exitText;

    public UnityEvent onExitComplete;
    private int numberOfExitingPlayers = 0;

    private void Start()
    {
        UpdateExitText();
    }

    private void Update()
    {
        if(numberOfExitingPlayers == GameManager.Instance.players.Count)
        {
            //Exit and end game
            UIManager.Instance.Show<GameOverView>();
            GameManager.Instance.StopGame();
        }

        if(exitActive != exitActivePrev)
        {
            SetExitShown(exitActive);
            exitActivePrev = exitActive;
        }
    }

    public void SetExitShown(bool enabled)
    {
        if (exitCollider) exitCollider.enabled = enabled;
        if (exitLight) exitLight.enabled = enabled;
        if (exitCanvas) exitCanvas.enabled = enabled;
        if (exitParticles)
        {
            if (enabled)
                exitParticles.Play();
            else
                exitParticles.Stop();
        }
    }

    private void UpdateExitText()
    {
        if (exitText == null) return;

        exitText.text = numberOfExitingPlayers + "/" + GameManager.Instance.players.Count + " Ready To Exit";
    }

    private void OnTriggerEnter(Collider coll)
    {
        if (coll.gameObject.layer == LayerMask.NameToLayer("Player"))
        {
            numberOfExitingPlayers++;
        }
    }

    private void OnTriggerExit(Collider coll)
    {
        if (coll.gameObject.layer == LayerMask.NameToLayer("Player"))
        {
            numberOfExitingPlayers--;
        }
    }
}