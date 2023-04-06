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

public class CharacterSpawner : NetworkBehaviour
{
    [SerializeField] private CharacterDatabase characterDatabase;

    public override void OnNetworkSpawn()
    {
        if (!IsServer) { return; }

        foreach(var client in HostManager.Instance.ClientData)
        {
            var character = characterDatabase.GetCharacterById(client.Value.characterId);

            if(character != null)
            {
                var spawnPos = character.SpawnPoint;
                var characterInstance = Instantiate(character.GamePlayPrefabNetworkObject, spawnPos, character.GamePlayPrefabNetworkObject.transform.rotation);
                characterInstance.SpawnAsPlayerObject(client.Value.clientId);
            }
        }
    }
}
