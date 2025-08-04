using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Events;

// Records input data via UnityEvents from the new input system
public class InputManager : MonoBehaviour
{

    public static InputManager Instance;

    private PlayerInput playerInput;

    string actionMapName;
    public Vector2 moveDirection {get; private set;}
    public bool jumpWasPressed {get; private set;}
    public bool jumpIsHeld {get; private set;}
    public bool jumpWasReleased {get; private set;}
    public bool runIsHeld {get; private set;}

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        RegisterSelf();
    }

    public static void RegisterSelf()
    {
        Instance.playerInput = Instance.gameObject.GetComponent<PlayerInput>();
        Instance.actionMapName = Instance.playerInput.currentActionMap.name;
    }

    public void OnMove(InputAction.CallbackContext aContext)
    {
        if (actionMapName == "Player")
        {
            moveDirection = aContext.ReadValue<Vector2>();
        }
    }

    public void OnRun(InputAction.CallbackContext aContext)
    {
        if (actionMapName == "Player")
        {
            if (aContext.phase == InputActionPhase.Performed)
            {
                runIsHeld = true;
            }
            if (aContext.action.WasReleasedThisFrame())
            {
                runIsHeld = false;
            }
        }
    }

    public void OnJump(InputAction.CallbackContext aContext)
    {
        jumpWasPressed = aContext.action.WasPressedThisFrame();
        jumpIsHeld = aContext.action.IsPressed();
        jumpWasReleased = aContext.action.WasReleasedThisFrame();
    }

    public void OnEscape(InputAction.CallbackContext aContext)
    {
        if (aContext.phase == InputActionPhase.Performed)
        {
            Debug.Log("OnEscape() called");
        }
    }
}
