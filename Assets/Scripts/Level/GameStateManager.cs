using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;

public class GameStateManager : MonoBehaviour
{
    private string gamePlaySceneName = "GamePlay";
    private bool gameIsStarted = false;
    private float shoppingTimer = 30;
    public  bool isRoundStarted = false;
    public bool canDoShopping = false;
    [SerializeField] private TextMeshProUGUI shoppingTimerUIText;
    private GameObject[] grids;
    private bool isUnitsReturnedToPosition = false;
    [SerializeField] private Button shopButton;
    private int roundCounter = 1;
    private int maxRoundNumber = 8;
    [SerializeField] private TextMeshProUGUI roundCounterUIText;
    private bool doItOnce = false;

    private void Start()
    {
        grids = GameObject.FindGameObjectsWithTag("Grid");
    }
    private void Update()
    {
        if (gameIsStarted)
        {
            if(shoppingTimer >= 0 && !isRoundStarted)//checks timer is not finished and round is not started
            {
                doItOnce = true;
                if (!shopButton.gameObject.activeInHierarchy)
                {
                    shopButton.gameObject.SetActive(true);
                }

                for (int i = 0; i < grids.Length; i++)//there is two grid on the map that is why this check both of them
                {
                    foreach (var item in grids[i].gameObject.GetComponent<Grid>().gameObjectsOnGrid)
                    {
                        if(item != null)
                        {
                            if (!item.gameObject.activeInHierarchy)//if units are not active(dead), activates them and sets their position and rotation from their saved informations
                            {
                                item.gameObject.SetActive(true);
                                item.gameObject.transform.position = item.gameObject.GetComponent<InfoOfUnit>().UnitsPosition;
                                item.gameObject.transform.rotation = item.gameObject.GetComponent<InfoOfUnit>().UnitsRotation;
                                item.gameObject.GetComponent<UnitController>().isNewRoundStarted = true;
                            }
                            else
                            {
                                if (!isUnitsReturnedToPosition)//if units are active, sets their position and rotation from their saved informations just once to prevent bugs
                                {
                                    if (item.gameObject.GetComponent<InfoOfUnit>().UnitsPosition != Vector3.zero)
                                    {
                                        item.gameObject.transform.position = item.gameObject.GetComponent<InfoOfUnit>().UnitsPosition;
                                        item.gameObject.transform.rotation = item.gameObject.GetComponent<InfoOfUnit>().UnitsRotation;
                                    }
                                }
                            }
                        }
                    }
                    isUnitsReturnedToPosition = true;
                }
                

                if (!shoppingTimerUIText.gameObject.activeInHierarchy)
                {
                    shoppingTimerUIText.gameObject.SetActive(true);
                }
                int secondsLeft = Mathf.RoundToInt(shoppingTimer);
                shoppingTimerUIText.text = "Time\n" + secondsLeft.ToString();
                shoppingTimer -= Time.deltaTime;
            }
            else
            {
                if (doItOnce)
                {
                    doItOnce = false;
                    if (shoppingTimerUIText.gameObject.activeInHierarchy)
                    {
                        shoppingTimerUIText.gameObject.SetActive(false);
                    }
                    shoppingTimer = 30;
                    isRoundStarted = true;
                    if (roundCounter != maxRoundNumber)
                    {
                        roundCounter++;
                        roundCounterUIText.text = "Round\n" + roundCounter.ToString() + "/" + maxRoundNumber.ToString();
                        CamCharacter[] camCharacters = GameObject.FindObjectsOfType<CamCharacter>();
                        foreach (CamCharacter item in camCharacters)
                        {
                            if (item.WonTheLevel)
                            {
                                item.CurrencyValue += 10;
                            }
                            else
                            {
                                item.CurrencyValue += 5;
                            }
                        }
                    }
                    else
                    {
                        Debug.Log("game is finished");
                        Application.Quit();
                    }
                    if (shopButton.gameObject.activeInHierarchy)
                    {
                        shopButton.gameObject.SetActive(false);
                    }
                }
                isUnitsReturnedToPosition = false;
            }
        }
        else
        {
            if (SceneManager.GetActiveScene().name == gamePlaySceneName)//game starts if active scene is the gameplay scene
            {
                gameIsStarted = true;
            }
        }
    }
}
