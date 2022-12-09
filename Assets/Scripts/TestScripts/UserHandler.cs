using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UserHandler : MonoBehaviour
{
    public Transform cameraTransform;
    public CharacterController characterController;

    [Header("Input")]
    public string verticalCameraInput = "Mouse Y";
    public string horizontalCameraInput = "Mouse X";
    public string verticalMovementInput = "Vertical";
    public string horizontalMovementInput = "Horizontal";
    [Tooltip("Button that is used to either jump or toggle grafity, depending on the setting")]
    public string jumpOrToggleGrafityInput = "Jump";
    
    [Tooltip("Button used to pause/unpause the program, the defaul value revers to Left Alt and Right Mouse Button when using keyboard and mouse (with the default input manager settings)")]
    public string pauseInput = "Fire2";
    [Tooltip("Button used to toggle user move speed modifier on off, the defaul value revers to Left Shift and Mouse Wheel Button when using keyboard and mouse (with the default input manager settings)")]
    public string increaseSpeedInput = "Fire3";

    //Camera
    [Header("Camera")]
    public bool useCamera = true;
    public float cameraSpeed = 5.0f;
    public float cameraMaxVerticalRotation = 90f;
    Vector3 cameraRotation;//Used to calculate the camera rotation before adding it to the actual camera position
    bool cameraLock = false;

    //User movement
    [Header("Movement")]
    public float movementSpeed = 1.0f;
    Vector3 userMoveDirection;//Used to calculate and set user move direction

    //User grafity and jumping
    public bool grafityEnabled = true;
    public float grafityValue = 9.81f;
    public bool useJumpInput = false;
    public float jumpHeight = 1.0f;

    float fallVelocity = 0f; //Used to calculate the users fall speed
    float groundedTimer;//Used to make checking if character is grounded more reliable

    //Adjust user move speed
    public List<float> moveSpeedModifiers = new List<float> { 1f, 0.5f, 2f };
    int moveSpeedModifierIndex = 0;
    float currentMoveSpeedModifier;

    //Automatic movement
    [Header("Automatic Movement")]
    [Tooltip("Move user automatically, if true uses the values in AutomaticMovementHandler for move speed, camera can still be used to look around if enabled")]
    public bool automaticMovement = false;
    public AutomaticMovementHandler autoMoveHandler;

    //Pause
    public static bool programPaused = false;

    private void Awake()
    {
        //Get components
        if (cameraTransform == null)
            cameraTransform = GetComponent<Transform>();

        if (characterController == null)
            characterController = GetComponent<CharacterController>();

        if (autoMoveHandler == null)
            autoMoveHandler = GetComponent<AutomaticMovementHandler>();

        //Camera
        if (cameraTransform != null)
            cameraRotation = new Vector3(cameraTransform.rotation.x, cameraTransform.rotation.y, 0);
        else
            useCamera = false;

        //Movement
        //Ensure moveSpeedModifiers list isn't empty
        if (moveSpeedModifiers.Count <= 0)
            moveSpeedModifiers = new List<float> { 1 };

    }

    // Start is called before the first frame update
    void Start()
    {
        //Automatic movement
        if (autoMoveHandler != null)
        {
            if (!automaticMovement)
            {
                autoMoveHandler.autoMoveEnabled = false;
                if (useCamera)
                    autoMoveHandler.applyRotation = false;
            }
            else
            {
                autoMoveHandler.autoMoveEnabled = true;
                if (autoMoveHandler.applyRotation)
                    useCamera = false;
            }
        }
        else
            automaticMovement = false;
    }

    // Update is called once per frame
    void Update()
    {
        //Camera
        if(useCamera)
            CameraRotation(Input.GetAxis(verticalCameraInput), Input.GetAxis(horizontalCameraInput));

        //Movement
        if (!automaticMovement)
            UserMovement(Input.GetAxis(verticalMovementInput), Input.GetAxis(horizontalMovementInput), Input.GetButtonDown(jumpOrToggleGrafityInput), Input.GetButtonDown(increaseSpeedInput));

        //Pause
        PauseProgram(Input.GetButtonDown(pauseInput));
    }

    //Camera
    void CameraRotation(float p_vertCameraInput, float p_horizCameraInput)
    {
        if (!programPaused)
        {
            cameraRotation.x -= p_vertCameraInput * cameraSpeed;
            cameraRotation.y += p_horizCameraInput * cameraSpeed;

            //Limit vertical rotation angle
            cameraRotation.x = Mathf.Clamp(cameraRotation.x, -cameraMaxVerticalRotation, cameraMaxVerticalRotation);

            //Apply rotation
            cameraTransform.eulerAngles = cameraRotation;
        }
    }

    //Movement
    void UserMovement(float p_vertMovInput, float p_horizMovInput, bool p_jumpOrTGrafInput, bool p_iSpeedInput)
    {
        //Set movement towards camera direction
        userMoveDirection = cameraTransform.right * p_horizMovInput + cameraTransform.forward * p_vertMovInput;

        //Prefent faster diagonal movement
        if (userMoveDirection.sqrMagnitude > 1)
            userMoveDirection.Normalize();


        //Grafity and jumping
        if (grafityEnabled)
        {
            ApplyGrafity();

            //Jump or toggle grafity
            if (p_jumpOrTGrafInput && useJumpInput && groundedTimer > 0)
            {
                //Reset grounded timer
                groundedTimer = 0f;

                //Add jump velocity
                fallVelocity += Mathf.Sqrt(jumpHeight * 2.0f * grafityValue);
            }

            //Add grafity
            userMoveDirection.y = fallVelocity;
        }

        //Toggle grafity if jumping isin't enabled
        if (!useJumpInput && p_jumpOrTGrafInput)
            grafityEnabled = !grafityEnabled;

        //Adjust user speed
        if (p_iSpeedInput)
        {
            if (moveSpeedModifierIndex < moveSpeedModifiers.Count - 1)
                moveSpeedModifierIndex++;
            else
                moveSpeedModifierIndex = 0;
        }

        currentMoveSpeedModifier = moveSpeedModifiers[moveSpeedModifierIndex];

        //Move user
        characterController.Move(userMoveDirection * movementSpeed * currentMoveSpeedModifier * Time.deltaTime);
    }

    void ApplyGrafity()
    {
        //Is player grounded(should be more reliaple than simply calling the CharacterController's isGrounded variable)?
        if (characterController.isGrounded)
            groundedTimer = 0.2f;

        if (groundedTimer > 0)
            groundedTimer -= Time.deltaTime;

        //Reset velocity when hitting ground
        if(characterController.isGrounded && fallVelocity < 0)
            fallVelocity = 0f;

        //Add grafity
        fallVelocity -= grafityValue * Time.deltaTime;
    }

    void PauseProgram(bool p_pauseInput)
    {
        if (p_pauseInput && !programPaused)
        {
            Time.timeScale = 0f;
            programPaused = true;
        }
        else if (p_pauseInput && programPaused)
        {
            Time.timeScale = 1f;
            programPaused = false;
        }
    }
}
