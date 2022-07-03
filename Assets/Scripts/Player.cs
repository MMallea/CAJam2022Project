using FishNet.Connection;
using FishNet.Object;
using FishNet.Object.Synchronizing;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;

public class Player : NetworkBehaviour
{
    public static Player Instance { get; private set; }

    #region Public.
    [SyncVar]
    public bool isReady;
    [SyncVar]
    public int lives = 9;
    [SyncVar]
    public string username;
    [SyncVar]
    public Game_CharacterController controlledCharacter;

    public GameObject controllerPrefab;
    #endregion

    public override void OnStartClient()
    {
        base.OnStartClient();

        if (!IsOwner) return;

        Instance = this;

        UIManager.Instance.Initialize();
        UIManager.Instance.Show<LobbyView>();
    }

    public override void OnStartServer()
    {
        base.OnStartServer();

        GameManager.Instance.players.Add(this);
        username = GameManager.Instance.GetDefaultUsername();
    }

    public override void OnStopServer()
    {
        base.OnStopServer();

        GameManager.Instance.players.Remove(this);
    }

    // Update is called once per frame
    void Update()
    {
        if (!IsOwner) return;

        //Set server is ready
    }

    public void StartGame()
    {
        Debug.Log("Player Starting To Create Controller");
        //GameObject controllerPrefab = Addressables.LoadAssetAsync<GameObject>("PlayerController").WaitForCompletion();

        GameObject controllerInstance = Instantiate(controllerPrefab);

        Spawn(controllerInstance, Owner);

        controlledCharacter = controllerInstance.GetComponent<Game_CharacterController>();

        controlledCharacter.controllingPlayer = this;

        TargetPawnSpawned(Owner);
    }

    public void StopGame()
    {
        if (controlledCharacter != null && controlledCharacter.IsSpawned) controlledCharacter.Despawn();
    }

    [ServerRpc(RequireOwnership = false)]
    public void ServerSpawnController()
    {
        StartGame();
    }

    [ServerRpc(RequireOwnership = false)]
    public void ServerSetIsReady(bool value)
    {
        isReady = value;
    }

    [TargetRpc]
    public void TargetControllerKilled(NetworkConnection networkConnection)
    {
        lives--;
        if(lives <= 0)
        {
            UIManager.Instance.Show<GameOverView>();
        } else
        {
            UIManager.Instance.Show<RespawnView>();
        }
    }

    [TargetRpc]
    private void TargetPawnSpawned(NetworkConnection networkConnection)
    {
        UIManager.Instance.Show<MainView>();
    }
}
