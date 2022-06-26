using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

namespace WV_TestCharacter
{
    public class ControllerMovement : MonoBehaviour
    {
        #region Variables
        // Declare reference variables.
        CharPlayerInput playerInput;
        CharacterController characterController;
        Animator animator;

        // Variables to store setter/getter parameter IDs (such as strings) for performance optimization.
        int isWalkingHash, isRunningHash;

        // Variables to store player input values.
        Vector2 currentMovementInput;
        Vector3 currentMovement, currentRunMovement, appliedMovement;

        // Constant variables.
        bool isMovementPressed, isRunPressed;
        float rotationFactorPerFrame = 1f;
        float runMultiplier = 3f;

        // Variables for Gravity
        float gravity = -9.81f;
        float groundedGravity = -0.05f;
        #endregion 

        #region Getters & Setters
        // Getters and Setters.
        public CharPlayerInput PlayerInput { get { return playerInput; } }
        public CharacterController CharacterController { get { return characterController; } }
        public Animator Animator { get { return animator; } }
        public bool IsMovementPressed { get { return isMovementPressed; } }
        public bool IsRunPressed { get { return isRunPressed; } }
        public float GroundedGravity { get { return groundedGravity; } }
        public float CurrentMovementY { get { return currentMovement.y; } set { currentMovement.y = value; } }
        public float AppliedMovementX { get { return appliedMovement.x; } set { appliedMovement.x = value; } }
        public float AppliedMovementY { get { return appliedMovement.y; } set { appliedMovement.y = value; } }
        public float AppliedMovementZ { get { return appliedMovement.z; } set { appliedMovement.z = value; } }
        public float RunMultiplier { get { return runMultiplier; } }
        public Vector2 CurrentMovementInput { get { return currentMovementInput; } }
        #endregion


        void Awake()
        {
            // Initially set reference variables.
            playerInput = new CharPlayerInput();
            characterController = GetComponent<CharacterController>();
            animator = GetComponent<Animator>();

            //pauseManager = GetComponent<PauseManager>();
            //playerEffects = GetComponent<PlayerEffects>();

            // Set up parameter hash references.
            isWalkingHash = Animator.StringToHash("isWalking");
            isRunningHash = Animator.StringToHash("isRunning");

            // Set up player input callbacks.
            playerInput.CharacterControls.Movement.started += OnMovementInput;
            playerInput.CharacterControls.Movement.performed += OnMovementInput;
            playerInput.CharacterControls.Movement.canceled += OnMovementInput;

            playerInput.CharacterControls.Running.started += OnRunning;
            playerInput.CharacterControls.Running.canceled += OnRunning;
        }

        // Update is called once per frame
        void Update()
        {
            handleAnimation();
            handleRotation();

            if (isRunPressed)
                characterController.Move(currentRunMovement * Time.deltaTime);
            else
                characterController.Move(currentMovement * Time.deltaTime);

            if (isRunPressed)
            {
                appliedMovement.x = currentRunMovement.x;
                appliedMovement.z = currentRunMovement.z;
            }
            else
            {
                appliedMovement.x = currentMovement.x;
                appliedMovement.z = currentMovement.z;
            }
            characterController.Move(appliedMovement * Time.deltaTime);

            handleGravity();
        }

        void OnMovementInput(InputAction.CallbackContext ctx)
        {
            currentMovementInput = ctx.ReadValue<Vector2>();
            float mag = Mathf.Clamp01(currentMovementInput.magnitude);
            Debug.Log(mag);
            currentMovement.x = currentMovementInput.x;
            currentMovement.z = currentMovementInput.y;
            currentRunMovement.x = currentMovementInput.x * runMultiplier;
            currentRunMovement.z = currentMovementInput.y * runMultiplier;

            isMovementPressed = currentMovementInput.x != 0 || currentMovementInput.y != 0;
            if (isMovementPressed) {
                playFootstepAudio();
            }
        }

        void OnRunning(InputAction.CallbackContext ctx)
        {
            isRunPressed = ctx.ReadValueAsButton();
        }

        void handleAnimation()
        {
            bool isWalking = animator.GetBool(isWalkingHash);
            bool isRunning = animator.GetBool(isRunningHash);

            if (isMovementPressed && !isWalking) {
                animator.SetBool(isWalkingHash, true);
                Debug.Log("WALK STEP!");
                playFootstepAudio();
            } else if (!isMovementPressed && isWalking) {
                animator.SetBool(isWalkingHash, false);
            } 
            if ((isMovementPressed && isRunPressed) && !isRunning) {
                animator.SetBool(isRunningHash, true);
                Debug.Log("RUN STEP!");
                playFootstepAudio();
            } else if ((!isMovementPressed || !isRunPressed) && isRunning) {
                animator.SetBool(isRunningHash, false);
            }
        }

        void handleRotation()
        {
            Vector3 positionToLookAt;

            positionToLookAt.x = currentMovement.x;
            positionToLookAt.y = 0f;
            positionToLookAt.z = currentMovement.z;

            Quaternion currentRotation = transform.rotation;

            if (isMovementPressed)
            {
                Quaternion targetRotation = Quaternion.LookRotation(positionToLookAt);
                transform.rotation = Quaternion.Slerp(currentRotation, targetRotation, rotationFactorPerFrame * Time.deltaTime);
            }
        }

        void handleGravity()
        {
            // Properly apply gravity to Character Controller for whether or not if the character is grounded.
            if (characterController.isGrounded)
            {
                currentMovement.y = groundedGravity;
                currentRunMovement.y = groundedGravity;
            }
        }

        // plays the footstep audio set up in wwise. CASE SENSITIVE -- "Footsteps"
        private void playFootstepAudio() {
            if (!characterController.isGrounded) {
                return; 
            }
           //animator event
            AkSoundEngine.PostEvent("Footsteps", gameObject);
            
        }

        void OnEnable()
        {
            Debug.Log("Enabling controls \n");
            playerInput.CharacterControls.Enable();
        }

        void OnDisable()
        {
            playerInput.CharacterControls.Disable();
        }
    }
}

