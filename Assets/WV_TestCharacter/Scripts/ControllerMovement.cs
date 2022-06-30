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
        int isWalkingHash, isRunningHash, isInteractingHash, isDropItemHash;
        bool hasMelee, hasShoot;

        // Variables to store player input values.
        Vector2 currentMovementInput;
        Vector3 currentMovement, currentRunMovement, appliedMovement;

        // Constant variables.
        bool isMovementPressed, isRunPressed, isJumpPressed, isInteractPressed, isDropItemPressed;
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
        public bool IsInteractPressed { get { return isInteractPressed; } }
        public bool IsDropItemPressed { get { return isDropItemPressed; } }
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

            // Set up parameter hash references.
            isWalkingHash = Animator.StringToHash("isWalking");
            isRunningHash = Animator.StringToHash("isRunning");
            isInteractingHash = Animator.StringToHash("isInteracting");
            isDropItemHash = Animator.StringToHash("disableObject");

            // Set up player input callbacks.
            playerInput.CharacterControls.Movement.started += OnMovementInput;
            playerInput.CharacterControls.Movement.performed += OnMovementInput;
            playerInput.CharacterControls.Movement.canceled += OnMovementInput;

            playerInput.CharacterControls.Running.started += OnRunning;
            playerInput.CharacterControls.Running.canceled += OnRunning;

            playerInput.CharacterControls.PickUpInteract.started += OnInteract;
            playerInput.CharacterControls.PickUpInteract.canceled += OnInteract;

            playerInput.CharacterControls.DropItem.started += OnDropItem;
            playerInput.CharacterControls.DropItem.canceled += OnDropItem;
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
        }

        void OnRunning(InputAction.CallbackContext ctx)
        {
            isRunPressed = ctx.ReadValueAsButton();
        }

        void OnInteract(InputAction.CallbackContext ctx)
        {
            isInteractPressed = ctx.ReadValueAsButton();
        }

        void OnDropItem(InputAction.CallbackContext ctx)
        {
            isDropItemPressed = ctx.ReadValueAsButton();
        }

        void handleAnimation()
        {
            bool isWalking = animator.GetBool(isWalkingHash);
            bool isRunning = animator.GetBool(isRunningHash);
            bool isInteracting = animator.GetBool(isInteractingHash);
            bool hasDroppedItem = animator.GetBool(isDropItemHash);

            if (isMovementPressed && !isWalking)
                animator.SetBool(isWalkingHash, true);
            else if (!isMovementPressed && isWalking)
                animator.SetBool(isWalkingHash, false);

            if ((isMovementPressed && isRunPressed) && !isRunning)
                animator.SetBool(isRunningHash, true);
            else if ((!isMovementPressed || !isRunPressed) && isRunning)
                animator.SetBool(isRunningHash, false);

            if ((hasMelee || hasShoot) && isInteractPressed)
            {
                animator.SetBool(isInteractingHash, true);
                Debug.Log("Item has been picked up.");

                if (hasMelee)
                {
                    animator.SetBool("equipMelee", true);
                    hasMelee = true;
                }
                else if (hasShoot)
                {
                    animator.SetBool("equipShoot", true);
                    hasShoot = true;
                }
            }
            else if (!isInteractPressed)
                    animator.SetBool(isInteractingHash, false);

            if (isDropItemPressed && !hasDroppedItem)
            {
                animator.SetBool(isDropItemHash, true);
                Debug.Log("Item has been disabled.");

                if (hasMelee)
                {
                    animator.SetBool("equipMelee", false);
                    hasMelee = false;
                }
                else if (hasShoot)
                {
                    animator.SetBool("equipShoot", false);
                    hasShoot = false;
                }
            }
            else if (!isDropItemPressed && hasDroppedItem)
                animator.SetBool(isDropItemHash, false);
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

        private void OnTriggerEnter(Collider other)
        {
            if (other.gameObject.tag == "Melee")
            {
                Debug.Log("Hit Melee Item");
                hasMelee = true;
            }
            else if (other.gameObject.tag == "Shoot")
            {
                Debug.Log("Hit Shoot Item");
                hasShoot = true;
            }
        }

        void OnEnable()
        {
            playerInput.CharacterControls.Enable();
        }

        void OnDisable()
        {
            playerInput.CharacterControls.Disable();
        }
    }
}

