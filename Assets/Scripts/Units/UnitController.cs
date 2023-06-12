using System.Collections;
using System.Collections.Generic;
using System.Security.Principal;
using Unity.Netcode;
using Unity.Netcode.Components;
using UnityEditor;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.UIElements;

public class UnitController : NetworkBehaviour
{
    private Outline outline;
    private Vector3 worldPosition;
    private Plane plane = new Plane(Vector3.up, 0);
    private GameObject gridGameObject;
    private GameObject[] grids;
    private GameObject[] benchs;
    private GameObject benchGameObject;
    public NetworkVariable<bool> inCombat;
    public NetworkVariable<bool> isMoving;
    [SerializeField] private float unitMoveSpeed = 1f;
    public NetworkVariable<Vector3> targetPosition;
    public NetworkVariable<bool> isCollidedWithEnemy;
    public NetworkVariable<bool> isRoundWon;
    public NetworkVariable<bool> isWalkingAnimPlaying;
    public NetworkVariable<bool> isAttackingAnimPlaying;
    private GameStateManager gameStateManager = null;
    public NetworkVariable<bool> isNewRoundStarted;
    public NetworkVariable<bool> isOnTheBench;
    private Camera _camera = null;
    [SerializeField] private float attackSpeedTimer = 2f;
    private bool canHit = true;

