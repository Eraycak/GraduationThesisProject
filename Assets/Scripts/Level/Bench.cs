using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class Bench : MonoBehaviour
{
    public int numBenchSlots = 8;
    public int emptyBenchSlots = 8;
    public bool benchsAreFull = false;
    private Outline outline;
    private Vector3 emptyBenchSlotPosition = Vector3.zero;
    public bool isUnitOnBench = false;

    private void OnMouseEnter()//outlines block if mouse is on itself
    {
        if (outline == null)
        {
            outline = gameObject.AddComponent<Outline>();
            outline.OutlineMode = Outline.Mode.OutlineAll;
            outline.OutlineColor = Color.yellow;
            outline.OutlineWidth = 5f;
        }
        else
        {
            gameObject.GetComponent<Outline>().enabled = true;
        }
    }

    private void OnMouseExit()//disables outline if mouse leaves the block
    {
        gameObject.GetComponent<Outline>().enabled = false;
    }

    private void OnTriggerEnter(Collider other)
    {
        for (int i = 0; i < transform.childCount; i++)
        {
            Transform child = transform.GetChild(i);
            bool hasUnitOnItself = child.gameObject.GetComponent<BenchSlot>().hasUnitOnItself;
            if (hasUnitOnItself == false)
            {
                isUnitOnBench = true;
                child.GetComponent<BenchSlot>().hasUnitOnItself = true;
                emptyBenchSlotPosition = child.transform.position;
                break;
            }
            else
            {
                if (emptyBenchSlots > 0)
                {
                    emptyBenchSlots--;
                }
            }
        }
        if (emptyBenchSlots == 0)
        {
            benchsAreFull = true;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        isUnitOnBench = false;
    }

    public void UnitIsRemovedFromBenchSlot()
    {
        benchsAreFull = false;
        if (emptyBenchSlots < 8)
        {
            emptyBenchSlots++;
        }
    }

    public Vector3 LocationOfEmptyBench()
    {
        return emptyBenchSlotPosition;
    }
}
