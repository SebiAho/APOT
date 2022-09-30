using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UserHandler : MonoBehaviour
{
    //Camera
    [Header("Camera")]
    public Transform cameraTransform;
    public float cameraSpeed = 5.0f;
    public float cameraMaxVerRot = 90f;

    Vector3 cameraRotation;//Used to calculate the camera rotation before adding it to the actual camera position

    [Tooltip("Button used to toggle camera movement lock on/off, can be disabled by leaving it empty, the defaul value revers to Left Alt and Right Mouse Button when using keyboard and mouse (with the default input manager settings)")]
    public string lockCameraButton = "Fire2";
    bool cameraLock = false;

    //User movement
    public CharacterController characterController;
    public float movementSpeed = 1.0f;

    public bool grafityEnabled = true;
    public float grafityValue = 9.81f;

    float fallVelocity = 0f; //Used to calculate the users fall speed

    //Adjust user move speed
    [Tooltip("Button used to toggle user move speed modifier on off, can be disabled by leaving it empty, the defaul value revers to Left Shift and Mouse Wheel Button when using keyboard and mouse (with the default input manager settings)")]
    public string increaseSpeedButton = "Fire3";
    public List<float> moveSpeedModifiers = new List<float> { 1f, 0.5f, 2f };
    int moveSpeedModifierIndex = 0;
    float currentMoveSpeedModifier;

    //Automatic movement
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

        //Ensure moveSpeedModifiers list isn't empty
        if (moveSpeedModifiers.Count <= 0)
            moveSpeedModifiers = new List<float> { 1 };

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
        if (!cameraLock)
        {
            cameraRotation.x -= p_mouseV * cameraSpeed;
            cameraRotation.y += p_mouseH * cameraSpeed;

            //Limit vertical rotation angle
            cameraRotation.x = Mathf.Clamp(cameraRotation.x, -cameraMaxVerRot, cameraMaxVerRot);

            //Apply rotation
            cameraTransform.eulerAngles = cameraRotation;
        }

        //Toggle camera lock
        if (Input.GetButtonDown(lockCameraButton))
            cameraLock = !cameraLock;

    }

    //Movement
    void UserMovement(float p_keyV, float p_keyH)
    {
        //Set movement towards camera direction
        Vector3 t_move = cameraTransform.right * p_keyH + cameraTransform.forward * p_keyV;

        //Prefent faster diagonal movement
        if (t_move.sqrMagnitude > 1)
            t_move.Normalize();

        if (grafityEnabled)
        {

            //Reset velocity
            if (characterController.isGrounded && fallVelocity < 0f)
                fallVelocity = 0f;

            //Increase velocity
            fallVelocity += -grafityValue;

            //Add grafity
            t_move.y = fallVelocity;

        }

        //Adjust user speed
        if (Input.GetButtonDown(increaseSpeedButton))
        {
            if (moveSpeedModifierIndex < moveSpeedModifiers.Count - 1)
                moveSpeedModifierIndex++;
            else
                moveSpeedModifierIndex = 0;
        }

        currentMoveSpeedModifier = moveSpeedModifiers[moveSpeedModifierIndex];

        //Move user
        characterController.Move(t_move * movementSpeed * currentMoveSpeedModifier * Time.deltaTime);
    }
}
