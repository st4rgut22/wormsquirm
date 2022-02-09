using UnityEngine;
using System.Collections.Generic;

namespace Tunnel
{
    public class NewTunnelFactory : Factory
    {
        public static int straightCount;
        public static int cornerCount;

        public static NewTunnelFactory instance;

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
         * Receives create tunnel events including replacing tunnels
         * 
         * @CellMove the position of the next cell
         * @directionPair the next ingress/egress directions
         */
        public void onCreateTunnel(CellMove cellMove, DirectionPair directionPair, Tunnel prevTunnel, string wormId)
        {
            this.cellMove = cellMove;
            this.directionPair = directionPair;
            Tunnel tunnel = getTunnel();
            tunnel.setWormCreatorId(wormId);

            addTunnel(tunnel, wormId); // for corners, must wait until the straight tunnel has stopped growing
        }

        /**
         * Given the previous direction, current direction and current tunnel create the next tunnel segment
         * 
         * @directionPair is the prev and current direction of the player
         * @curTunnel is the current tunnel turn is made from
         * @gameObject is a reference to TunnelManager required for using the extension method Instantiate()
         */
        public override Tunnel getTunnel()
        {
            GameObject newTunnelGO;

            bool isTunnelStraight = directionPair.prevDir == directionPair.curDir;
            bool isTunnelInitialized = directionPair.prevDir != Direction.None;
            List<Direction> egressDirectionList = new List<Direction>() { directionPair.curDir };

            if (isTunnelInitialized && !isTunnelStraight)
            {
                cornerCount += 1;
                string cornerId = Type.CORNER + " " + cornerCount;
                newTunnelGO = gameObject.instantiate(cellMove.startPosition, Type.instance.TunnelNetwork, Type.instance.Corner, directionPair, egressDirectionList, cornerId);
            }
            else // create a straight tunnel
            {
                straightCount += 1;
                string straightId = Type.STRAIGHT + " " + straightCount;
                newTunnelGO = gameObject.instantiate(cellMove.startPosition, Type.instance.TunnelNetwork, Type.instance.Straight, directionPair, egressDirectionList, straightId);
            }

            Tunnel newTunnel = newTunnelGO.GetComponent<Tunnel>();

            return newTunnel;
        }
    }
}