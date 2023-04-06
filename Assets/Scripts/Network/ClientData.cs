using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using Unity.Networking.Transport.Relay;
using Unity.Services.Relay;
using Unity.Services.Relay.Models;
using UnityEngine.SceneManagement;
using UnityEditor.PackageManager;

[Serializable]
public class ClientData
{
    public ulong clientId;
    public int characterId = -1;

    public ClientData(ulong clientId)
    {
        this.clientId = clientId;
    }
}
