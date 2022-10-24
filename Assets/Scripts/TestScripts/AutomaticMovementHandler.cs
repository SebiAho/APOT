using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AutomaticMovementHandler : MonoBehaviour
{
    public bool autoMoveEnabled = true;

    [Tooltip("Current movepoint the object with this movement handler is moving towards, the starting move point needs to be set in editor or the automatic movement will not start")]
    public AutomaticMovementPoint currentMovePoint;
    public float movementSpeed = 1f;

    [Tooltip("Apply rotation to object")]
    public bool applyRotation = true;
    [Tooltip("Use move direction set by move point, if move point does not have move point set, previous view direction will be used")]
    public bool useMovePointViewDirection = true;
    public Transform viewDirection;
    public float rotationSpeed = 1f;
    Quaternion lookRotation;
    Vector3 direction;

    [Header("User auto movement")]
    [Tooltip("Does this AutomaticMovementHandler handle the movement of user")]
    public bool userMovementHandler = false;
    [Tooltip("Set view using camera movement provided by other classes")]
    public bool CustomView = true;

    bool atMovePoint;

    private void Awake()
    {
        if (!MovePointValid(ref currentMovePoint))
            autoMoveEnabled = false; 
    }
    // Start is called before the first frame update
    void Start()
    {
        if (applyRotation)
        {
            if (autoMoveEnabled && useMovePointViewDirection)
                viewDirection = currentMovePoint.viewDirection;

            if (viewDirection == null)
                applyRotation = false;
        }
    }

    // Update is called once per frame
    void Update()
    {
        //Movement
        if(autoMoveEnabled)
        {
            //Set if object is or isn't at current move point
            atMovePoint = Vector3.Distance(transform.position, currentMovePoint.transform.position) <= currentMovePoint.touchRadius;

            if (!atMovePoint)
            {
                transform.position = Vector3.MoveTowards(transform.position, currentMovePoint.transform.position, movementSpeed * Time.deltaTime);

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
            direction = (viewDirection.position - transform.position).normalized;

            //Create rotation
            lookRotation = Quaternion.LookRotation(direction);

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
