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
    [Tooltip("The direction object moving towards this move point looks at, leafing this unassigned uses the transform of this movement point as a viewpoint ")]
    public Transform viewDirection;
    [Tooltip("Is this the last movepoint, can be used to tell the movement handler when movement or a circuit has been finished")]
    public bool lastMovepoint = false;

    private void Awake()
    {
        if (disableMovePointGraphics && GetComponent<MeshRenderer>() != null)
            GetComponent<MeshRenderer>().enabled = false;

        if (viewDirection == null)
            viewDirection = transform;
    }
}
