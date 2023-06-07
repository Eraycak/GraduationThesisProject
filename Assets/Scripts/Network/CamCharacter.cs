using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

public class CamCharacter : MonoBehaviour
{
    [SerializeField] private int id = -1;
    [SerializeField] private string displayName = "New Display Name";
    [SerializeField] private Sprite icon;
    [SerializeField] private Vector3 spawnPoint;
    [SerializeField] private int currencyValue = 50;
    private bool wonTheLevel = false;
    private int winCounter = 0;

    public int Id => id;
    public string DisplayName => displayName;
    public Sprite Icon => icon;
    public Vector3 SpawnPoint => spawnPoint;
    public int CurrencyValue
    {
        get => currencyValue;
        set => currencyValue = value;
    }
    public bool WonTheLevel
    {
        get => wonTheLevel;
        set => wonTheLevel = value;
    }

    public int WinCounter
    {
        get => winCounter;
        set => winCounter = value;
    }

    private void Awake()
    {
        gameObject.GetComponent<Camera>().enabled = true;
    }
}
