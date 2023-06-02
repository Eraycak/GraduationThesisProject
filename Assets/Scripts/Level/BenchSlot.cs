using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class BenchSlot : MonoBehaviour
{
    [SerializeField] public bool hasUnitOnItself = false;

    private void OnTriggerExit(Collider other)
    {
        transform.parent.GetComponent<Bench>().UnitIsRemovedFromBenchSlot();
        hasUnitOnItself = false;
    }
}
