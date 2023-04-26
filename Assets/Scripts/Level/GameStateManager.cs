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

    private void Start()
    {
        grids = GameObject.FindGameObjectsWithTag("Grid");
    }
    private void Update()
    {
        if (gameIsStarted)
        {
            if(shoppingTimer >= 0 && !isRoundStarted)
            {
                for (int i = 0; i < grids.Length; i++)
                {
                    foreach (var item in grids[i].gameObject.GetComponent<Grid>().gameObjectsOnGrid)
                    {
                        if (!item.gameObject.activeInHierarchy)
                        {
                            item.gameObject.SetActive(true);
                            item.gameObject.GetComponent<UnitController>().isNewRoundStarted = true;
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
            }
        }
        else
        {
            if (SceneManager.GetActiveScene().name == gamePlaySceneName)
            {
                gameIsStarted = true;
            }
        }
    }
}
