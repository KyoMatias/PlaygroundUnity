using System;
using UnityEngine;
using UnityEngine.InputSystem;

/* =============================================================================
   Project:        $PROJECT_NAME$
   File:           $NAME$.cs
   Author:         $USER$
   Studio:         SundayMood Studios (Indie Home Studio)
   IDE:            JetBrains Rider
   Engine:         Unity
   Created:        $DATE$

   Description:
   ---------------------------------------------------------------------------
   [Brief description of what this script does.]

   Notes:
   ---------------------------------------------------------------------------
   - Part of the $PROJECT_NAME$ project by SundayMood Studios.
   ========================================================================== */
public class PlayerMovement : MonoBehaviour
{
   [Header("Input Reference")] 
   [SerializeField] private InputActionReference moveAction, jumpAction, sprintAction, walkAction;
   
   [Header("Movement Properties")] 
   public float playerMoveSpeed = 5;
   [SerializeField] private float _accelaration = 10f;
   [SerializeField] private float _deccelaration = 5f;

   [Header("Gravity")] 
   [SerializeField] private float gravity = -10f; // stronger than Physics.gravity.y
   [SerializeField] private float groundedStickForce = -5f;


   [Header("Components")] 
   private Rigidbody rb;
   [SerializeField] private Transform _cameraTransform;
   private Vector3 currentVelocity;
   [SerializeField] private CapsuleCollider _capsule;

   [Header("Actions")] 
   [SerializeField] private bool _isWalking;
   [SerializeField] private bool _isSprinting;
   [SerializeField] private bool _isJumping;

   [Header("Sprint")] 
   [SerializeField] private float sprintMultiplier = 1.6f;
   [SerializeField] private float sprintRamp;
   private float currentSprintMultiplier = 1f;
   private float sprintVelReference;

   [Header("Jump")] 
   [SerializeField] private float jumpForce = 200f;
   [SerializeField] private float groundCheckDistance = 0.01f;
   [SerializeField] private LayerMask groundLayer;

   private bool jumpRequested;
   private bool isGrounded;
   
   
   private void Awake()
   {
      InitializeComponents();
   }

   private void OnEnable()
   {
      moveAction?.action.Enable();
      walkAction?.action.Enable();
      sprintAction?.action.Enable();
      jumpAction?.action.Enable();

      walkAction.action.performed += Walk;
      walkAction.action.canceled += WalkRelease;
      sprintAction.action.performed += Sprint;
      sprintAction.action.canceled += SprintRelease;

      jumpAction.action.performed += Jump;
   }

   private void OnDisable()
   {
      moveAction?.action.Disable();
      walkAction?.action.Disable(); 
      sprintAction?.action.Disable();
      
      walkAction.action.performed -= Walk;
      walkAction.action.canceled -= WalkRelease;
      sprintAction.action.performed -= Sprint;
      sprintAction.action.canceled -= SprintRelease;

      jumpAction.action.performed -= Jump;
   }


   private void Update()
   {
      CheckGrounded();
      UpdateSprintMultiplier();
      HandleMovement();
      HandleJump();
      ApplyGravity();
   }
   void HandleMovement()
   {
      Vector2 input = moveAction.action.ReadValue<Vector2>();
      
      Vector3 forward = _cameraTransform.forward;
      Vector3 right = _cameraTransform.right;

      forward.y = 0f;
      right.y = 0f;
      
      Vector3 moveDir = (forward * input.y + right * input.x).normalized;

      float speed;

      if (_isWalking)
      {
         speed = playerMoveSpeed * 0.5f;
      }
      else
      {
         speed = playerMoveSpeed * currentSprintMultiplier;
      }
      
      Vector3 targetVelocity = moveDir * speed;

      float rate = moveDir.magnitude > 0 ? _accelaration : _deccelaration;
      currentVelocity = Vector3.Lerp(currentVelocity, targetVelocity, rate * Time.deltaTime);
      
      rb.MovePosition(rb.position + currentVelocity * Time.deltaTime);
   }
   void InitializeComponents()
   {
      rb = GetComponent<Rigidbody>();
      _capsule = GetComponentInChildren<CapsuleCollider>();
      
      rb.freezeRotation = true;
      rb.useGravity = false;
   }

   void CheckGrounded()
   {
      Vector3 origin = transform.position + Vector3.up * 0.05f;
      float rayLength = (_capsule.height * 0.5f) + groundCheckDistance;

      isGrounded = Physics.Raycast(origin, Vector3.down, rayLength, groundLayer);
   }

   void ApplyGravity()
   {
      if (isGrounded && rb.linearVelocity.y <= 0f)
      {
         rb.linearVelocity = new Vector3(
            rb.linearVelocity.x,
            groundedStickForce,
            rb.linearVelocity.z
         );
      }
      else
      {
         rb.AddForce(Vector3.up * gravity, ForceMode.Acceleration);
      }
   }


   void HandleJump()
   {
      if (!jumpRequested || !isGrounded) return;

      rb.linearVelocity = new Vector3(rb.linearVelocity.x, 0f, rb.linearVelocity.z);
      rb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);

      jumpRequested = false;
   }

   void UpdateSprintMultiplier()
   {
      float target = _isSprinting ? sprintMultiplier : 1f;

      currentSprintMultiplier = Mathf.SmoothDamp(currentSprintMultiplier, target, ref sprintVelReference, sprintRamp,
         Mathf.Infinity, Time.deltaTime);
   }

   void Jump(InputAction.CallbackContext context)
   {
      if (!isGrounded) return;
      jumpRequested = true;
   }

   void Walk(InputAction.CallbackContext context)
   {
      _isWalking = true;
   }

   void WalkRelease(InputAction.CallbackContext context)
   {
      _isWalking = false;
   }

   void Sprint(InputAction.CallbackContext context)
   {
      if (_isWalking) return;
      _isSprinting = true;
   }

   void SprintRelease(InputAction.CallbackContext context)
   {
      _isSprinting = false;
   }
   
   
}
