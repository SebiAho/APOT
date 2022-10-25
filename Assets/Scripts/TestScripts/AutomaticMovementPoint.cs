using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AutomaticMovementPoint : MonoBehaviour
{
    public bool movePointEnabled = true;
    public bool disableMovePointGraphics = true;

    [Tooltip("The move point the user will be sent towards after this, if empty the user movement will be stopped")]
    public AutomaticMovementPoint nextMovePoint;

    [Tooltip("The distance where object moving toward this point is considered to be touching it")]
    public float touchRadius = 0.001f;
    [Tooltip("Use the transform of this move point as it's viewDirection?")]
    public bool useAsViewPoint = true;
    public Transform viewDirection;

    private void Awake()
    {
        if (disableMovePointGraphics && GetComponent<MeshRenderer>() != null)
            GetComponent<MeshRenderer>().enabled = false;

        if (useAsViewPoint)
            viewDirection = transform;
    }
}
