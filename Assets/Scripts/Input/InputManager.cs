using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InputManager: MonoBehaviour
{
    public InputKeyPair inputKeyPairXZ { get; private set; }
    public InputKeyPair inputKeyPairZY { get; private set; }

    public static InputManager instance;

    private void Awake()
    {
        instance = this;

        InputKey upKeyXZ = new InputKey(KeyCode.W, Direction.Up);
        InputKey downKeyXZ = new InputKey(KeyCode.S, Direction.Down);
        inputKeyPairXZ = new InputKeyPair(upKeyXZ, downKeyXZ); // W,S controls movement along XZ Plane

        InputKey upKeyZY = new InputKey(KeyCode.A, Direction.Forward);
        InputKey downKeyZY = new InputKey(KeyCode.D, Direction.Back);
        inputKeyPairZY = new InputKeyPair(upKeyZY, downKeyZY); // A,D controls movement along ZY Plane
    }

    public InputKeyPair getInputKeyPair()
    {
        if (inputKeyPairXZ.isPairPressed())
            return inputKeyPairXZ;
        else if (inputKeyPairZY.isPairPressed())
            return inputKeyPairZY;
        else
        {
            return null;
        }
    }
}
