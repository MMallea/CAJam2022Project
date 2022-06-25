using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using FishNet.Object;
using FishNet.Object.Synchronizing;
using System.Linq;

public class GameManager : NetworkBehaviour
{
    public static GameManager Instance { get; private set; }

    [SyncObject]
    public readonly SyncList<Player> players = new();

    [SyncVar]
    public bool canStart;

    // Start is called before the first frame update
    void Awake()
    {
        Instance = this;
    }

    // Update is called once per frame
    void Update()
    {
        if (!IsServer) return;

        canStart = players.All(players => players.isReady);
    }

    [Server]
    public void StartGame()
    {
        if (!canStart) return;

        for(int i = 0; i < players.Count; i++)
        {
            players[i].StartGame();
        }
    }

    [Server]
    public void StopGame()
    {
        for (int i = 0; i < players.Count; i++)
        {
            players[i].StopGame();
        }
    }
}
