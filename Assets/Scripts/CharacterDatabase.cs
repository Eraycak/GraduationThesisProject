using System.Linq;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using Unity.Networking.Transport.Relay;
using Unity.Services.Relay;
using Unity.Services.Relay.Models;
using UnityEngine.SceneManagement;
using UnityEditor.PackageManager;

[CreateAssetMenu(fileName = "New Character Database", menuName = "Characters/Database")]
public class CharacterDatabase : ScriptableObject
{
    [SerializeField] private Character[] characters = new Character[0];

    public Character[] GetAllCharacters() => characters;

    public Character GetCharacterById(int id)
    {
        foreach (var character in characters)
        {
            if (character.Id == id) { return character; }
        }

        return null;
    }

    public bool IsValidCharacterId(int id)
    {
        return characters.Any(c => c.Id == id);
    }
}
