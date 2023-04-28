using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AutomaticMovementHandler : MonoBehaviour
{

    bool mainHandler = true;//Is this script the main handler of the object, if false the main handler needs to call the initialization and update functions separately 

    [Tooltip("Does the object move automatically")]
    public bool autoMoveEnabled = true;
    [Tooltip("Ignore the Y axis when testing for distance")]
    public bool ignoreYDistance = false;
    [Tooltip("Current movepoint the object with this movement handler is moving towards, the starting move point needs to be set in editor or the automatic movement will not start")]
    public AutomaticMovementPoint currentMovePoint;
    [Tooltip("When enabled the handler wont move the object enabling allowing other scripts use their own code to do so. Note that the handler will store the move point direction to moveDirection vector")]
    public bool useCustomMovement = false;
    [Tooltip("The move speed of the object")]
    public float movementSpeed = 1f;
    [Tooltip("Apply automatic rotation using viewDirection to object")]
    public bool applyRotation = true;
    [Tooltip("Use view direction set by move point, if move point does not have view direction set, previous view direction will be used")]
    public bool useMovePointViewDirection = true;
    [Tooltip("The point object will be roatated towards, if left empty and useMovePointViewDirection is false the object will maintain it's original rotation ")]
    public Transform viewDirection;
    [Tooltip("The roation speed of the object")]
    public float rotationSpeed = 1f;
    
    [HideInInspector]
    public Vector3 moveDirection;

    bool atMovePoint;//Is object at movepoint
    Quaternion lookRotation;//Used to calculate the direction object is rotated towards
    Vector3 lookDirection;//Used to calculate the direction object is rotated towards
    Vector3 currentPosition, targetPosition;//Used to perform position related calculations

    private void Awake()
    {
    }
    // Start is called before the first frame update
    void Start()
    {
        if (mainHandler)
            HandlerInit();
    }

    // Update is called once per frame
    void Update()
    {
        if (mainHandler)
            HandlerUpdate();
    }

    void HandlerInit()
    {
        if (applyRotation && currentMovePoint != null)
        {
            if (autoMoveEnabled && useMovePointViewDirection)
                viewDirection = currentMovePoint.viewDirection;

            if (viewDirection == null)
                applyRotation = false;
        }
    }

    //If this script is not the main handler this or its overload needs to be called in the actual handler script of the object
    public void HandlerInitialization(bool p_move = true)
    {
        if (p_move && MovePointValid(ref currentMovePoint))
            autoMoveEnabled = true;
        else
            autoMoveEnabled = false;

        mainHandler = false;
        HandlerInit();
    }

    //If this script is not the main handler this needs to be called in the actual main handler script of the object
    public void HandlerUpdate()
    {
        //Movement
        if (autoMoveEnabled)
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

            if (!atMovePoint)
            {
                if(!useCustomMovement)
                    transform.position = moveDirection = Vector3.MoveTowards(transform.position, currentMovePoint.transform.position, movementSpeed * Time.deltaTime);
                else
                    moveDirection = Vector3.MoveTowards(transform.position, currentMovePoint.transform.position, movementSpeed * Time.deltaTime);
            }
            else if (MovePointValid(ref currentMovePoint.nextMovePoint))
            {
                currentMovePoint = currentMovePoint.nextMovePoint;

                if (applyRotation && useMovePointViewDirection && currentMovePoint.viewDirection != null)
                    viewDirection = currentMovePoint.viewDirection;
            }
            else
                autoMoveEnabled = false;
        }

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

    bool MovePointValid(ref AutomaticMovementPoint p_movePoint)
    {
        if (p_movePoint != null && p_movePoint.movePointEnabled)
            return true;

        return false;
    }
}
