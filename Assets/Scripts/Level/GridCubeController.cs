using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GridCubeController : MonoBehaviour
{
    [SerializeField] public bool hasUnitOnItself = false;
    private Outline outline;

    private void OnMouseEnter()
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

    private void OnMouseExit()
    {
        gameObject.GetComponent<Outline>().enabled = false;
    }

    private void OnCollisionStay(Collision collision)
    {
        hasUnitOnItself = true;
    }

    private void OnCollisionExit(Collision collision)
    {
        hasUnitOnItself = false;
    }
}
