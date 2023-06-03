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
                                        isUnitsReturnedToPosition = true;
                                    }
                                }
                            }
                        }
                    }
                }
                

                if (!shoppingTimerUIText.gameObject.activeInHierarchy)
                {
                    shoppingTimerUIText.gameObject.SetActive(true);
                }
                int secondsLeft = Mathf.RoundToInt(shoppingTimer);
                shoppingTimerUIText.text = secondsLeft.ToString();
                shoppingTimer -= Time.deltaTime;
            }
            else
            {
                if (shoppingTimerUIText.gameObject.activeInHierarchy)
                {
                    shoppingTimerUIText.gameObject.SetActive(false);
                }
                shoppingTimer = 30;
                isRoundStarted = true;
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
