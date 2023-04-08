using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UnitController : MonoBehaviour
{
    private Outline outline;

    private void OnMouseEnter()
    {
        if (outline == null)
        {
            outline = gameObject.AddComponent<Outline>();
            outline.OutlineMode = Outline.Mode.OutlineAll;
            outline.OutlineColor = Color.red;
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
}
