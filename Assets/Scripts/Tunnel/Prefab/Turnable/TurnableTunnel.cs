using UnityEngine;

namespace Tunnel
{
    public abstract class TurnableTunnel : Tunnel
    {
        protected GameObject DeadEndInstance;

        private void Awake()
        {
            base.Awake();
        }

        public override void onAddTunnel(Tunnel tunnel, Vector3Int cell, DirectionPair directionPair, string wormId)
        {
            base.onAddTunnel(tunnel, cell, directionPair, wormId);
            Quaternion deadEndRotation = Rotation.DeadEndRot.getRotationFromDirection(directionPair.curDir);

            if (directionPair.isStraight())
            {
                Vector3 deadEndPosition = getOffsetPosition(directionPair.curDir, tunnel.center);
                DeadEndInstance = Instantiate(DeadEnd, deadEndPosition, deadEndRotation, Type.instance.TunnelNetwork);
            }
            else
            {
                Vector3 egressPosition = getOffsetPosition(directionPair.curDir, center);
                DeadEndInstance = Instantiate(DeadEnd, egressPosition, deadEndRotation, Type.instance.TunnelNetwork);
            }

            DeadEndInstance.name = gameObject.name + "DEADEND";

        }

        /**
         * When turn is completed, remove the dead-end cap to the tunnel
         */
        public void onCompleteTurn(string wormId, Direction direction)
        {
            print("destroy deadEnd " + DeadEndInstance.name);
            Destroy(DeadEndInstance);
        }
    }

}