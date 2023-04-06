using System;
using Unity.Netcode;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode.Transports.UTP;
using Unity.Networking.Transport.Relay;
using Unity.Services.Relay;
using Unity.Services.Relay.Models;
using UnityEngine.SceneManagement;
using UnityEditor.PackageManager;

public struct CharacterSelectState : INetworkSerializable, IEquatable<CharacterSelectState>
{
    public ulong ClientId;
    public int CharacterId;
    public bool IsReady;

    public CharacterSelectState(ulong clientId, int characterId = -1, bool isReady = false)
    {
        ClientId = clientId;
        CharacterId = characterId;
        IsReady = isReady;
    }

    public bool Equals(CharacterSelectState other)
    {
        return ClientId == other.ClientId && CharacterId == other.CharacterId && IsReady == other.IsReady; 
    }

    public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
    {
        serializer.SerializeValue(ref ClientId);
        serializer.SerializeValue(ref CharacterId);
        serializer.SerializeValue(ref IsReady);
    }
}
