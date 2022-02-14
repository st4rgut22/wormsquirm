using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InputKey
{
    public KeyCode keyCode { get; private set; }
    public Direction initDirection { get; private set; }
    public bool isPositive { get; private set; }

    /**
     * The key pressed
     * 
     * @keyCode         the name of the key pressed
     * @initDirection   the initial direction pressing the key should trigger
     * @isPositive      whether this key press should rotate player to a positive direction
     */
    public InputKey(KeyCode keyCode, Direction initDirection, bool isPositive)
    {
        this.keyCode = keyCode;
        this.initDirection = initDirection;
        this.isPositive = isPositive;
    }

    public bool isPressed()
    {
        return (Input.GetKeyDown(keyCode));
    }
}
