using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GridCubeController : MonoBehaviour
{
    [SerializeField] public bool hasUnitOnItself = false;
    private Outline outline;

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

    private void OnCollisionStay(Collision collision)//if block has collision, it has unit on itself
    {
        hasUnitOnItself = true;
    }

    private void OnCollisionExit(Collision collision)
    {
        hasUnitOnItself = false;
    }
}
