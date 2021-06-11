using System;
using System.Collections;
using System.Collections.Generic;
using Mirror;
using UnityEngine;
using UnityEngine.InputSystem;

public class CameraController : NetworkBehaviour
{
    [SerializeField] private Transform playerCameraTransform = null;
    [SerializeField] private float speed = 20f;
    [SerializeField] private float screenBorderThickness = 10f;
    [SerializeField] private Vector2 screenXLimits = Vector2.zero; // {min, max}
    [SerializeField] private Vector2 screenZLimits = Vector2.zero; // {min, max}

    private Vector2 previousInput;
    
    private Controls controls;

    public override void OnStartAuthority()
    {
        playerCameraTransform.gameObject.SetActive(true);
        
        controls = new Controls();

        controls.Player.MoveCamera.performed += SetPreviousInput;
        controls.Player.MoveCamera.canceled += SetPreviousInput;

        controls.Enable();
    }

    [ClientCallback]
    private void Update() 
    {
        if(!hasAuthority || !Application.isFocused) { return ;}

        UpdateCameraPosition();
    }

    private void UpdateCameraPosition()
    {
        Vector3 pos = playerCameraTransform.position;

        if(previousInput == Vector2.zero) // no keyboard input, check mouse
        {
            Vector3 cursorMovement = Vector3.zero;

            Vector2 cursorPosition = Mouse.current.position.ReadValue();

            if(cursorPosition.y >= Screen.height - screenBorderThickness) // top
            {
                cursorMovement.z += 1;
            }
            else if(cursorPosition.y <= screenBorderThickness) // bottom
            {
                cursorMovement.z -= 1;
            }
            else if(cursorPosition.x >= Screen.width - screenBorderThickness) // right
            {
                cursorMovement.x += 1;
            }
            else if(cursorPosition.x <= screenBorderThickness) // left
            {
                cursorMovement.x -= 1;
            }

            pos += cursorMovement.normalized * speed * Time.deltaTime;
        }
        else // keyboard input
        {
            pos += new Vector3(previousInput.x, 0f, previousInput.y) * speed * Time.deltaTime;
        }

        pos.x = Mathf.Clamp(pos.x, screenXLimits.x, screenZLimits.y);
        pos.z = Mathf.Clamp(pos.z, screenZLimits.x, screenZLimits.y);

        playerCameraTransform.position = pos;
    }

    private void SetPreviousInput(InputAction.CallbackContext cxt)
    {
        previousInput = cxt.ReadValue<Vector2>();
    }
}
