using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InputKey
{
    public KeyCode keyCode { get; private set; }
    public Direction initDirection { get; private set; }

    public InputKey(KeyCode keyCode, Direction initDirection)
    {
        this.keyCode = keyCode;
        this.initDirection = initDirection;
    }

    public bool isPressed()
    {
        return (Input.GetKeyDown(keyCode));
    }
}
