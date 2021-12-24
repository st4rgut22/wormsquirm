using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Tunnel
{
    /**
     * Modifies existing tunnels by creating junctions to replace intersected tunnel segments
     */
    public class ModTunnelFactory : Factory
    {
        private static int junctionCount = 0;
        protected Tunnel collidedTunnel;

        /**
         * Fill the void left by the intersection of two tunnels
         * 
         * @collisionTunnel the tunnel that has been collided into
         * @ingressDirection the direction intering the collided tunnel
         * @contactPosition the position the tunnels made contact (center of the tunnel)
         */
        public void onCreateJunction(Tunnel collisionTunnel, DirectionPair dirPair, CellMove cellMove)
        {
            collidedTunnel = collisionTunnel;
            directionPair = dirPair;
            this.cellMove = cellMove;
            Tunnel tunnel = getTunnel();
            addTunnel(tunnel);
        }

        private Transform getJunction(List<Direction> holeDirectionList)
        {
            int holeCount = holeDirectionList.Count;
            bool isDirAdjacent = Dir.Base.areDirectionsAdjacent(holeDirectionList);

            if (holeCount == 3)
            {
                return isDirAdjacent ? Type.instance.ThreewayTunnelAdj : Type.instance.ThreewayTunnelOpp; // get the correc three way tunnl (based on pivot rotation)
            }
            else if (holeCount == 4)
            {
                return isDirAdjacent ? Type.instance.FourwayTunnelAdj : Type.instance.FourwayTunnelOpp;
            }
            else if (holeCount == 5)
            {
                return Type.instance.FivewayTunnel;
            }
            else if (holeCount == 6)
            {
                return Type.instance.SixwayTunnel;
            }
            else
            {
                throw new System.Exception("Tunnel segment cannot have " + holeCount + " holes");
            }
        }

        /**
         * Given the previous direction, current direction and current tunnel create the next tunnel segment
         */
        public override Tunnel getTunnel()
        {
            List<Direction>holeDirections = collidedTunnel.holeDirectionList;

            string junctionId = Type.JUNCTION + " " + junctionCount;

            List<Direction> allHoleDirections = new List<Direction>(holeDirections);
            Direction newHoleDir = Dir.Base.getOppositeDirection(directionPair.prevDir);
            allHoleDirections.Add(newHoleDir);
            Transform junctionType = getJunction(allHoleDirections);

            GameObject junctionGO = gameObject.instantiate(cellMove.startPosition, Type.instance.TunnelNetwork, junctionType, directionPair, holeDirections, cellMove.cell, junctionId);
            Junction junctionTunnel = junctionGO.GetComponent<Junction>();
            junctionCount += 1;
            return junctionTunnel;
        }
    }

}