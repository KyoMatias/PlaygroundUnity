using System;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Rendering;

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

public class CameraLook : MonoBehaviour
{
   [Header("Input")] [SerializeField] private InputActionReference lookAction;
   
   [Header("Camera Component")] 
   [SerializeField] private Transform _playerBody;
   
   [Header("Camera State")] 
   [SerializeField] private bool _isActive;
   public bool PlayerCameraState => _isActive;

   [Header("Camera Look Setings")] 
   private float baseSensitivity = 0.1f;
   public float UserSensitivity;
   private float maxYAngle = 80f;

   private float xRotation;
   private void Awake()
   {
      UpdateSensitivity(UserSensitivity);
   }

   private void OnEnable()
   {
      lookAction.action.Enable();
      Cursor.lockState = CursorLockMode.Locked;
      Cursor.visible = false;
   }

   private void OnDisable()
   {
      lookAction.action.Disable();
      Cursor.lockState = CursorLockMode.None;
      Cursor.visible = true;
   }

   private void LateUpdate()
   {
      Look();
   }

   public void UpdateSensitivity(float sens)
   {
      baseSensitivity = sens * baseSensitivity;
   }

   public bool SetCamera(bool value)
   {
      _isActive = value;
      return _isActive;
   }

   void Look()
   {
      Vector2 lookInput = lookAction.action.ReadValue<Vector2>() * baseSensitivity;

      float mouseX = lookInput.x;
      float mouseY = lookInput.y;

      xRotation -= mouseY;
      xRotation = Mathf.Clamp(xRotation, -maxYAngle, maxYAngle);
      
      transform.localRotation = Quaternion.Euler(xRotation, 0f,0f);
      
      _playerBody.Rotate(Vector3.up * mouseX);
   }
}