using FishNet;
using FishNet.Transporting.Tugboat;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MultiplayerMenu : MonoBehaviour
{
    public Tugboat tugBoat;

    [SerializeField]
    private Button hostButton;

    [SerializeField]
    private Button connectButton;

    [SerializeField]
    private Button quitButton;

    private void Start()
    {
        #if UNITY_EDITOR
        hostButton.onClick.AddListener(() =>
        {
            if(tugBoat)
            {
                tugBoat.SetPort(8080);
                tugBoat.SetClientAddress("localhost");
            }

            InstanceFinder.ServerManager.StartConnection();
            InstanceFinder.ClientManager.StartConnection();
        });
        hostButton.gameObject.SetActive(true);
        #endif

        connectButton.onClick.AddListener(() => InstanceFinder.ClientManager.StartConnection());
        quitButton.onClick.AddListener(() => Application.Quit());
    }
}
