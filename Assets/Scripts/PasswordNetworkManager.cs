using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;
using System.Text;
using UnityEngine.UI;
using System;

namespace UMT.ConnectionApproval
{
    public class PasswordNetworkManager : MonoBehaviour
    {
        [SerializeField] private InputField passwordInputField;
        [SerializeField] private GameObject passwordEntryUI;
        [SerializeField] private GameObject leaveButton;

        private void Start ()
        {
            NetworkManager.Singleton.OnServerStarted += HandleServerStarted;
            NetworkManager.Singleton.OnClientConnectedCallback += HandleClientConnected;
            NetworkManager.Singleton.OnClientDisconnectCallback += HandleClientDisconnected;
        }

        private void OnDestroy()
        {
            if(NetworkManager.Singleton != null) { return; }

            NetworkManager.Singleton.OnServerStarted -= HandleServerStarted;
            NetworkManager.Singleton.OnClientConnectedCallback -= HandleClientConnected;
            NetworkManager.Singleton.OnClientDisconnectCallback -= HandleClientDisconnected;
        }
        
        public void Host()
        {
            NetworkManager.Singleton.ConnectionApprovalCallback += ApprovalCheck;
            NetworkManager.Singleton.StartHost();
        }

        public void Client()
        {
            NetworkManager.Singleton.NetworkConfig.ConnectionData=Encoding.ASCII.GetBytes(passwordInputField.text);
            NetworkManager.Singleton.StartClient();
        }

        public void Leave()
        {
            if (NetworkManager.Singleton.IsHost)
            {
                NetworkManager.Singleton.Shutdown();
            }
        }

        private void ApprovalCheck(NetworkManager.ConnectionApprovalRequest request, NetworkManager.ConnectionApprovalResponse response)
        {
            string password = Encoding.ASCII.GetString(request.Payload);
            bool approveConnection = password == passwordInputField.text;
            if (approveConnection)
            {
                response.Approved = true;
                response.CreatePlayerObject = true;
                response.PlayerPrefabHash = null;
                response.Position = Vector3.zero;
                response.Rotation = Quaternion.identity;
            }
            else
            {
                response.Approved= false;
                response.Reason = "Wrong Password";
            }
        }

        private void HandleServerStarted()
        {
            if (NetworkManager.Singleton.IsHost)
            {
                HandleClientConnected(NetworkManager.Singleton.LocalClientId);
            }
        }

        private void HandleClientConnected(ulong clientId)
        {
            if (clientId == NetworkManager.Singleton.LocalClientId)
            {
                passwordEntryUI.gameObject.SetActive(false);
                leaveButton.gameObject.SetActive(true);
            }
        }

        private void HandleClientDisconnected(ulong clientId)
        {
            if (clientId == NetworkManager.Singleton.LocalClientId)
            {
                passwordEntryUI.gameObject.SetActive(true);
                leaveButton.gameObject.SetActive(false);
            }
        }
    }
}
