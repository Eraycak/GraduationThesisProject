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
    }
}
