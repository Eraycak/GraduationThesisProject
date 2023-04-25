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
    private GameObject gridGameObject;
    private GameObject[] grids;
    private bool inCombat = false;
    private bool isMoving = false;
    private float unitMoveSpeed = 1f;
    private Transform targetPosition;
    private bool isCollidedWithEnemy = false;
    private bool isRoundWon = false;
    private bool isWalkingAnimPlaying = false;
    private bool isAttackingAnimPlaying = false;

    private void Start()
    {
        grids = GameObject.FindGameObjectsWithTag("Grid");
        if (gameObject.GetComponent<InfoOfUnit>().TeamNumber == 0)
        {
            if (grids[0].gameObject.transform.name == "Grid")
            {
                gridGameObject = grids[0];
            }
            else
            {
                gridGameObject = grids[1];
            }
        }
        else
        {
            if (grids[0].gameObject.transform.name == "Grid")
            {
                gridGameObject = grids[1];
            }
            else
            {
                gridGameObject = grids[0];
            }
        }
    }

    void Update()
    {
        if (!isRoundWon)
        {
            float distance;
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            if (plane.Raycast(ray, out distance))
            {
                worldPosition = ray.GetPoint(distance);
            }

            if (gridGameObject.GetComponent<Grid>().gameObjectsOnGrid != null && !inCombat)
            {
                int enemyNumber = 0;
                foreach (var item in gridGameObject.GetComponent<Grid>().gameObjectsOnGrid)
                {
                    if (item != null)
                    {
                        if (item.GetComponent<InfoOfUnit>().TeamNumber != gameObject.GetComponent<InfoOfUnit>().TeamNumber)
                        {
                            enemyNumber++;
                            Combat();
                        }
                    }
                }
                if (enemyNumber == 0)
                {
                    isRoundWon = true;
                    Debug.Log("Won");
                }
            }

            if (isMoving)
            {
                var step = unitMoveSpeed * Time.deltaTime;
                if (targetPosition != null)
                {
                    gameObject.transform.position = Vector3.MoveTowards(gameObject.transform.position, targetPosition.transform.position, step);
                    if (!isWalkingAnimPlaying)
                    {
                        isWalkingAnimPlaying = true;
                        ChangeToMoveAnimation();
                    }
                }
                else
                {
                    isMoving = false;
                    isWalkingAnimPlaying = false;
                }
            }
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
        inCombat = false;
        targetPosition = null;
        isMoving = false;
        isWalkingAnimPlaying = false;
        gameObject.GetComponent<Collider>().isTrigger = true;
        gameObject.transform.position = new Vector3(worldPosition.x, 0, worldPosition.z);
    }

    private void FindLocation()
    {
        bool isCursorOnBlock = false;
        for (int i = 0; i < gridGameObject.transform.childCount; i++)
        {
            GameObject childGameObject = gridGameObject.transform.GetChild(i).gameObject;
            if (childGameObject.GetComponent<Outline>() != null && childGameObject.GetComponent<Outline>().isActiveAndEnabled)
            {
                gameObject.transform.position = new Vector3(childGameObject.transform.position.x, gameObject.transform.position.y, childGameObject.transform.position.z);
                gameObject.transform.position = new Vector3(gameObject.transform.position.x, 0, gameObject.transform.position.z);
                isCursorOnBlock = true;
                break;
            }
        }

        if (!isCursorOnBlock)
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
                        childGameObject = null;
                    }
                }
            }
            gameObject.transform.position = new Vector3(nearestGameObject.transform.position.x, gameObject.transform.position.y, nearestGameObject.transform.position.z);
            gameObject.transform.position = new Vector3(gameObject.transform.position.x, 0, gameObject.transform.position.z);
        }
    }

    private void OnMouseUp()
    {
        FindLocation();
        StartCoroutine(WaitForSecondsCoroutine(1f));
        gameObject.GetComponent<Collider>().isTrigger = false;
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.GetComponent<InfoOfUnit>() != null)
        {
            if (collision.gameObject.GetComponent<InfoOfUnit>().TeamNumber != gameObject.GetComponent<InfoOfUnit>().TeamNumber)
            {
                isCollidedWithEnemy = true;
                isMoving = false;
                isWalkingAnimPlaying = false;
                StartCoroutine(DamageEnemyUnit(collision.gameObject));
            }
        }
    }

    private void OnCollisionExit(Collision collision)
    {
        if (collision.gameObject.GetComponent<InfoOfUnit>() != null)
        {
            if (collision.gameObject.GetComponent<InfoOfUnit>().TeamNumber != gameObject.GetComponent<InfoOfUnit>().TeamNumber)
            {
                isCollidedWithEnemy = false;
                inCombat = false;
                isAttackingAnimPlaying = false;
            }
        }
    }

    private void Combat()
    {
        inCombat = true;
        float distance = -1;
        GameObject nearEnemyGameObject = null;
        foreach (var item in gridGameObject.GetComponent<Grid>().gameObjectsOnGrid)
        {
            if (item != null)
            {
                if (item.GetComponent<InfoOfUnit>().TeamNumber != gameObject.GetComponent<InfoOfUnit>().TeamNumber)
                {
                    float tmpDistance = Vector3.Distance(item.transform.position, gameObject.transform.position);
                    if (distance == -1)
                    {
                        distance = tmpDistance;
                        nearEnemyGameObject = item.gameObject;
                    }
                    else if (distance > tmpDistance)
                    {
                        distance = tmpDistance;
                        nearEnemyGameObject = item.gameObject;
                    }
                }
            }
        }
        if (nearEnemyGameObject != null)
        {
            MoveToEnemy(nearEnemyGameObject);
        }
    }

    private void MoveToEnemy(GameObject enemyGameObject)
    {
        targetPosition = enemyGameObject.transform;
        isMoving = true;
    }

    private IEnumerator DamageEnemyUnit(GameObject enemyGameObject)
    {
        while (isCollidedWithEnemy)
        {
            if (enemyGameObject == null)
            {
                isCollidedWithEnemy = false;
                inCombat = false;
                isAttackingAnimPlaying = false;
            }
            else
            {
                enemyGameObject.GetComponent<InfoOfUnit>().HealthValue -= gameObject.GetComponent<InfoOfUnit>().DamageValue;
                if (!isAttackingAnimPlaying)
                {
                    isAttackingAnimPlaying = true;
                    ChangeToAttackAnimation();
                }
            }
            yield return new WaitForSeconds(2f);
        }
    }

    private void ChangeToMoveAnimation()
    {
        gameObject.GetComponent<InfoOfUnit>().Animator.SetTrigger("Walking");
    }
    private void ChangeToAttackAnimation()
    {
        gameObject.GetComponent<InfoOfUnit>().Animator.SetTrigger("Attacking");
    }
}