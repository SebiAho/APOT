using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UserHandler : MonoBehaviour
{
    [Header("Camera")]
    public Transform cameraTransform;
    public float cameraSpeed = 5.0f;
    public float cameraMaxVerRot = 90f;

    Vector3 cameraRotation;

    [Header("Movement")]
    public CharacterController characterController;
    public float movementSpeed = 1.0f;

    public bool grafityEnabled = true;
    public float grafityValue = 9.81f;

    float fallVelocity = 0f;

    [Tooltip("Move user automatically (note: cannot be changed after starting the program)?")]
    public bool automaticMovement = false;
    bool autoMove;
    private void Awake()
    {
        //Camera
        if (cameraTransform == null)
            cameraTransform = GetComponent<Transform>();

        cameraRotation = new Vector3(cameraTransform.rotation.x, cameraTransform.rotation.y, 0);

        //Movement
        autoMove = automaticMovement;

        if (!autoMove)
        {
            if (characterController == null)
                characterController = GetComponent<CharacterController>();
        }
    }

    // Start is called before the first frame update
    void Start()
    {
    }

    // Update is called once per frame
    void Update()
    {
        //Camera
        float t_mouseV = Input.GetAxis("Mouse Y");
        float t_mouseH = Input.GetAxis("Mouse X");

        CameraRotation(t_mouseV, t_mouseH);

        //Movement
        if (!autoMove)
        {
            float t_KeyV = Input.GetAxis("Vertical");
            float t_KeyH = Input.GetAxis("Horizontal");
            
            UserMovement(t_KeyV, t_KeyH);
        }        
    }

    //Camera
    void CameraRotation(float p_mouseV, float p_mouseH)
    {
        cameraRotation.x -= p_mouseV * cameraSpeed;
        cameraRotation.y += p_mouseH * cameraSpeed;

        //Limit vertical rotation angle
        cameraRotation.x = Mathf.Clamp(cameraRotation.x, -cameraMaxVerRot, cameraMaxVerRot);

        //Apply rotation
        cameraTransform.eulerAngles = cameraRotation;
    }

    //Movement
    void UserMovement(float p_keyV, float p_keyH)
    {
        //Set movement towards camera direction
        Vector3 t_move = cameraTransform.right * p_keyH + cameraTransform.forward * p_keyV;

        //Prefent faster diagonal movement
        if (t_move.sqrMagnitude > 1)
            t_move.Normalize();

        if(grafityEnabled)
        {

            //Reset velocity
            if (characterController.isGrounded && fallVelocity < 0f)
                fallVelocity = 0f;

            //Increase velocity
            fallVelocity += -grafityValue;

            //Add grafity
            t_move.y = fallVelocity;

        }

        //Move user
        characterController.Move(t_move * movementSpeed * Time.deltaTime);
    }
}
