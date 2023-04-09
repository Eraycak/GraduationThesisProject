using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Grid : MonoBehaviour
{
    //private GameObject[] gameObjectsOnGrid = null;
    public  InfoOfUnit[] gameObjectsOnGrid = null;

    private void Start()
    {
        gameObjectsOnGrid = FindObjectsOfType<InfoOfUnit>();
    }
}
