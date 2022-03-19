namespace Worm
{
    public class AIInputProcessor : InputProcessor
    {
        public override void onPlayerInput(Direction direction)
        {
            if (!isDecisionProcessing)
            {
                bool isDifferentDirection = Tunnel.ActionPoint.instance.isDirectionAlongDecisionAxis(currentTunnel, direction);
                if (!isDifferentDirection) // dont execute a decision event if it is already going in same direction
                {
                    return;
                }

                bool isStraightTunnel = Tunnel.Type.isTypeStraight(currentTunnel.type); // if this is the first tunnel it should be straight type
                isDecisionProcessing = true;

                RaiseDecisionEvent(isStraightTunnel, direction);
            }
        }
    }
}