public static class AxisFactory
{
    public static Axis Get()
    {
        int xzInput = getInputAlongAxis(InputManager.instance.inputKeyPairXZ); 
        int zyInput = getInputAlongAxis(InputManager.instance.inputKeyPairZY); 


        if (xzInput != Move.NEUTRAL)
        {
            return new AxisXZ(xzInput);
        }
        else if (zyInput != Move.NEUTRAL)
        {
            return new AxisZY(zyInput);
        }
        else
        {
            return null;
        }
    }

    public static int getInputAlongAxis(InputKeyPair inputKeyPair)
    {
        if (inputKeyPair.upKeyCode.isPressed())
        {
            return Move.CLOCKWISE;
        }
        else if (inputKeyPair.downKeyCode.isPressed())
        {
            return Move.COUNTER_CLOCKWISE;
        }
        else
        {
            return Move.NEUTRAL;
        }
    }
}
