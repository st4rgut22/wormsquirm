namespace Worm
{
    public class AIInputProcessor : InputProcessor
    {
        public override void onPlayerInput(Direction direction)
        {
            if (!isDecisionProcessing)
            {
                bool isSameDirection = Tunnel.ActionPoint.instance.isDirectionAlongDecisionAxis(currentTunnel, direction);
                if (!isSameDirection)
                {
                    throw new System.Exception("Input direction should not be in the same direction (or opposite) of worm " + direction);
                }

                bool isStraightTunnel = Tunnel.Type.isTypeStraight(currentTunnel.type); // if this is the first tunnel it should be straight type
                isDecisionProcessing = true;

                RaiseDecisionEvent(isStraightTunnel, direction);
            }
        }
    }
}