    private void Start()
    {
        UpdateVariables(false, false, false, false, false, false, true, false, Vector3.one);

        grids = GameObject.FindGameObjectsWithTag("Grid");
        if (gameObject.GetComponent<InfoOfUnit>().TeamNumber.Value == 0)
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
        if (gameObject.GetComponent<InfoOfUnit>().TeamNumber.Value == 0)
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

        if (Input.mousePosition != null)
        {
            if (_camera != null)
            {
                Ray ray = _camera.ScreenPointToRay(Input.mousePosition);
                if (plane.Raycast(ray, out distance))
                {
                    worldPosition = ray.GetPoint(distance);//get mouse position on the grid for using it later to drag units
                }
            }
        }

        if (!isOnTheBench.Value)
        {
            if (gameStateManager == null)
            {
                gameStateManager = FindFirstObjectByType<GameStateManager>();
            }

            if (gridGameObject == null)
            {
                grids = GameObject.FindGameObjectsWithTag("Grid");
                if (gameObject.GetComponent<InfoOfUnit>().TeamNumber.Value == 0)
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

            if (benchGameObject == null)
            {
                benchs = GameObject.FindGameObjectsWithTag("Bench");
                if (gameObject.GetComponent<InfoOfUnit>().TeamNumber.Value == 0)
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

            if (!isRoundWon.Value && gameStateManager.isRoundStarted.Value)//checks if round is won by one of the players
            {
                if (isNewRoundStarted.Value)//if new round is started resets unit
                {
                    if (gameObject.GetComponent<InfoOfUnit>().UnitsPosition.Value == Vector3.zero)
                    {
                        gameObject.GetComponent<InfoOfUnit>().UnitsPosition.Value = gameObject.transform.position;//saves units position and rotation before round starts. And returns it to there after round finishes.
                        gameObject.GetComponent<InfoOfUnit>().UnitsRotation.Value = gameObject.transform.rotation;
                    }
                    gameObject.transform.position = gameObject.GetComponent<InfoOfUnit>().UnitsPosition.Value;//returns unit to started position and rotation at every round.
                    gameObject.transform.rotation = gameObject.GetComponent<InfoOfUnit>().UnitsRotation.Value;
                    UpdateVariables(false, false, false, false, false, false, false, false, Vector3.one);
                }

                if (gridGameObject.GetComponent<Grid>().gameObjectsOnGrid != null && !inCombat.Value)
                {
                    int enemyNumber = 0;
                    foreach (var item in gridGameObject.GetComponent<Grid>().gameObjectsOnGrid)//checks enemy number count on the grid
                    {
                        if (item != null && item.gameObject.activeInHierarchy)
                        {
                            if (item.GetComponent<InfoOfUnit>().TeamNumber.Value != gameObject.GetComponent<InfoOfUnit>().TeamNumber.Value)
                            {
                                enemyNumber++;
                                Combat();
                            }
                        }
                    }
                    if (enemyNumber == 0)//if there is no enemy unit, the player won the round
                    {
                        UpdateVariables(inCombat.Value, isMoving.Value, isCollidedWithEnemy.Value, true, isWalkingAnimPlaying.Value, isAttackingAnimPlaying.Value, isNewRoundStarted.Value, isOnTheBench.Value, Vector3.one);
                        if (_camera.GetComponent<CamCharacter>().Id - 1 == gameObject.GetComponent<InfoOfUnit>().TeamNumber.Value)
                        {
                            _camera.GetComponent<CamCharacter>().WonTheLevel = true;
                            Debug.Log(_camera);
                        }
                        else
                        {
                            Debug.Log("you lost");
                        }
                    }
                }

                if (isMoving.Value)
                {
                    var step = unitMoveSpeed * Time.deltaTime;
                    if (Vector3.Distance(targetPosition.Value, Vector3.one) > 0f)
                    {
                        gameObject.transform.position = Vector3.MoveTowards(gameObject.transform.position, targetPosition.Value, step);//moves unit to enemy units position step by step
                        Vector3 direction = targetPosition.Value - gameObject.transform.position;
                        if (direction.magnitude > 0.01f)//turns unit towards to the enemy unit
                        {
                            transform.LookAt(gameObject.transform.position + direction);
                        }
                        if (!isWalkingAnimPlaying.Value)//plays walking anim of unit
                        {
                            UpdateVariables(inCombat.Value, isMoving.Value, isCollidedWithEnemy.Value, isRoundWon.Value, true, isAttackingAnimPlaying.Value, isNewRoundStarted.Value, isOnTheBench.Value, targetPosition.Value);
                            ChangeToMoveAnimation();
                        }
                        //PositionObject(gameObject.transform.position);
                    }
                    else
                    {//if there is no target position, stops walking and walking animation
                        UpdateVariables(inCombat.Value, false, isCollidedWithEnemy.Value, isRoundWon.Value, false, isAttackingAnimPlaying.Value, isNewRoundStarted.Value, isOnTheBench.Value, Vector3.one);
                    }
                }
            }
            else if (isRoundWon.Value)//if round is won resets unit
            {
                if (IsHost)
                {
                    gameStateManager.isRoundStarted.Value = false;
                }
                UpdateVariables(false, false, false, false, false, false, true, false, Vector3.one);
            }
        }
    }

    IEnumerator WaitForSecondsCoroutine(float time)
    {
        yield return new WaitForSeconds(time);
    }

    private void OnMouseEnter()//enables outline on unit if mouse enters that unit
    {
        if (!gameStateManager.isRoundStarted.Value)
        {
            if (Camera.allCamerasCount > 1)
            {
                //prevents players to access enemy units
                if ((_camera.name.Contains("First")) && (gameObject.GetComponent<InfoOfUnit>().TeamNumber.Value == 0))
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
                else if ((_camera.name.Contains("Second")) && (gameObject.GetComponent<InfoOfUnit>().TeamNumber.Value == 1))
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
                if ((Camera.main.name.Contains("Main")) && (gameObject.GetComponent<InfoOfUnit>().TeamNumber.Value == 0))
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
        if (!gameStateManager.isRoundStarted.Value)
        {
            if (Camera.allCamerasCount > 1)
            {
                //prevents players to access enemy units
                if ((_camera.name.Contains("First")) && (gameObject.GetComponent<InfoOfUnit>().TeamNumber.Value == 0))
                {
                    gameObject.GetComponent<Outline>().enabled = false;
                }
                else if ((_camera.name.Contains("Second")) && (gameObject.GetComponent<InfoOfUnit>().TeamNumber.Value == 1))
                {
                    gameObject.GetComponent<Outline>().enabled = false;
                }
            }
            else
            {
                //prevents players to access enemy units
                if ((Camera.main.name.Contains("Main")) && (gameObject.GetComponent<InfoOfUnit>().TeamNumber.Value == 0))
                {
                    gameObject.GetComponent<Outline>().enabled = false;
                }
            }
        }
    }

    private void OnMouseDrag()//Drags selected unit to cursor position
    {
        if (!gameStateManager.isRoundStarted.Value)
        {
            if (Camera.allCamerasCount > 1)
            {
                UpdateVariables(false, false, isCollidedWithEnemy.Value, isRoundWon.Value, isWalkingAnimPlaying.Value, isAttackingAnimPlaying.Value, isNewRoundStarted.Value, isOnTheBench.Value, Vector3.one);
                if (IsClient)
                {
                    // Call the ClientRpc method to send the new position to the server
                    PositionObject(worldPosition);
                }
                //prevents players to access enemy units
                if ((_camera.name.Contains("First")) && (gameObject.GetComponent<InfoOfUnit>().TeamNumber.Value == 0))
                {
                    //targetPosition = null;
                    //isWalkingAnimPlaying.Value = false;
                    gameObject.GetComponent<Collider>().isTrigger = true;
                    gameObject.transform.position = new Vector3(worldPosition.x, 0, worldPosition.z);
                }
                else if ((_camera.name.Contains("Second")) && (gameObject.GetComponent<InfoOfUnit>().TeamNumber.Value == 1))
                {
                    //targetPosition = null;
                    //isWalkingAnimPlaying.Value = false;
                    gameObject.GetComponent<Collider>().isTrigger = true;
                    gameObject.transform.position = new Vector3(worldPosition.x, 0, worldPosition.z);
                }
            }
            else
            {
                //prevents players to access enemy units
                if ((Camera.main.name.Contains("Main")) && (gameObject.GetComponent<InfoOfUnit>().TeamNumber.Value == 0))
                {
                    //targetPosition = null;
                    //isWalkingAnimPlaying.Value = false;
                    gameObject.GetComponent<Collider>().isTrigger = true;
                    gameObject.transform.position = new Vector3(worldPosition.x, 0, worldPosition.z);
                }
            }
        }
    }

    private void OnMouseUp()//if dragged unit is released finds nearest block to set position of unit
    {
        if (!gameStateManager.isRoundStarted.Value)
        {
            //prevents players to access enemy units
            if (Camera.allCamerasCount > 1)
            {
                if ((_camera.name.Contains("First")) && (gameObject.GetComponent<InfoOfUnit>().TeamNumber.Value == 0))
                {
                    FindLocation();
                    StartCoroutine(WaitForSecondsCoroutine(1f));
                    gameObject.GetComponent<Collider>().isTrigger = false;
                    //gameObject.GetComponent<InfoOfUnit>().UnitsPosition.Value = gameObject.transform.position;//saves units position and rotation before round starts. And returns it to there after round finishes.
                    //gameObject.GetComponent<InfoOfUnit>().UnitsRotation.Value = gameObject.transform.rotation;
                    gameObject.GetComponent<InfoOfUnit>().UpdateVariablesInfo(gameObject.transform.position, gameObject.transform.rotation, gameObject.GetComponent<InfoOfUnit>().HealthValue.Value);
                }
                else if ((_camera.name.Contains("Second")) && (gameObject.GetComponent<InfoOfUnit>().TeamNumber.Value == 1))
                {
                    FindLocation();
                    StartCoroutine(WaitForSecondsCoroutine(1f));
                    gameObject.GetComponent<Collider>().isTrigger = false;
                    //gameObject.GetComponent<InfoOfUnit>().UnitsPosition.Value = gameObject.transform.position;//saves units position and rotation before round starts. And returns it to there after round finishes.
                    //gameObject.GetComponent<InfoOfUnit>().UnitsRotation.Value = gameObject.transform.rotation;
                    gameObject.GetComponent<InfoOfUnit>().UpdateVariablesInfo(gameObject.transform.position, gameObject.transform.rotation, gameObject.GetComponent<InfoOfUnit>().HealthValue.Value);
                }
            }
            else
            {
                if ((Camera.main.name.Contains("Main")) && (gameObject.GetComponent<InfoOfUnit>().TeamNumber.Value == 0))
                {
                    FindLocation();
                    StartCoroutine(WaitForSecondsCoroutine(1f));
                    gameObject.GetComponent<Collider>().isTrigger = false;
                    //gameObject.GetComponent<InfoOfUnit>().UnitsPosition.Value = gameObject.transform.position;//saves units position and rotation before round starts. And returns it to there after round finishes.
                    //gameObject.GetComponent<InfoOfUnit>().UnitsRotation.Value = gameObject.transform.rotation;
                    gameObject.GetComponent<InfoOfUnit>().UpdateVariablesInfo(gameObject.transform.position, gameObject.transform.rotation, gameObject.GetComponent<InfoOfUnit>().HealthValue.Value);
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
            UpdateVariables(inCombat.Value, isMoving.Value, isCollidedWithEnemy.Value, isRoundWon.Value, isWalkingAnimPlaying.Value, isAttackingAnimPlaying.Value, isNewRoundStarted.Value, true, Vector3.one);
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
                    UpdateVariables(inCombat.Value, isMoving.Value, isCollidedWithEnemy.Value, isRoundWon.Value, isWalkingAnimPlaying.Value, isAttackingAnimPlaying.Value, isNewRoundStarted.Value, false, Vector3.one);
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
            UpdateVariables(inCombat.Value, isMoving.Value, isCollidedWithEnemy.Value, isRoundWon.Value, isWalkingAnimPlaying.Value, isAttackingAnimPlaying.Value, isNewRoundStarted.Value, false, Vector3.one);
            foreach (var item in grids)
            {
                if (item.GetComponent<Grid>() != null && item.GetComponent<Grid>().isActiveAndEnabled)
                {
                    item.GetComponent<Grid>().UpdateGameObjectOnTheGrid();
                }
            }
        }

        PositionObject(gameObject.transform.position);
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.GetComponent<InfoOfUnit>() != null)
        {
            if (collision.gameObject.GetComponent<InfoOfUnit>().TeamNumber.Value != gameObject.GetComponent<InfoOfUnit>().TeamNumber.Value)//checks collision with enemy is started
            {
                UpdateVariables(inCombat.Value, false, true, isRoundWon.Value, false, isAttackingAnimPlaying.Value, isNewRoundStarted.Value, isOnTheBench.Value, Vector3.one);
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
            if (collision.gameObject.GetComponent<InfoOfUnit>().TeamNumber.Value != gameObject.GetComponent<InfoOfUnit>().TeamNumber.Value)//checks collision with enemy is finished
            {
                UpdateVariables(false, isMoving.Value, false, isRoundWon.Value, isWalkingAnimPlaying.Value, false, isNewRoundStarted.Value, isOnTheBench.Value, Vector3.one);
            }
        }
    }

    private void Combat()
    {
        UpdateVariables(true, isMoving.Value, isCollidedWithEnemy.Value, isRoundWon.Value, isWalkingAnimPlaying.Value, isAttackingAnimPlaying.Value, isNewRoundStarted.Value, isOnTheBench.Value, Vector3.one);
        float distance = -1;
        GameObject nearEnemyGameObject = null;
        foreach (var item in gridGameObject.GetComponent<Grid>().gameObjectsOnGrid)//checks every units in the grid to find nearest enemy
        {
            if (item != null && item.gameObject.activeInHierarchy)
            {
                if (item.GetComponent<InfoOfUnit>().TeamNumber.Value != gameObject.GetComponent<InfoOfUnit>().TeamNumber.Value)
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
        //targetPosition.Value = enemyGameObject.transform;
        UpdateVariables(true, true, isCollidedWithEnemy.Value, isRoundWon.Value, isWalkingAnimPlaying.Value, isAttackingAnimPlaying.Value, isNewRoundStarted.Value, isOnTheBench.Value, enemyGameObject.transform.position);
    }

    private IEnumerator DamageEnemyUnit(GameObject enemyGameObject)
    {
        //damages enemy every two seconds while being collided
        while (isCollidedWithEnemy.Value)
        {
            if (canHit)
            {
                canHit = false;
                //checks enemy is died to stop attacking
                if (enemyGameObject == null || !enemyGameObject.activeInHierarchy)
                {
                    UpdateVariables(false, isMoving.Value, false, isRoundWon.Value, isWalkingAnimPlaying.Value, false, isNewRoundStarted.Value, isOnTheBench.Value, Vector3.one);
                }
                else
                {
                    enemyGameObject.GetComponent<InfoOfUnit>().HealthValue.Value -= gameObject.GetComponent<InfoOfUnit>().DamageValue.Value;//reduces enemy unit health
                    if (!isAttackingAnimPlaying.Value)//sets attacking anim
                    {
                        UpdateVariables(inCombat.Value, isMoving.Value, isCollidedWithEnemy.Value, isRoundWon.Value, isWalkingAnimPlaying.Value, true, isNewRoundStarted.Value, isOnTheBench.Value, Vector3.one);
                        ChangeToAttackAnimation();
                    }
                }
            }
            yield return new WaitForSeconds(attackSpeedTimer);
            canHit = true;
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
        if (!isCollidedWithEnemy.Value)//if unit is collided with enemy, it will not be affected
        {
            // Calculate avoidance distance
            Vector3 avoidanceDirection = transform.position - friendGameObject.transform.position;
            gameObject.transform.position += avoidanceDirection / 2;//calculated distance is to big therefore it is divided
        }
    }

    private void PositionObject(Vector3 position)
    {
        if (NetworkManager.Singleton.IsServer || NetworkManager.Singleton.IsHost)
        {
            Positioner(position, NetworkManager.Singleton.LocalClientId);
        }
        else
        {
            UpdatePositionServerRpc(position);
        }
    }

    private void Positioner(Vector3 position, ulong clientId)
    {
        transform.position = position;
        bool isAlreadyOnTheGrid = false;
        foreach (var item in grids)
        {
            if (item.GetComponent<Grid>() != null && item.GetComponent<Grid>().isActiveAndEnabled)
            {
                foreach (var objectonGrid in item.GetComponent<Grid>().gameObjectsOnGrid)
                {
                    if (objectonGrid.gameObject.name == gameObject.name)
                    {
                        isAlreadyOnTheGrid = true;
                        break;
                    }
                }

                if (!isAlreadyOnTheGrid)
                {
                    item.GetComponent<Grid>().UpdateGameObjectOnTheGrid();
                }
            }
        }
    }

    [ServerRpc(RequireOwnership = false)]
    void UpdatePositionServerRpc(Vector3 position, ServerRpcParams serverRpcParams = default)
    {
        Positioner(position, serverRpcParams.Receive.SenderClientId);
    }

    private void UpdateVariables(bool comVal, bool movVal, bool colVal, bool wonVal, bool walkVal, bool attackVal, bool newRoundVal, bool onBenchVal, Vector3 _target)
    {
        if (NetworkManager.Singleton.IsServer || NetworkManager.Singleton.IsHost)
        {
            UpdateNetworkVariables(comVal, movVal, colVal, wonVal, walkVal, attackVal, newRoundVal, onBenchVal, _target);
        }
        else
        {
            UpdateNetworkVariablesServerRpc(comVal, movVal, colVal, wonVal, walkVal, attackVal, newRoundVal, onBenchVal, _target);
        }
    }

    public void UpdateNetworkVariables(bool comVal, bool movVal, bool colVal, bool wonVal, bool walkVal, bool attackVal, bool newRoundVal, bool onBenchVal, Vector3 _target)
    {
        inCombat.Value = comVal;
        isMoving.Value = movVal;
        isCollidedWithEnemy.Value = colVal;
        isRoundWon.Value = wonVal;
        isWalkingAnimPlaying.Value = walkVal;
        isAttackingAnimPlaying.Value = attackVal;
        isNewRoundStarted.Value = newRoundVal;
        isOnTheBench.Value = onBenchVal;
        targetPosition.Value = _target;
    }

    [ServerRpc(RequireOwnership = false)]
    void UpdateNetworkVariablesServerRpc(bool comVal, bool movVal, bool colVal, bool wonVal, bool walkVal, bool attackVal, bool newRoundVal, bool onBenchVal, Vector3 _target, ServerRpcParams serverRpcParams = default)
    {
        UpdateNetworkVariables(comVal, movVal, colVal, wonVal, walkVal, attackVal, newRoundVal, onBenchVal, _target);
    }
}