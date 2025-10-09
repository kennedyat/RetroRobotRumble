using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Controls;

public class UIPlayerControllerHandler : MonoBehaviour
{
    private enum InputType
    {
        None,
        KeyboardMouse,
        Gamepad
    }

    private InputType currentInputType = InputType.None;

    [SerializeField] private List<GameObject> keyboardSprites;
    [SerializeField] private List<GameObject> gamepadSprites;

    protected void Update()
    {
        // Check for mouse or keyboard activity
        if (Keyboard.current.anyKey.wasPressedThisFrame || Mouse.current.delta.ReadValue() != Vector2.zero)
        {
            if (currentInputType != InputType.KeyboardMouse)
            {
                currentInputType = InputType.KeyboardMouse;
                Debug.Log("Switched to Keyboard/Mouse");
            }

            HandlingKeyboardInput(true);
        }

        // Check for gamepad activity
        else if (Gamepad.current != null)
        {
            foreach (var control in Gamepad.current.allControls)
            {
                if (control is ButtonControl button && button.wasPressedThisFrame)
                {
                    if (currentInputType != InputType.Gamepad)
                    {
                        currentInputType = InputType.Gamepad;
                        Debug.Log("Switched to Gamepad");
                    }

                    HandlingKeyboardInput(false);
                    break;
                }
            }
        }

        // For Debugging Purposes
        // if (Keyboard.current.tKey.isPressed)
        // {
        //     HandlingKeyboardInput(true);
        // }
        // if (Keyboard.current.yKey.isPressed)
        // {
        //     HandlingKeyboardInput(false);
        // }
    }

    private void HandlingKeyboardInput(bool isKeyboard)
    {
        foreach (GameObject gm in keyboardSprites)
        {
            gm.SetActive(isKeyboard);
        }
        foreach (GameObject gm in gamepadSprites)
        {
            gm.SetActive(!isKeyboard);
        }
    }
}
