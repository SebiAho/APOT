using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AutomaticMovementPoint : MonoBehaviour
{
    public bool movePointEnabled = true;
    public bool disableGraphics = true;

    [Tooltip("The move point the user will be sent towards after this, if empty the user movement will be stopped")]
    public AutomaticMovementPoint nextMovePoint;

    [Tooltip("The distance where object moving toward this point is considered to be touching it")]
    public float touchRadius = 0.001f;
    public bool useAsViewPoint = true;
    public Transform viewDirection;

    private void Awake()
    {
        if (disableGraphics && GetComponent<MeshRenderer>() != null)
            GetComponent<MeshRenderer>().enabled = false;

        if (useAsViewPoint)
            viewDirection = transform;
    }

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
    }
}
