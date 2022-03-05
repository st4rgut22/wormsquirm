using UnityEngine;

namespace Tunnel
{
    public abstract class TurnableTunnel : Tunnel
    {
        private void Awake()
        {
            base.Awake();
        }

        public override void onAddTunnel(Tunnel tunnel, CellMove cellMove, DirectionPair directionPair, string wormId)
        {
            Vector3Int lastCellPosition = cellMove.cell;
            base.onAddTunnel(tunnel, cellMove, directionPair, wormId);
            Quaternion deadEndRotation = Rotation.DeadEndRot.getRotationFromDirection(directionPair.curDir);
            Vector3Int deadEndCellPosition = Dir.Vector.getNextCellFromDirection(lastCellPosition, directionPair.curDir);
            Tunnel nextTunnel = TunnelMap.getTunnelFromDict(deadEndCellPosition);

            if (nextTunnel == null)// check if tunnel already exists where dead end should be placed 
            {
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
        }

        /**
         * When turn is completed, remove the dead-end cap to the tunnel
         */
        public void onCompleteTurn(string wormId, Direction direction)
        {
            if (DeadEndInstance != null)
            {
                Destroy(DeadEndInstance);
            }
        }
    }

}