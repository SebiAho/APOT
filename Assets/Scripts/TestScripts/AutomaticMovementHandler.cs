using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AutomaticMovementHandler : MonoBehaviour
{
    [Tooltip("Is this script the main handler of the object, if false the main handler needs to call the initialization and update functions separately")]
    public bool mainHandler = true;
    [Tooltip("Does the object move automatically")]
    public bool autoMoveEnabled = true;
    [Tooltip("The number of seconds the movement is delayed from the start of the scene, helps avoid potential bugs that can occur if moving before the scene is fully loaded")]
    public float moveDelay = 1f;
    [Tooltip("Ignore the Y axis when testing for distance")]
    public bool ignoreYDistance = false;
    [Tooltip("Current movepoint the object with this movement handler is moving towards, the starting move point needs to be set in editor or the automatic movement will not start")]
    public AutomaticMovementPoint currentMovePoint;
    [Tooltip("When enabled the handler wont move the object enabling allowing other scripts use their own code to do so. Note that the handler will store the move point direction to moveDirection vector")]
    public bool useCustomMovement = false;
    [Tooltip("The move speed of the object")]
    public float movementSpeed = 1f;
    [Tooltip("The number of times the object goes trough the movepoints, when it encounters a movepoint where lastMovePoint = true the current circuit is considered finished")]
    public int circuitAmount = 1;

    [Header("Rotation")]
    [Tooltip("Apply automatic rotation using viewDirection to object")]
    public bool applyRotation = true;
    [Tooltip("Use view direction set by move point, if move point does not have view direction set, previous view direction will be used")]
    public bool useMovePointViewDirection = true;
    [Tooltip("The point object will be roatated towards, if left empty and useMovePointViewDirection is false the object will maintain it's original rotation ")]
    public Transform viewDirection;
    [Tooltip("The roation speed of the object")]
    public float rotationSpeed = 1f;

    Vector3 moveDirection;//Used to give other scripts the direction of the current movepoint
    bool movementFinished = false;//Used to tell other scripts when the movement has been finished

    bool atMovePoint;//Is object at movepoint
    int completedCircuits = 0;//The amount of circuits the object has completed

    Quaternion lookRotation;//Used to calculate the direction object is rotated towards
    Vector3 lookDirection;//Used to calculate the direction object is rotated towards
    Vector3 currentPosition, targetPosition;//Used to perform position related calculations

    private void Awake()
    {
        if (mainHandler)
            HandlerInitAwake();
    }
    // Start is called before the first frame update
    void Start()
    {
        if(ABOTData.testStarted)
        {
            moveDelay = ABOTData.delayTime;
        }

        if (mainHandler)
            HandlerInitStart();
    }

    // Update is called once per frame
    void Update()
    {
        if (mainHandler)
            HandlerUpdate();
    }

    //If this script is not the main handler this needs to be called in the actual handler script of the object
    public void HandlerInitAwake()
    {
        if (!MovePointValid(currentMovePoint))
        {
            autoMoveEnabled = false;
        }

        if (autoMoveEnabled)
        {
            //Initialize moveDirection value to avoid potential bugs
            moveDirection = Vector3.MoveTowards(transform.position, currentMovePoint.transform.position, 0);
        }
    }

    //If this script is not the main handler this needs to be called in the actual handler script of the object
    public void HandlerInitStart()
    {
        if (applyRotation && currentMovePoint != null)
        {
            if (useMovePointViewDirection)
                viewDirection = currentMovePoint.viewDirection;
        }

        if (viewDirection == null)
            applyRotation = false;
    }

    //If this script is not the main handler this needs to be called in the actual main handler script of the object
    public void HandlerUpdate()
    {
        //Movement
        if (autoMoveEnabled && moveDelay <= 0)
        {
            //Check if object is or isn't at current move point
            if (ignoreYDistance)
            {
                currentPosition = new Vector3(transform.position.x, 0, transform.position.z);
                targetPosition = new Vector3(currentMovePoint.transform.position.x, 0, currentMovePoint.transform.position.z);
            }
            else
            {
                currentPosition = transform.position;
                targetPosition = currentMovePoint.transform.position;
            }
            atMovePoint = Vector3.Distance(currentPosition, targetPosition) <= currentMovePoint.touchRadius;

            if (!atMovePoint)//If object isin't at movepoint move object towards it or assign it's direction to be used in a custom movement code
            {
                if (!useCustomMovement)
                    transform.position = moveDirection = Vector3.MoveTowards(transform.position, currentMovePoint.transform.position, movementSpeed * Time.deltaTime);
                else
                    moveDirection = Vector3.MoveTowards(transform.position, currentMovePoint.transform.position, movementSpeed * Time.deltaTime);
            }
            else//If object is at a movepoint either assign a new one or finish the movement
            {
                //Finish movement or continue to the next circuit when at the last movepoint
                if (currentMovePoint.lastMovepoint == true)
                {
                    if (completedCircuits < circuitAmount)
                        completedCircuits++;

                    if(completedCircuits >= circuitAmount | !MovePointValid(currentMovePoint.nextMovePoint))
                    {
                        movementFinished = true;
                        autoMoveEnabled = false;
                        //Debug.Log("Movement finished");
                    }
                }

                //Set the next movepoint if it is valid
                if (MovePointValid(currentMovePoint.nextMovePoint) && !movementFinished)
                {
                    currentMovePoint = currentMovePoint.nextMovePoint;

                    if (applyRotation && useMovePointViewDirection && currentMovePoint.viewDirection != null)
                        viewDirection = currentMovePoint.viewDirection;
                }
            }
        }
        else if(moveDelay > 0)
            moveDelay -= Time.deltaTime;


        //Rotation
        if (applyRotation)
        {
            //Find vector pointing towards viewDirection
            lookDirection = (viewDirection.position - transform.position).normalized;

            //Create rotation
            lookRotation = Quaternion.LookRotation(lookDirection);

            //Rotate over time
            transform.rotation = Quaternion.Slerp(transform.rotation, lookRotation, Time.deltaTime * rotationSpeed);
        }
    }

    bool MovePointValid(AutomaticMovementPoint p_movePoint)
    {
        if (p_movePoint != null && p_movePoint.movePointEnabled)
            return true;

        return false;
    }

    public Vector3 getMoveDirection()
    {
        return moveDirection;
    }

    public bool getMovementFinished()
    {
        return movementFinished;
    }
}
