using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Events;

// Manages input & switches between menu and overworld modes
public class InputManager : MonoBehaviour
{

    public static InputManager Instance;

    private PlayerInput playerInput;
    //private PlayerTopDown playerTopDown;

    string actionMapName;
    public Vector2 moveDirection {get; private set;}
    public bool jumpWasPressed {get; private set;}
    public bool jumpIsHeld {get; private set;}
    public bool jumpWasReleased {get; private set;}
    public bool runIsHeld {get; private set;}
    
    // input select cooldown
    private bool selectCooldownActive = false;
    public float selectCooldown = 0.1f;

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

    // When the Move action is performed
    public void OnMove(InputAction.CallbackContext aContext)
    {

        if (actionMapName == "Player")
        {
            moveDirection = aContext.ReadValue<Vector2>();
        }

        if(actionMapName == "UI")
        {
            if (aContext.phase == InputActionPhase.Performed)
            {
                Debug.Log("OnMove() called in UI mode");
            }
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

    // When the Select action is performed
    public void OnJump(InputAction.CallbackContext aContext)
    {
        jumpWasPressed = aContext.action.WasPressedThisFrame();
        jumpIsHeld = aContext.action.IsPressed();
        jumpWasReleased = aContext.action.WasReleasedThisFrame();

        if (jumpWasPressed)
        {
            Debug.Log("jump was pressed");
        }
        if (jumpIsHeld)
        {
            Debug.Log("jump is currently held");
        }
        if (jumpWasReleased)
        {
            Debug.Log("jump was released");
        }

        if (aContext.phase == InputActionPhase.Performed)
        {
            StartCoroutine(WaitForSelectCooldown());
            if (actionMapName == "Player")
            {
                if (true) // check player status
                {
                    //Debug.Log("OnJump pressed in Overworld mode");
                }
            }
            if (!selectCooldownActive)
            {
                if (actionMapName == "UI")
                {
                    Debug.Log("OnJump pressed in UI mode");
                }
            }
        }

    }

    public void OnEscape(InputAction.CallbackContext aContext)
    {
        if (aContext.phase == InputActionPhase.Performed)
        {
            Debug.Log("OnEscape() called");
        }
    }

    private IEnumerator WaitForSelectCooldown()
    {
        selectCooldownActive = true;
        yield return new WaitForSeconds(selectCooldown);
        selectCooldownActive = false;
    }

    // Switch to Overworld input mode
    public static void SwitchInputModeOverworld()
    {
        Instance.playerInput.SwitchCurrentActionMap("Player");
        Instance.actionMapName = Instance.playerInput.currentActionMap.name;
        Debug.Log("Switched action map to Player (overworld)");
    }

    // Switch to Menu UI input mode
    public static void SwitchInputModeMenu()
    {
        Instance.playerInput.SwitchCurrentActionMap("UI");
        Instance.actionMapName = Instance.playerInput.currentActionMap.name;
        Debug.Log("Switched action map to UI");
    }
}
