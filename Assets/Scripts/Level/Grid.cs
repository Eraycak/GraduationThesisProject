using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Grid : MonoBehaviour
{
    public  InfoOfUnit[] gameObjectsOnGrid = null;

    private void Start()
    {
        gameObjectsOnGrid = FindObjectsOfType<InfoOfUnit>();//gets all gameObjects with InfoOfUnit script to store all units for further use
    }

    public void UpdateGameObjectOnTheGrid()
    {
        gameObjectsOnGrid = FindObjectsOfType<InfoOfUnit>();//gets all gameObjects with InfoOfUnit script to store all units for further use
        int i = 0;
        foreach (var obj in gameObjectsOnGrid)
        {
            if(obj.GetComponent<UnitController>() != null && obj.GetComponent<UnitController>().isActiveAndEnabled)
            {
                if (obj.GetComponent<UnitController>().isOnTheBench)
                {
                    gameObjectsOnGrid[i] = null;
                }
            }
            i++;
        }
    }
}
