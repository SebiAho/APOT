using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AutomaticMovementHandler : MonoBehaviour
{
    public bool autoMoveEnabled = true;

    [Tooltip("Current movepoint the object with this movement handler is moving towards, the starting move point needs to be set in editor or the automatic movement will not start")]
    public AutomaticMovementPoint currentMovePoint;
    public float movementSpeed = 1f;

    [Header("User auto movement")]
    [Tooltip("Does this AutomaticMovementHandler handle the movement of user")]
    public bool userMovementHandler = false;
    [Tooltip("Set view using camera movement provided by other classes")]
    public bool CustomView = true;
    [Tooltip("Allow move points to set view direction")]
    public Transform viewDirection;

    bool atMovePoint;

    private void Awake()
    {
        if (currentMovePoint == null)
            autoMoveEnabled = false;
    }
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if(!currentMovePoint.enabled)
        {
           
        }

        //Set if object is or isn't at current move point
        atMovePoint = Vector3.Distance(transform.position, currentMovePoint.transform.position) <= currentMovePoint.touchRadius;

        if (atMovePoint == false)
        {
            transform.position = Vector3.MoveTowards(transform.position, currentMovePoint.transform.position, movementSpeed * Time.deltaTime);
        }
        else
        {
            currentMovePoint = currentMovePoint.nextMovePoint;
        }
    }

    void MoveTowardsMovePoint(ref AutomaticMovementPoint p_movePoint)
    {

    }

    //Is current move point valid 
    bool MovePointValid()
    {
        if (currentMovePoint != null && currentMovePoint.enabled)
            return true;

        return false;
    }
}
