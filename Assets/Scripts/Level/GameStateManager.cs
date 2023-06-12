using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;
using Unity.Netcode;

public class GameStateManager : NetworkBehaviour
{
    public string gamePlaySceneName = "GamePlay";
    public NetworkVariable<bool> gameIsStarted = new NetworkVariable<bool>();
    public NetworkVariable<float> shoppingTimer = new NetworkVariable<float>();
    public NetworkVariable<bool> isRoundStarted = new NetworkVariable<bool>();
    public NetworkVariable<bool> canDoShopping = new NetworkVariable<bool>();
    [SerializeField] public TextMeshProUGUI shoppingTimerUIText;
    public GameObject[] grids;
    public NetworkVariable<bool> isUnitsReturnedToPosition = new NetworkVariable<bool>();
    [SerializeField] public Button shopButton;
    public NetworkVariable<int> roundCounter = new NetworkVariable<int>();
    public NetworkVariable<int> maxRoundNumber = new NetworkVariable<int>();
    [SerializeField] public TextMeshProUGUI roundCounterUIText;
    public NetworkVariable<bool> doItOnce = new NetworkVariable<bool>();
    [SerializeField] public TextMeshProUGUI winCounterUIText;

    public void Start()
    {
        if(IsHost)
        {
            shoppingTimer.Value = 30;
            isRoundStarted.Value = false;
            canDoShopping.Value = false;
            isUnitsReturnedToPosition.Value = false;
            roundCounter.Value = 1;
            maxRoundNumber.Value = 8;
            doItOnce.Value = false;
        }
        grids = GameObject.FindGameObjectsWithTag("Grid");
    }
    public void Update()
    {
        if (gameIsStarted.Value)
        {
            if (IsHost)
            {
                if (shoppingTimer.Value >= 0 && !isRoundStarted.Value)//checks timer is not finished and round is not started
                {
                    doItOnce.Value = true;
                    if (!shopButton.gameObject.activeInHierarchy)
                    {
                        shopButton.gameObject.SetActive(true);
                    }

                    for (int i = 0; i < grids.Length; i++)//there is two grid on the map that is why this check both of them
                    {
                        foreach (var item in grids[i].gameObject.GetComponent<Grid>().gameObjectsOnGrid)
                        {
                            if (item != null)
                            {
                                if (!item.gameObject.activeInHierarchy)//if units are not active(dead), activates them and sets their position and rotation from their saved informations
                                {
                                    item.gameObject.SetActive(true);
                                    item.gameObject.transform.position = item.gameObject.GetComponent<InfoOfUnit>().UnitsPosition.Value;
                                    item.gameObject.transform.rotation = item.gameObject.GetComponent<InfoOfUnit>().UnitsRotation.Value;
                                    item.gameObject.GetComponent<UnitController>().isNewRoundStarted.Value = true;
                                    item.gameObject.GetComponent<InfoOfUnit>().HealthValue = item.gameObject.GetComponent<InfoOfUnit>().StartHealthValue;
                                    item.gameObject.GetComponent<UnitController>().inCombat.Value = false;
                                    item.gameObject.GetComponent<UnitController>().isMoving.Value = false;
                                    item.gameObject.GetComponent<UnitController>().targetPosition = null;
                                    item.gameObject.GetComponent<UnitController>().isCollidedWithEnemy.Value = false;
                                    item.gameObject.GetComponent<UnitController>().isRoundWon.Value = false;
                                    item.gameObject.GetComponent<UnitController>().isWalkingAnimPlaying.Value = false;
                                    item.gameObject.GetComponent<UnitController>().isAttackingAnimPlaying.Value = false;
                                    item.gameObject.GetComponent<UnitController>().isNewRoundStarted.Value = true;
                                }
                                else
                                {
                                    if (!isUnitsReturnedToPosition.Value)//if units are active, sets their position and rotation from their saved informations just once to prevent bugs
                                    {
                                        if (item.gameObject.GetComponent<InfoOfUnit>().UnitsPosition.Value != Vector3.zero)
                                        {
                                            item.gameObject.transform.position = item.gameObject.GetComponent<InfoOfUnit>().UnitsPosition.Value;
                                            item.gameObject.transform.rotation = item.gameObject.GetComponent<InfoOfUnit>().UnitsRotation.Value;
                                            item.gameObject.GetComponent<InfoOfUnit>().HealthValue = item.gameObject.GetComponent<InfoOfUnit>().StartHealthValue;
                                            item.gameObject.GetComponent<UnitController>().inCombat.Value = false;
                                            item.gameObject.GetComponent<UnitController>().isMoving.Value = false;
                                            item.gameObject.GetComponent<UnitController>().targetPosition = null;
                                            item.gameObject.GetComponent<UnitController>().isCollidedWithEnemy.Value = false;
                                            item.gameObject.GetComponent<UnitController>().isRoundWon.Value = false;
                                            item.gameObject.GetComponent<UnitController>().isWalkingAnimPlaying.Value = false;
                                            item.gameObject.GetComponent<UnitController>().isAttackingAnimPlaying.Value = false;
                                            item.gameObject.GetComponent<UnitController>().isNewRoundStarted.Value = true;
                                        }
                                    }
                                }
                            }
                        }
                        isUnitsReturnedToPosition.Value = true;
                    }


                    if (!shoppingTimerUIText.gameObject.activeInHierarchy)
                    {
                        shoppingTimerUIText.gameObject.SetActive(true);
                    }
                    int secondsLeft = Mathf.RoundToInt(shoppingTimer.Value);
                    shoppingTimerUIText.text = "Time\n" + secondsLeft.ToString();
                    shoppingTimer.Value -= Time.deltaTime;
                }
                else
                {
                    if (doItOnce.Value)
                    {
                        doItOnce.Value = false;
                        if (shoppingTimerUIText.gameObject.activeInHierarchy)
                        {
                            shoppingTimerUIText.gameObject.SetActive(false);
                        }
                        shoppingTimer.Value = 30;
                        isRoundStarted.Value = true;
                        if (roundCounter.Value != maxRoundNumber.Value)
                        {
                            roundCounterUIText.text = "Round\n" + roundCounter.Value.ToString() + "/" + maxRoundNumber.Value.ToString();
                            roundCounter.Value++;
                            CamCharacter[] camCharacters = GameObject.FindObjectsOfType<CamCharacter>();
                            foreach (CamCharacter item in camCharacters)
                            {
                                if (item.WonTheLevel)
                                {
                                    item.CurrencyValue += 10;
                                    item.WinCounter++;
                                }
                                else
                                {
                                    item.CurrencyValue += 5;
                                }
                                winCounterUIText.text = "Win\n" + item.WinCounter.ToString();
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
                    isUnitsReturnedToPosition.Value = false;
                }
            }
            else if (IsClient)
            {
                if (shoppingTimer.Value >= 0 && !isRoundStarted.Value)//checks timer is not finished and round is not started
                {
                    if (!shopButton.gameObject.activeInHierarchy)
                    {
                        shopButton.gameObject.SetActive(true);
                    }

                    if (!shoppingTimerUIText.gameObject.activeInHierarchy)
                    {
                        shoppingTimerUIText.gameObject.SetActive(true);
                    }
                    int secondsLeft = Mathf.RoundToInt(shoppingTimer.Value);
                    shoppingTimerUIText.text = "Time\n" + secondsLeft.ToString();
                }
                else
                {
                    if (doItOnce.Value)
                    {
                        if (shoppingTimerUIText.gameObject.activeInHierarchy)
                        {
                            shoppingTimerUIText.gameObject.SetActive(false);
                        }
                        if (roundCounter.Value != maxRoundNumber.Value)
                        {
                            roundCounterUIText.text = "Round\n" + roundCounter.Value.ToString() + "/" + maxRoundNumber.Value.ToString();
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
                }
            }
        }
        else
        {
            if (SceneManager.GetActiveScene().name == gamePlaySceneName)//game starts if active scene is the gameplay scene
            {
                gameIsStarted.Value = true;
            }
        }
    }

    public void QuitMethod()
    {
        Application.Quit();
    }
}
