using UnityEngine;
using TMPro;
using Unity.Netcode;
using UnityEngine.UI;
using System.Collections.Generic;

public class CharacterSelectDisplay : NetworkBehaviour
{
    [SerializeField] private CharacterDatabase characterDatabase;
    [SerializeField] private Transform charactersHolder;
    [SerializeField] private CharacterSelectButton characterSelectButtonPrefab;
    [SerializeField] private PlayerCard[] playerCards;
    [SerializeField] private TMP_Text joinCodeText;
    [SerializeField] private Button readyButton;

    private List<CharacterSelectButton> characterSelectButtons = new List<CharacterSelectButton>();

    private NetworkList<CharacterSelectState> players;

    private void Awake()
    {
        players = new NetworkList<CharacterSelectState>();
    }

    public override void OnNetworkSpawn()
    {
        if(IsClient)
        {
            Character[] allCharacters = characterDatabase.GetAllCharacters();

            foreach(var character in allCharacters)
            {
                var characterSelectButtonPrefabInstance = Instantiate(characterSelectButtonPrefab, charactersHolder);
                characterSelectButtonPrefabInstance.SetCharacter(this, character);
                characterSelectButtons.Add(characterSelectButtonPrefabInstance);
            }

            players.OnListChanged += HandlePlayersStateChanged;
        }

        if (IsServer)
        {
            NetworkManager.Singleton.OnClientConnectedCallback += HandleClientConnected;
            NetworkManager.Singleton.OnClientDisconnectCallback += HandleClientDisconnected;

            foreach (NetworkClient client in NetworkManager.Singleton.ConnectedClientsList)
            {
                HandleClientConnected(client.ClientId);
            }
        }

        if (IsHost)
        {
            joinCodeText.text = "Join Code: " + HostManager.Instance.JoinCode;
        }
    }

    public override void OnNetworkDespawn()
    {
        if (IsClient)
        {
            players.OnListChanged -= HandlePlayersStateChanged;
        }

        if (IsServer)
        {
            NetworkManager.Singleton.OnClientConnectedCallback -= HandleClientConnected;
            NetworkManager.Singleton.OnClientDisconnectCallback -= HandleClientDisconnected;
        }
    }

    private void HandlePlayersStateChanged(NetworkListEvent<CharacterSelectState> changeEvent)
    {
        for (int i = 0; i<playerCards.Length; i++)
        {
            if(players.Count > i)
            {
                playerCards[i].UpdateDisplay(players[i]);
            }
            else
            {
                playerCards[i].DisableDisplay();
            }
        }

        foreach(var button in characterSelectButtons)
        {
            if(button.IsDisabled) { continue; }

            if(IsCharacterTaken(button.Character.Id, false))
            {
                button.SetDisabled();
            }
        }

        foreach(var player in players)
        {
            if(player.ClientId != NetworkManager.Singleton.LocalClientId) { continue; }
            
            if (player.IsReady)
            {
                readyButton.interactable = false;
                break;
            }

            if(IsCharacterTaken(player.CharacterId, false))
            {
                readyButton.interactable = false;
                break;
            }

            readyButton.interactable = true;
            break;
        }
    }

    private void HandleClientDisconnected(ulong clientId)
    {
        for (int i = 0; i < players.Count; i++)
        {
            if (players[i].ClientId != clientId) { continue; }

            players.RemoveAt(i);
            break;
        }
    }

    private void HandleClientConnected(ulong clientId)
    {
        players.Add(new CharacterSelectState(clientId));
    }

    public void Select(Character character)
    {
        for (int i = 0; i < players.Count; i++)
        {
            if (players[i].ClientId != NetworkManager.Singleton.LocalClientId) { continue; }

            if (players[i].IsReady) { return; }

            if (players[i].CharacterId == character.Id) { return; }

            if(IsCharacterTaken(character.Id,false)) { return; }
        }

        SelectServerRpc(character.Id);
    }

    [ServerRpc(RequireOwnership = false)]
    private void SelectServerRpc(int characterId, ServerRpcParams serverRpcParams = default)
    {
        for(int i = 0;i < players.Count;i++)
        {
            if (players[i].ClientId != serverRpcParams.Receive.SenderClientId) { continue; }

            if (!characterDatabase.IsValidCharacterId(characterId)) { return; }

            if (IsCharacterTaken(characterId, true)) { return; }

            players[i] = new CharacterSelectState(players[i].ClientId, characterId, players[i].IsReady);
        }
    }

    public void GetReady()
    {
        GetReadyServerRpc();
    }

    [ServerRpc(RequireOwnership = false)]
    private void GetReadyServerRpc(ServerRpcParams serverRpcParams = default)
    {
        for (int i = 0; i < players.Count; i++)
        {
            if (players[i].ClientId != serverRpcParams.Receive.SenderClientId) { continue; }

            if (!characterDatabase.IsValidCharacterId(players[i].CharacterId)) { return; }

            if (IsCharacterTaken(players[i].CharacterId, true)) { return; }

            players[i] = new CharacterSelectState(players[i].ClientId, players[i].CharacterId, true);
        }

        foreach (var player in players)
        {
            if (!player.IsReady) { return; }
        }

        foreach(var player in players)
        {
            HostManager.Instance.SetCharacter(player.ClientId, player.CharacterId);
        }

        HostManager.Instance.StartGame();
    }

    private bool IsCharacterTaken(int characterId, bool checkAll)
    {
        for(int i = 0; i < players.Count; i++)
        {
            if (!checkAll)
            {
                if (players[i].ClientId == NetworkManager.Singleton.LocalClientId) { continue; }
            }

            if (players[i].CharacterId == characterId && players[i].IsReady) { return true; }
        }

        return false;
    }
}
