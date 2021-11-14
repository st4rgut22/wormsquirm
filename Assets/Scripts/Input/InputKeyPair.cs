using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InputKeyPair
{
    public InputKey upKeyCode { get; private set; }
    public InputKey downKeyCode { get; private set; }

    public InputKeyPair(InputKey upKeyCode, InputKey downKeyCode)
    {
        this.upKeyCode = upKeyCode;
        this.downKeyCode = downKeyCode;
    }

    /*
     * Is either the up or down key pressed
     */
    public bool isPairPressed()
    {
        bool isPressed = upKeyCode.isPressed() || downKeyCode.isPressed();
        return isPressed;
    }

    public InputKey getPressedInputKey()
    {
        if (upKeyCode.isPressed()) return upKeyCode;
        else if (downKeyCode.isPressed()) return downKeyCode;
        else
        {
            return null;
        }
    }
}
