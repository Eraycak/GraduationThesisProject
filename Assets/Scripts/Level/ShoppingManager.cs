using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ShoppingManager : MonoBehaviour
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
            GameObject spawnObject = null;
            foreach (GameObject _gameObject in listUnits)
            {
                if (_gameObject.name.Contains(nameOfImage))
                {
                    spawnObject = _gameObject;
                }
            }
            if (playerCurrencyValue >= spawnObject.GetComponent<InfoOfUnit>().costOfUnit)
            {
                Vector3 spawnLocation = benchGameObjects[currentPlayerTeamNumber].GetComponent<Bench>().LocationForBoughtUnit();
                Vector3 spawnRotation = new Vector3(0, 90, 0);
                if (currentPlayerTeamNumber == 1)
                {
                    spawnRotation = new Vector3(0, -90, 0);
                }
                GameObject unit = Instantiate(spawnObject, spawnLocation, Quaternion.Euler(spawnRotation));
                unit.GetComponent<InfoOfUnit>().TeamNumber = currentPlayerTeamNumber;
                float realTimeSeconds = Time.realtimeSinceStartup;
                unit.name = unit.name + realTimeSeconds.ToString("0.0");
                // Deduct the unit cost from the player's currency
                UpdateCurrency(unit.GetComponent<InfoOfUnit>().costOfUnit);
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

    private IEnumerator WaitUntilDisable(GameObject _gameObject)
    {
        yield return new WaitForSeconds(1f);
        _gameObject.SetActive(false);
    }
}
