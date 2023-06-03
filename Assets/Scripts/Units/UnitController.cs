using System.Collections;
using System.Collections.Generic;
using System.Security.Principal;
using UnityEditor;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

public class UnitController : MonoBehaviour
{
    private Outline outline;
    private Vector3 worldPosition;
    private Plane plane = new Plane(Vector3.up, 0);
    private GameObject gridGameObject;
    private GameObject[] grids;
    private GameObject[] benchs;
    private GameObject benchGameObject;
    private bool inCombat = false;
    private bool isMoving = false;
    private float unitMoveSpeed = 1f;
    private Transform targetPosition;
    private bool isCollidedWithEnemy = false;
    private bool isRoundWon = false;
    private bool isWalkingAnimPlaying = false;
    private bool isAttackingAnimPlaying = false;
    private GameStateManager gameStateManager = null;
    public bool isNewRoundStarted = true;
    public bool isOnTheBench = false;
    private Camera _camera = null;

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
        gameStateManager = FindFirstObjectByType<GameStateManager>();
        benchs = GameObject.FindGameObjectsWithTag("Bench");
        if (gameObject.GetComponent<InfoOfUnit>().TeamNumber == 0)
        {
            if (benchs[0].gameObject.transform.name == "Bench")
            {
                benchGameObject = benchs[0];
            }
            else
            {
                benchGameObject = benchs[1];
            }
        }
        else
        {
            if (benchs[0].gameObject.transform.name == "Bench")
            {
                benchGameObject = benchs[1];
            }
            else
            {
                benchGameObject = benchs[0];
            }
        }
    }

    void Update()
    {
        float distance;
        if (Camera.current != null)
        {
            _camera = Camera.current;
        }

        if (Camera.allCamerasCount == 1)
        {
            _camera = Camera.main;
        }
        //Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        if (Input.mousePosition != null)
        {
            Ray ray = _camera.ScreenPointToRay(Input.mousePosition);
            if (plane.Raycast(ray, out distance))
            {
                worldPosition = ray.GetPoint(distance);//get mouse position on the grid for using it later to drag units
            }
        }

        if (!isOnTheBench)
        {
            if (!isRoundWon && gameStateManager.isRoundStarted)//checks if round is won by one of the players
            {
                if (isNewRoundStarted)//if new round is started resets unit
                {
                    isRoundWon = false;
                    inCombat = false;
                    isMoving = false;
                    isCollidedWithEnemy = false;
                    isWalkingAnimPlaying = false;
                    isAttackingAnimPlaying = false;
                    if (gameObject.GetComponent<InfoOfUnit>().UnitsPosition == Vector3.zero)
                    {
                        gameObject.GetComponent<InfoOfUnit>().UnitsPosition = gameObject.transform.position;//saves units position and rotation before round starts. And returns it to there after round finishes.
                        gameObject.GetComponent<InfoOfUnit>().UnitsRotation = gameObject.transform.rotation;
                    }
                    gameObject.transform.position = gameObject.GetComponent<InfoOfUnit>().UnitsPosition;//returns unit to started position and rotation at every round.
                    gameObject.transform.rotation = gameObject.GetComponent<InfoOfUnit>().UnitsRotation;
                    isNewRoundStarted = false;
                }

                if (gridGameObject.GetComponent<Grid>().gameObjectsOnGrid != null && !inCombat)
                {
                    int enemyNumber = 0;
                    foreach (var item in gridGameObject.GetComponent<Grid>().gameObjectsOnGrid)//checks enemy number count on the grid
                    {
                        if (item != null && item.gameObject.activeInHierarchy)
                        {
                            if (item.GetComponent<InfoOfUnit>().TeamNumber != gameObject.GetComponent<InfoOfUnit>().TeamNumber)
                            {
                                enemyNumber++;
                                Combat();
                            }
                        }
                    }
                    if (enemyNumber == 0)//if there is no enemy unit, the player won the round
                    {
                        isRoundWon = true;
                        _camera.GetComponent<CamCharacter>().WonTheLevel = true;
                        Debug.Log(_camera);
                    }
                }

                if (isMoving)
                {
                    var step = unitMoveSpeed * Time.deltaTime;
                    if (targetPosition != null)
                    {
                        gameObject.transform.position = Vector3.MoveTowards(gameObject.transform.position, targetPosition.transform.position, step);//moves unit to enemy units position step by step
                        Vector3 direction = targetPosition.transform.position - gameObject.transform.position;
                        if (direction.magnitude > 0.01f)//turns unit towards to the enemy unit
                        {
                            transform.LookAt(gameObject.transform.position + direction);
                        }
                        if (!isWalkingAnimPlaying)//plays walking anim of unit
                        {
                            isWalkingAnimPlaying = true;
                            ChangeToMoveAnimation();
                        }
                    }
                    else
                    {//if there is no target position, stops walking and walking animation
                        isMoving = false;
                        isWalkingAnimPlaying = false;
                    }
                }
            }
            else if (isRoundWon)//if round is won resets unit
            {
                gameStateManager.isRoundStarted = false;
                isRoundWon = false;
                inCombat = false;
                isMoving = false;
                isCollidedWithEnemy = false;
                isWalkingAnimPlaying = false;
                isAttackingAnimPlaying = false;
                isNewRoundStarted = true;
            }
        }
    }

    IEnumerator WaitForSecondsCoroutine(float time)
    {
        yield return new WaitForSeconds(time);
    }

    private void OnMouseEnter()//enables outline on unit if mouse enters that unit
    {
        if (!gameStateManager.isRoundStarted)
        {
            if (Camera.allCamerasCount > 1)
            {
                //prevents players to access enemy units
                if ((_camera.name.Contains("First")) && (gameObject.GetComponent<InfoOfUnit>().TeamNumber == 0))
                {
                    if (outline == null)//adds outline component to object if it does not have that
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
                else if ((_camera.name.Contains("Second")) && (gameObject.GetComponent<InfoOfUnit>().TeamNumber == 1))
                {
                    if (outline == null)//adds outline component to object if it does not have that
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
            }
            else
            {
                if ((Camera.main.name.Contains("Main")) && (gameObject.GetComponent<InfoOfUnit>().TeamNumber == 0))
                {
                    if (outline == null)//adds outline component to object if it does not have that
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
            }
        }
    }

    private void OnMouseExit()//disables outline on unit if mouse exits that unit
    {
        if (!gameStateManager.isRoundStarted)
        {
            if (Camera.allCamerasCount > 1)
            {
                //prevents players to access enemy units
                if ((_camera.name.Contains("First")) && (gameObject.GetComponent<InfoOfUnit>().TeamNumber == 0))
                {
                    gameObject.GetComponent<Outline>().enabled = false;
                }
                else if ((_camera.name.Contains("Second")) && (gameObject.GetComponent<InfoOfUnit>().TeamNumber == 1))
                {
                    gameObject.GetComponent<Outline>().enabled = false;
                }
            }
            else
            {
                //prevents players to access enemy units
                if ((Camera.main.name.Contains("Main")) && (gameObject.GetComponent<InfoOfUnit>().TeamNumber == 0))
                {
                    gameObject.GetComponent<Outline>().enabled = false;
                }
            }
        }
    }

    private void OnMouseDrag()//Drags selected unit to cursor position
    {
        if (!gameStateManager.isRoundStarted)
        {
            if (Camera.allCamerasCount > 1)
            {
                //prevents players to access enemy units
                if ((_camera.name.Contains("First")) && (gameObject.GetComponent<InfoOfUnit>().TeamNumber == 0))
                {
                    inCombat = false;
                    targetPosition = null;
                    isMoving = false;
                    isWalkingAnimPlaying = false;
                    gameObject.GetComponent<Collider>().isTrigger = true;
                    gameObject.transform.position = new Vector3(worldPosition.x, 0, worldPosition.z);
                }
                else if ((_camera.name.Contains("Second")) && (gameObject.GetComponent<InfoOfUnit>().TeamNumber == 1))
                {
                    inCombat = false;
                    targetPosition = null;
                    isMoving = false;
                    isWalkingAnimPlaying = false;
                    gameObject.GetComponent<Collider>().isTrigger = true;
                    gameObject.transform.position = new Vector3(worldPosition.x, 0, worldPosition.z);
                }
            }
            else
            {
                //prevents players to access enemy units
                if ((Camera.main.name.Contains("Main")) && (gameObject.GetComponent<InfoOfUnit>().TeamNumber == 0))
                {
                    inCombat = false;
                    targetPosition = null;
                    isMoving = false;
                    isWalkingAnimPlaying = false;
                    gameObject.GetComponent<Collider>().isTrigger = true;
                    gameObject.transform.position = new Vector3(worldPosition.x, 0, worldPosition.z);
                }
            }
        }
    }

    private void OnMouseUp()//if dragged unit is released finds nearest block to set position of unit
    {
        if (!gameStateManager.isRoundStarted)
        {
            //prevents players to access enemy units
            if (Camera.allCamerasCount > 1)
            {
                if ((_camera.name.Contains("First")) && (gameObject.GetComponent<InfoOfUnit>().TeamNumber == 0))
                {
                    FindLocation();
                    StartCoroutine(WaitForSecondsCoroutine(1f));
                    gameObject.GetComponent<Collider>().isTrigger = false;
                    gameObject.GetComponent<InfoOfUnit>().UnitsPosition = gameObject.transform.position;//saves units position and rotation before round starts. And returns it to there after round finishes.
                    gameObject.GetComponent<InfoOfUnit>().UnitsRotation = gameObject.transform.rotation;
                }
                else if ((_camera.name.Contains("Second")) && (gameObject.GetComponent<InfoOfUnit>().TeamNumber == 1))
                {
                    FindLocation();
                    StartCoroutine(WaitForSecondsCoroutine(1f));
                    gameObject.GetComponent<Collider>().isTrigger = false;
                    gameObject.GetComponent<InfoOfUnit>().UnitsPosition = gameObject.transform.position;//saves units position and rotation before round starts. And returns it to there after round finishes.
                    gameObject.GetComponent<InfoOfUnit>().UnitsRotation = gameObject.transform.rotation;
                }
            }
            else
            {
                if ((Camera.main.name.Contains("Main")) && (gameObject.GetComponent<InfoOfUnit>().TeamNumber == 0))
                {
                    FindLocation();
                    StartCoroutine(WaitForSecondsCoroutine(1f));
                    gameObject.GetComponent<Collider>().isTrigger = false;
                    gameObject.GetComponent<InfoOfUnit>().UnitsPosition = gameObject.transform.position;//saves units position and rotation before round starts. And returns it to there after round finishes.
                    gameObject.GetComponent<InfoOfUnit>().UnitsRotation = gameObject.transform.rotation;
                }
            }
        }
    }

    private void FindLocation()
    {
        bool isCursorOnBlock = false;
        var benchComponent = benchGameObject.GetComponent<Bench>();
        if (benchComponent.isUnitOnBench)
        {
            benchComponent.isUnitOnBench = false;
            isCursorOnBlock = true;
            var assignedLocation = benchComponent.LocationOfEmptyBench();
            gameObject.transform.position = new Vector3(assignedLocation.x, gameObject.transform.position.y, assignedLocation.z);
            gameObject.transform.position = new Vector3(gameObject.transform.position.x, 0, gameObject.transform.position.z);
            benchComponent.isUnitOnBench = false;
            isOnTheBench = true;
            foreach (var item in grids)
            {
                if (item.GetComponent<Grid>() != null && item.GetComponent<Grid>().isActiveAndEnabled)
                {
                    item.GetComponent<Grid>().UpdateGameObjectOnTheGrid();
                }
            }
        }
        else
        {
            for (int i = 0; i < gridGameObject.transform.childCount; i++)//checks every grid object to find last activated grid to snap unit on it
            {
                GameObject childGameObject = gridGameObject.transform.GetChild(i).gameObject;
                if (childGameObject.GetComponent<Outline>() != null && childGameObject.GetComponent<Outline>().isActiveAndEnabled)
                {
                    gameObject.transform.position = new Vector3(childGameObject.transform.position.x, gameObject.transform.position.y, childGameObject.transform.position.z);
                    gameObject.transform.position = new Vector3(gameObject.transform.position.x, 0, gameObject.transform.position.z);
                    isCursorOnBlock = true;
                    isOnTheBench = false;
                    foreach (var item in grids)
                    {
                        if (item.GetComponent<Grid>() != null && item.GetComponent<Grid>().isActiveAndEnabled)
                        {
                            item.GetComponent<Grid>().UpdateGameObjectOnTheGrid();
                        }
                    }
                    break;
                }
            }
        }

        if (!isCursorOnBlock)//if cursor was not on any block then chooses the block which is nearest to the unit
        {
            GameObject nearestGameObject = gridGameObject.transform.GetChild(0).gameObject;
            float distance = Vector3.Distance(nearestGameObject.transform.position, gameObject.transform.position);
            for (int i = 1; i < gridGameObject.transform.childCount; i++)
            {
                GameObject childGameObject = gridGameObject.transform.GetChild(i).gameObject;
                if (!childGameObject.GetComponent<GridCubeController>().hasUnitOnItself)//if block has unit on itself, it will be passed
                {
                    float tmpDistance = Vector3.Distance(childGameObject.transform.position, gameObject.transform.position);
                    if (tmpDistance < distance)
                    {
                        distance = tmpDistance;
                        nearestGameObject = childGameObject;
                    }
                }
            }
            gameObject.transform.position = new Vector3(nearestGameObject.transform.position.x, gameObject.transform.position.y, nearestGameObject.transform.position.z);
            gameObject.transform.position = new Vector3(gameObject.transform.position.x, 0, gameObject.transform.position.z);
            isOnTheBench = false;
            foreach (var item in grids)
            {
                if (item.GetComponent<Grid>() != null && item.GetComponent<Grid>().isActiveAndEnabled)
                {
                    item.GetComponent<Grid>().UpdateGameObjectOnTheGrid();
                }
            }
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.GetComponent<InfoOfUnit>() != null)
        {
            if (collision.gameObject.GetComponent<InfoOfUnit>().TeamNumber != gameObject.GetComponent<InfoOfUnit>().TeamNumber)//checks collision with enemy is started
            {
                isCollidedWithEnemy = true;
                isMoving = false;
                isWalkingAnimPlaying = false;
                StartCoroutine(DamageEnemyUnit(collision.gameObject));
            }
            else
            {
                AvoidFriendUnits(collision.gameObject);//if friendly objects collided, aparts them
            }
        }
    }

    private void OnCollisionExit(Collision collision)
    {
        if (collision.gameObject.GetComponent<InfoOfUnit>() != null)
        {
            if (collision.gameObject.GetComponent<InfoOfUnit>().TeamNumber != gameObject.GetComponent<InfoOfUnit>().TeamNumber)//checks collision with enemy is finished
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
        foreach (var item in gridGameObject.GetComponent<Grid>().gameObjectsOnGrid)//checks every units in the grid to find nearest enemy
        {
            if (item != null && item.gameObject.activeInHierarchy)
            {
                if (item.GetComponent<InfoOfUnit>().TeamNumber != gameObject.GetComponent<InfoOfUnit>().TeamNumber)
                {
                    float tmpDistance = Vector3.Distance(item.transform.position, gameObject.transform.position);
                    if (distance == -1)
                    {
                        distance = tmpDistance;
                        nearEnemyGameObject = item.gameObject;
                    }
                    else if (distance > tmpDistance)//sets minimum distanced units as nearEnemyGameObject
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

    private void MoveToEnemy(GameObject enemyGameObject)//sets enemy position as target position
    {
        targetPosition = enemyGameObject.transform;
        isMoving = true;
    }

    private IEnumerator DamageEnemyUnit(GameObject enemyGameObject)
    {
        //damages enemy every two seconds while being collided
        while (isCollidedWithEnemy)
        {
            //checks enemy is died to stop attacking
            if (enemyGameObject == null || !enemyGameObject.activeInHierarchy)
            {
                isCollidedWithEnemy = false;
                inCombat = false;
                isAttackingAnimPlaying = false;
            }
            else
            {
                enemyGameObject.GetComponent<InfoOfUnit>().HealthValue -= gameObject.GetComponent<InfoOfUnit>().DamageValue;//reduces enemy unit health
                if (!isAttackingAnimPlaying)//sets attacking anim
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
        //Changes units animation to walking
        gameObject.GetComponent<InfoOfUnit>().Animator.SetTrigger("Walking");
    }

    private void ChangeToAttackAnimation()
    {
        //Changes units animation to attacking
        gameObject.GetComponent<InfoOfUnit>().Animator.SetTrigger("Attacking");
    }

    private void AvoidFriendUnits(GameObject friendGameObject)
    {
        if (!isCollidedWithEnemy)//if unit is collided with enemy, it will not be affected
        {
            // Calculate avoidance distance
            Vector3 avoidanceDirection = transform.position - friendGameObject.transform.position;
            gameObject.transform.position += avoidanceDirection / 2;//calculated distance is to big therefore it is divided
        }
    }
}