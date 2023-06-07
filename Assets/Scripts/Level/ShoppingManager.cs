using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

public class ShoppingManager : NetworkBehaviour
{
    [SerializeField] private List<GameObject> benchGameObjects = new List<GameObject>(); // Reference to the Bench game objects
    [SerializeField] private TMP_Text currencyText; // Text component to display the player's currency
    [SerializeField] private List<GameObject> listUnits = new List<GameObject>();
    [SerializeField] private int playerCurrencyValue = 0;
    [SerializeField] private int currentPlayerTeamNumber = 3;
    [SerializeField] private List<GameObject> listOfUnitsImages = new List<GameObject>();
    [SerializeField] private TMP_Text benchFullText;
    [SerializeField] private TMP_Text currencyNotEnoughText;
    private Camera _camera = null;
    private GameObject unit = null;
    private Vector3 spawnLocation = Vector3.zero;
    private Vector3 spawnRotation = Vector3.zero;
    private string spawnObjectName = null;
    //private int currentPlayerNumber = -3;

    private void Awake()
    {
        foreach(GameObject _images in listOfUnitsImages)
        {
            string nameOfImage = _images.name.Remove(0, 6);
            foreach (GameObject item in listUnits)
            {
                if (item.name.Contains(nameOfImage))
                {
                    _images.GetComponentInChildren<TextMeshProUGUI>().text = "Cost: " + item.GetComponent<InfoOfUnit>().costOfUnit;
                    break;
                }
            }
        }
        
    }

    private void Update()
    {
        _camera = Camera.current;
    }

    // Function to update the player's currency value
    public void UpdateCurrency(int amount)
    {
        if (Camera.allCamerasCount > 1)
        {
            if (_camera != null)
            {
                //prevents players to access enemy units
                if (_camera.name.Contains("First"))
                {
                    playerCurrencyValue = _camera.GetComponent<CamCharacter>().CurrencyValue;
                    currentPlayerTeamNumber = 0;
                }
                else if (_camera.name.Contains("Second"))
                {
                    playerCurrencyValue = _camera.GetComponent<CamCharacter>().CurrencyValue;
                    currentPlayerTeamNumber = 1;
                }
            }
        }
        else
        {
            if (Camera.main.name.Contains("Main"))
            {
                playerCurrencyValue = 10;
                currentPlayerTeamNumber = 0;
            }
        }
        playerCurrencyValue -= amount;
        currencyText.text = "Currency Value: " + playerCurrencyValue.ToString();
        if (!Camera.main || _camera != null)
        {
            _camera.GetComponent<CamCharacter>().CurrencyValue = playerCurrencyValue;
        }
    }

    // Function to buy a unit
    public void BuyUnit(Transform transformOfUnit)
    {
        UpdateCurrency(0);
        if (!benchGameObjects[currentPlayerTeamNumber].GetComponent<Bench>().benchsAreFull)
        {
            string nameOfImage = transformOfUnit.name.Remove(0, 6);
            GameObject spawnObject = GetObjectFromName(nameOfImage);
            if (playerCurrencyValue >= spawnObject.GetComponent<InfoOfUnit>().costOfUnit)
            {
                Vector3 spawnLocation = benchGameObjects[currentPlayerTeamNumber].GetComponent<Bench>().LocationForBoughtUnit();
                Vector3 spawnRotation = new Vector3(0, 90, 0);
                if (currentPlayerTeamNumber == 1)
                {
                    spawnRotation = new Vector3(0, -90, 0);
                }
                this.spawnObjectName = nameOfImage;
                this.spawnRotation = spawnRotation;
                this.spawnLocation = spawnLocation;
                // Deduct the unit cost from the player's currency
                UpdateCurrency(spawnObject.GetComponent<InfoOfUnit>().costOfUnit);
                SpawnObject();
            }
            else
            {
                currencyNotEnoughText.gameObject.SetActive(true);
                StartCoroutine(WaitUntilDisable(currencyNotEnoughText.gameObject));
            }
        }
        else
        {
            benchFullText.gameObject.SetActive(true);
            StartCoroutine(WaitUntilDisable(benchFullText.gameObject));
        }
    }

    private GameObject GetObjectFromName(string nameOfImage)
    {
        GameObject spawnObject = null;
        foreach (GameObject _gameObject in listUnits)
        {
            if (_gameObject.name.Contains(nameOfImage))
            {
                spawnObject = _gameObject;
            }
        }

        return spawnObject;
    }

    private IEnumerator WaitUntilDisable(GameObject _gameObject)
    {
        yield return new WaitForSeconds(1f);
        _gameObject.SetActive(false);
    }

    private void SpawnObject()
    {
        if (NetworkManager.Singleton.IsServer || NetworkManager.Singleton.IsHost)
        {
            Spawner(spawnObjectName, spawnLocation, spawnRotation, currentPlayerTeamNumber, NetworkManager.Singleton.LocalClientId);
        }
        else
        {
            SpawnOnServerRpc(spawnObjectName, spawnLocation, spawnRotation, currentPlayerTeamNumber);
        }
    }

    private void Spawner(string _spawnObjectName, Vector3 _spawnLocation, Vector3 _spawnRotation, int _currentPlayerTeamNumber, ulong clientId)
    {
        GameObject _spawnObject = GetObjectFromName(_spawnObjectName);
        unit = Instantiate(_spawnObject, _spawnLocation, Quaternion.Euler(_spawnRotation));
        NetworkObject networkObject = unit.GetComponent<NetworkObject>();
        networkObject.Spawn();
        networkObject.ChangeOwnership(clientId);
        unit.GetComponent<InfoOfUnit>().TeamNumber = _currentPlayerTeamNumber;
        float realTimeSeconds = Time.realtimeSinceStartup;
        unit.name = unit.name + realTimeSeconds.ToString("0.0");
        unit = null;
        spawnObjectName = null;
        spawnLocation = Vector3.zero;
        spawnRotation = Vector3.zero;
    }

    [ServerRpc(RequireOwnership = false)]
    private void SpawnOnServerRpc(string _spawnObjectName, Vector3 _spawnLocation, Vector3 _spawnRotation, int _currentPlayerTeamNumber, ServerRpcParams serverRpcParams = default)
    {
        Spawner(_spawnObjectName, _spawnLocation, _spawnRotation, _currentPlayerTeamNumber, serverRpcParams.Receive.SenderClientId);
    }
}
