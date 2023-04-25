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
}
