using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GridCubeController : MonoBehaviour
{
    [SerializeField] public bool hasUnitOnItself = false;

    private void OnCollisionStay(Collision collision)
    {
        hasUnitOnItself = true;
    }

    private void OnCollisionExit(Collision collision)
    {
        hasUnitOnItself = false;
    }
}
