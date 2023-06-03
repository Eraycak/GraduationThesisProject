using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class BenchSlot : MonoBehaviour
{
    [SerializeField] public bool hasUnitOnItself = false;
    public GameObject unitOnMe = null;

    private void OnTriggerExit(Collider other)
    {
        if (unitOnMe != null)
        {
            if (other.gameObject.name == unitOnMe.name)
            {
                hasUnitOnItself = false;
                unitOnMe = null;
            }
            else
            {
                hasUnitOnItself = true;
            }
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        if(unitOnMe == null)
        {
            unitOnMe = collision.gameObject;
            hasUnitOnItself = true;
        }
    }
}
