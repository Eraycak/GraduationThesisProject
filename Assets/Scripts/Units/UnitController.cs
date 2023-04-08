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
    private bool isCollided = false;
    private GameObject gridGameObject;

    private void Start()
    {
        gridGameObject = GameObject.FindGameObjectWithTag("Grid");
    }

    void Update()
    {
        float distance;
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        if (plane.Raycast(ray, out distance))
        {
            worldPosition = ray.GetPoint(distance);
        }
    }

    IEnumerator WaitForSecondsCoroutine(float time)
    {
        yield return new WaitForSeconds(time);
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
        gameObject.GetComponent<Outline>().enabled = false;
    }

    private void OnMouseDrag()
    {
        gameObject.GetComponent<Collider>().isTrigger = true;
        gameObject.transform.position = new Vector3(worldPosition.x, 0, worldPosition.z);
    }

    private void FindLocation()
    {
        GameObject nearestGameObject = gridGameObject.transform.GetChild(0).gameObject;

        float distance = Vector3.Distance(nearestGameObject.transform.position, gameObject.transform.position);
        for (int i = 1; i < gridGameObject.transform.childCount; i++)
        {
            GameObject childGameObject = gridGameObject.transform.GetChild(i).gameObject;
            if (!childGameObject.GetComponent<GridCubeController>().hasUnitOnItself)
            {
                float tmpDistance = Vector3.Distance(childGameObject.transform.position, gameObject.transform.position);
                if (tmpDistance < distance)
                {
                    distance = tmpDistance;
                    nearestGameObject = childGameObject;
                    //Debug.Log(distance + " dist" + " " + nearestGameObject.gameObject.name + " nearesttt");
                    childGameObject = null;
                }
            }
            
        }
        gameObject.transform.position = new Vector3(nearestGameObject.transform.position.x, gameObject.transform.position.y, nearestGameObject.transform.position.z);
        gameObject.transform.position = new Vector3(gameObject.transform.position.x, 0, gameObject.transform.position.z);
    }

    private void OnMouseUp()
    {
        FindLocation();
        StartCoroutine(WaitForSecondsCoroutine(1f));
        gameObject.GetComponent<Collider>().isTrigger = false;
    }

    private void OnCollisionEnter(Collision collision)
    {
        if(collision.gameObject.name != "Plane")
        {
            if (collision.gameObject.transform.parent != null && collision.gameObject.transform.parent.name != "Grid")
            {
                isCollided = true;
            }
        }
    }

    private void OnCollisionExit(Collision collision)
    {
        if (collision.gameObject.name != "Plane")
        {
            if (collision.gameObject.transform.parent != null && collision.gameObject.transform.parent.name != "Grid")
            {
                isCollided = false;
            }
        }
    }
}
