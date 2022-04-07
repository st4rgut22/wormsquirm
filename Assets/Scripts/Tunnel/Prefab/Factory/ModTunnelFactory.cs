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

        public static ModTunnelFactory instance;

        private void Awake()
        {
            if (instance == null)
            {
                instance = this;
                DontDestroyOnLoad(instance);
            }
            else if (instance != this)
            {
                Destroy(gameObject);
            }
        }

        /**
         * Create a junction where the player is initialized to allow entrance/exit from all sides
         * 
         * @dirPair the current and next direction through the junction
         * @cellMove information about the cell of the new junction
         * @ingressDir is the direction the player enters the tunnel
         * @holeDirections the list of directions wherre the junction has holes
         */
        public void onCreateJunctionOnInit(DirectionPair dirPair, CellMove cellMove, Direction ingressDir, List<Direction> allHoleDirections, string playerId)
        {
            Direction tunnelIngressHoleDir = Dir.Base.getOppositeDirection(ingressDir); // the direction of ingress hole from the tunnel's perspective (opposite the worm)
            List<Direction> egressHoleDirections = new List<Direction>(allHoleDirections); // makee a copy of all hole list
            egressHoleDirections.Remove(tunnelIngressHoleDir);
            initializeJunction(dirPair, cellMove);
            string junctionId = getTunnelId(Type.JUNCTION, junctionCount);
            Tunnel tunnel = getJunction(allHoleDirections, egressHoleDirections, junctionId);
            addTunnel(tunnel, playerId);
        }

        /**
         * Initialize the direction pair and cell information about the new junction
         * 
         * @dirPair the current and next direction to take through junction
         * @cellMove junction cell information
         */
        private void initializeJunction(DirectionPair dirPair, CellMove cellMove)
        {
            directionPair = dirPair;
            this.cellMove = cellMove;
        }

        /**
         * Fill the void left by the intersection of two tunnels
         * 
         * @collisionTunnel the tunnel that caused the collision
         * @dirPair the current and next direction through the junction
         * @cellMove information about the cell of the new junction
         */
        public void onCreateJunctionOnCollision(Tunnel collisionTunnel, DirectionPair dirPair, CellMove cellMove, string playerId)
        {
            collidedTunnel = collisionTunnel;
            initializeJunction(dirPair, cellMove);
            Tunnel tunnel = getTunnel();
            addTunnel(tunnel, playerId);
        }

        private Transform getJunctionTransform(List<Direction> holeDirectionList)
        {
            int holeCount = holeDirectionList.Count;
            bool isDirAdjacent = Dir.Base.areDirectionsAdjacent(holeCount, holeDirectionList);
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
         * Add holes from worm entrance to 
         */
        private bool addHoleDirs(List<Direction> allHoleList, List<Direction>egressHoleList, Direction inDirHole, Direction egressHole)
        {
            bool isNewHolesAdded = false;
            if (!allHoleList.Contains(inDirHole)) // add the ingress hole to the list of all holes if not already included
            {
                isNewHolesAdded = true;
                allHoleList.Add(inDirHole);
            }
            if (!allHoleList.Contains(egressHole)) // add the egress hole to the list of all holes if not already included
            {
                isNewHolesAdded = true;
                allHoleList.Add(egressHole);

                if (egressHoleList.Contains(egressHole))
                {
                    throw new System.Exception("Egress hole list should not include the egress hole " + egressHole);
                }
                egressHoleList.Add(egressHole);
            }
            if (egressHoleList.Contains(inDirHole)) // exclude the ingress hole from the egress hole list. we will use list to get the correct orientation of junction
            {
                egressHoleList.Remove(inDirHole);
            }
            return isNewHolesAdded;
        }

        /**
         * Instantiate and get the junction
         * 
         * @allHoleDirections the list of directions where the junction has holes is used to get the type of junction
         * @egressHoleDirections the list of directions in the junction excluding the entrance hole
         * @junctionId the id of the junction
         */
        private Tunnel getJunction(List<Direction> allHoleDirections, List<Direction> egressHoleDirections, string junctionId)
        {
            Transform junctionTransform = getJunctionTransform(allHoleDirections);
            GameObject junctionGO = gameObject.instantiate(cellMove.startPosition, Type.instance.TunnelNetwork, junctionTransform, directionPair, egressHoleDirections, junctionId);
            Junction junctionTunnel = junctionGO.GetComponent<Junction>();
            junctionCount += 1;
            return junctionTunnel;
        }

        /**
         * Given the previous direction, current direction and current tunnel create the next tunnel segment
         */
        public override Tunnel getTunnel()
        {
            List<Direction>holeDirections = collidedTunnel.holeDirectionList;
            print("get junction with hole directions " + holeDirections);
            string junctionId = getTunnelId(Type.JUNCTION, junctionCount);            

            List<Direction> allHoleDirections = new List<Direction>(holeDirections);
            Direction ingressHoleDir = Dir.Base.getOppositeDirection(directionPair.prevDir);
            bool isJunctionNew = addHoleDirs(allHoleDirections, holeDirections, ingressHoleDir, directionPair.curDir);

            if (isJunctionNew)
            {
                return getJunction(allHoleDirections, holeDirections, junctionId);
            }
            else
            {
                return collidedTunnel;
            }
        }
    }
}