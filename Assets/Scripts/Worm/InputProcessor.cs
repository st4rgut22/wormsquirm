using UnityEngine;


namespace Worm
{
    public class InputProcessor : MonoBehaviour
    {
        public static float INPUT_SPEED = Tunnel.Tunnel.SCALED_GROWTH_RATE; // Match the tunnel growth rate

        private Vector3 unitVectorDirection;
        Tunnel.Tunnel changeDirTunnel;

        private bool isFollowingNewTunnelHead; // indicates if the worm is chasing the tunnel's head or if it is in prexisting tunnel
        private bool isDecisionProcessing;

        public delegate void Decision(bool isStraightTunnel, Direction direction, Tunnel.Tunnel tunnel);
        public event Decision DecisionEvent;

        private void OnEnable()
        {
            DecisionEvent += Turn.Instance.onDecision;
            DecisionEvent += FindObjectOfType<Tunnel.Map>().onDecision;
        }

        private void Awake()
        {
            changeDirTunnel = null;
            unitVectorDirection = Vector3.zero; // initially the worm is not moving
            isDecisionProcessing = false;
            isFollowingNewTunnelHead = false;
        }

        /**
         * If worm is following waypoints, prevent decision making
         */
        public void onDecisionProcessing(bool isDecisionProcessing)
        {
            this.isDecisionProcessing = isDecisionProcessing;
        }

        public void onCreateTunnel(Tunnel.CellMove cellMove, DirectionPair directionPair)
        {
            isFollowingNewTunnelHead = true;
        }

        public void onCreateJunction(Tunnel.Tunnel collisionTunnel, DirectionPair dirPair, Tunnel.CellMove cellMove)
        {
            isFollowingNewTunnelHead = false;
        }

        /**
         * Get the current cell to position the next corner tunnel. If worm is currently 'chasing' the tunnel head, use the tunnel's last added cell.
         * If worm is in a prexisting cell use worm's rounded position as the cell position
         */
        private Vector3Int getCurrentCell()
        {
            if (isFollowingNewTunnelHead)
            {
                Tunnel.Tunnel lastAddedTunnel = Tunnel.TunnelManager.Instance.getLastTunnel();
                Vector3Int lastCellPosition = lastAddedTunnel.getLastCellPosition();
                return lastCellPosition;
            }
            else
            {
                Vector3Int cell = transform.position.castToVector3Int();
                return cell;
            }
        }

        /**
         * Move the worm in a different direction and determine whether it has reached one of the six block decision points
         */
        public void onPlayerInput(Direction turnDirection)
        {
            if (!isDecisionProcessing)
            {
                unitVectorDirection = Dir.Vector.getUnitVectorFromDirection(turnDirection);
                Vector3 inputPosition = transform.position + unitVectorDirection * INPUT_SPEED;

                Vector3Int cell = getCurrentCell();

                print("input position is " + inputPosition + " next cell (HEAD) is " + cell);
                changeDirTunnel = Tunnel.Map.getTunnelFromDict(cell);

                bool isDecision = Tunnel.ActionPoint.instance.isDecisionBoundaryCrossed(changeDirTunnel, inputPosition, turnDirection);
                if (isDecision)
                {
                    bool isStraightTunnel = changeDirTunnel.type == Tunnel.Type.Name.STRAIGHT; // if this is the first tunnel it should be straight type
                    isDecisionProcessing = true;
                    DecisionEvent(isStraightTunnel, turnDirection, changeDirTunnel);
                }
            }
        }

        private void OnDisable()
        {
            if (FindObjectOfType<Turn>())
            {
                DecisionEvent -= FindObjectOfType<Turn>().onDecision;
            }
            if (FindObjectOfType<Tunnel.Map>())
            {
                DecisionEvent -= FindObjectOfType<Tunnel.Map>().onDecision;
            }
        }

    }

}