using Unity.Netcode;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode.Transports.UTP;
using Unity.Networking.Transport.Relay;
using Unity.Services.Relay;
using Unity.Services.Relay.Models;
using UnityEngine.SceneManagement;
using UnityEditor.PackageManager;

[CreateAssetMenu(fileName = "New Character", menuName = "Characters/Character")]
public class Character : ScriptableObject
{
    [SerializeField] private int id = -1;
    [SerializeField] private string displayName = "New Display Name";
    [SerializeField] private Sprite icon;
    [SerializeField] private NetworkObject gamePlayPrefabNetworkObject;
    [SerializeField] private Vector3 spawnPoint;

    public int Id=>id;
    public string DisplayName => displayName;
    public Sprite Icon => icon;
    public NetworkObject GamePlayPrefabNetworkObject => gamePlayPrefabNetworkObject;
    public Vector3 SpawnPoint => spawnPoint;
}
