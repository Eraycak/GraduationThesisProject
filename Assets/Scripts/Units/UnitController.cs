using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.InputSystem;

public class UnitController : MonoBehaviour
{
    private Outline outline;
    private Vector3 worldPosition;
    private Plane plane = new Plane(Vector3.up, 0);
    //private bool isCollided = false;
    void Update()
    {
        float distance;
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        if (plane.Raycast(ray, out distance))
        {
            worldPosition = ray.GetPoint(distance);
        }
    }

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
        /*if (isCollided)
        {
            gameObject.transform.position = gameObject.transform.position + new Vector3(2, 0, 0);
        }*/
        gameObject.GetComponent<Outline>().enabled = false;
    }

    private void OnMouseDrag()
    {
        gameObject.transform.position = new Vector3(worldPosition.x, 1, worldPosition.z);
    }

    private void OnMouseUp()
    {
        gameObject.transform.position = new Vector3(worldPosition.x, 0, worldPosition.z);
    }

    /*private void OnCollisionEnter(Collision collision)
    {
        if(collision.gameObject.name != "Plane")
        {
            isCollided = true;
        }
    }

    private void OnCollisionExit(Collision collision)
    {
        isCollided = false;
    }*/
}
