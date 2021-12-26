using System.Collections.Generic;
using UnityEngine;

namespace Tunnel
{
    public class Corner : Tunnel
    {
        private new void Awake()
        {
            base.Awake();
            isStopped = true;
            type = Type.Name.CORNER;
        }

        public override void onAddTunnel(Tunnel tunnel, Vector3Int cell, DirectionPair directionPair)
        {
            addCellToList(cell);
            FindObjectOfType<NewTunnelFactory>().AddTunnelEvent -= onAddTunnel;
        }

        public override void setHoleDirections(DirectionPair dirPair)
        {
            Direction oppDir = Dir.Base.getOppositeDirection(dirPair.prevDir); // new hole faces opposite the ingress direction
            holeDirectionList = new List<Direction>() { oppDir, dirPair.curDir };
        }
    }
}