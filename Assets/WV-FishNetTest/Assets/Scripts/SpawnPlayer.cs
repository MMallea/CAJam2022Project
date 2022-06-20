using FishNet.Connection;
using FishNet.Object;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace WV
{
    public class SpawnPlayer : NetworkBehaviour
    {
        [SerializeField] private GameObject playerPrefab;

        public override void OnStartClient()
        {
            base.OnStartClient();

            PlayerSpawn();
        }

        [ServerRpc(RequireOwnership = false)]
        private void PlayerSpawn(NetworkConnection client = null)
        {
            GameObject gameObject = Instantiate(playerPrefab);

            Spawn(gameObject, client);
        }
    }
}