using System.Collections.Generic;
using UnityEngine;

namespace Tunnel
{
    public class Corner : Tunnel
    {
        private GameObject DeadEndInstance;

        private new void Awake()
        {
            base.Awake();
            isStopped = true;
            type = Type.Name.CORNER;
        }

        /**
         * When turn is completed, remove the dead-end cap to the tunnel
         */
        public void onCompleteTurn(Direction direction)
        {
            Destroy(DeadEndInstance);
        }

        public override void onAddTunnel(Tunnel tunnel, Vector3Int cell, DirectionPair directionPair, string wormId)
        {
            Vector3 egressPosition = getEgressPosition(directionPair.curDir, center);
            Quaternion deadEndRotation = Rotation.DeadEndRot.getRotationFromDirection(directionPair.curDir);
            DeadEndInstance = Instantiate(DeadEnd, egressPosition, deadEndRotation, Type.instance.TunnelNetwork);

            addCellToList(cell);
            FindObjectOfType<NewTunnelFactory>().AddTunnelEvent -= onAddTunnel;
        }

        public override Vector3 getContactPosition(DirectionPair dirPair)
        {
            return getEgressPosition(dirPair.curDir, center);
        }

        public override void setHoleDirections(DirectionPair dirPair)
        {
            Direction oppDir = Dir.Base.getOppositeDirection(dirPair.prevDir); // new hole faces opposite the ingress direction
            holeDirectionList = new List<Direction>() { oppDir, dirPair.curDir };
        }
    }
}