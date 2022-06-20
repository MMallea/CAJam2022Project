using FishNet;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace WV
{
    public class ClientServerManager : MonoBehaviour
    {
        [SerializeField] private bool isServer;
        //[SerializeField] private bool isHost;

        private void Awake()
        {
            //if (isHost)
            //{
            //    InstanceFinder.ServerManager.StartConnection();
            //    InstanceFinder.ClientManager.StartConnection();
            //}
            //else
            //    return;

            if (isServer)
                InstanceFinder.ServerManager.StartConnection();
            else
                InstanceFinder.ClientManager.StartConnection();
        }
    }
}
