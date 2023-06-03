using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class Bench : MonoBehaviour
{
    private int numBenchSlots;
    private int emptyBenchSlots;
    internal bool benchsAreFull = false;
    private Outline outline;
    private Vector3 emptyBenchSlotPosition = Vector3.zero;
    internal bool isUnitOnBench = false;

    private void Start()
    {
        numBenchSlots = transform.childCount;
        emptyBenchSlots = numBenchSlots;
    }

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
        CheckEmptyBenchSlots();
        if (!benchsAreFull)
        {
            for (int i = 0; i < transform.childCount; i++)
            {
                Transform child = transform.GetChild(i);
                BenchSlot slot = child.GetComponent<BenchSlot>();
                bool hasUnitOnItself = slot.hasUnitOnItself;
                if (hasUnitOnItself == false)
                {
                    isUnitOnBench = true;
                    slot.hasUnitOnItself = true;
                    emptyBenchSlotPosition = child.transform.position;
                    break;
                }
                else
                {
                    if (slot.unitOnMe != null)
                    {
                        if (slot.unitOnMe.name == other.name)
                        {
                            isUnitOnBench = true;
                            child.GetComponent<BenchSlot>().hasUnitOnItself = true;
                            emptyBenchSlotPosition = child.transform.position;
                            break;
                        }
                    }
                }
            }
        }
        else
        {
            Debug.Log("Benchs Are Full");
        }
    }

    private void OnTriggerExit(Collider other)
    {
        CheckEmptyBenchSlots();
        isUnitOnBench = false;
    }

    private void OnCollisionEnter(Collision collision)
    {
        CheckEmptyBenchSlots();
    }

    private void OnCollisionExit(Collision collision)
    {
        CheckEmptyBenchSlots();
    }

    private void CheckEmptyBenchSlots()
    {
        emptyBenchSlots = numBenchSlots;
        for (int i = 0; i < transform.childCount; i++)
        {
            Transform child = transform.GetChild(i);
            BenchSlot slot = child.GetComponent<BenchSlot>();
            if (slot.hasUnitOnItself)
            {
                emptyBenchSlots--;
            }
        }

        if (emptyBenchSlots == 0)
        {
            benchsAreFull = true;
        }
        else
        {
            benchsAreFull = false;
        }
    }

    public Vector3 LocationOfEmptyBench()
    {
        return emptyBenchSlotPosition;
    }

    public Vector3 LocationForBoughtUnit()
    {
        for (int i = 0; i < transform.childCount; i++)
        {
            Transform child = transform.GetChild(i);
            BenchSlot slot = child.GetComponent<BenchSlot>();
            bool hasUnitOnItself = slot.hasUnitOnItself;
            if (hasUnitOnItself == false)
            {
                isUnitOnBench = true;
                slot.hasUnitOnItself = true;
                emptyBenchSlotPosition = child.transform.position;
                break;
            }
        }
        return emptyBenchSlotPosition;
    }
}
