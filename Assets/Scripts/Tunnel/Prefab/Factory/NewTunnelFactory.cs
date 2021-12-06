using UnityEngine;

namespace Tunnel
{
    public class NewTunnelFactory : Factory
    {
        /**
         * Given the previous direction, current direction and current tunnel create the next tunnel segment
         * 
         * @directionPair is the prev and current direction of the player
         * @curTunnel is the current tunnel turn is made from
         * @gameObject is a reference to TunnelManager required for using the extension method Instantiate()
         */
        public Tunnel createTunnel(DirectionPair directionPair, GameObject gameObject, CellMove cellMove)
        {

            GameObject newTunnelGO = null;

            bool isTunnelStraight = directionPair.prevDir == directionPair.curDir;
            bool isTunnelInitialized = directionPair.prevDir != Direction.None;

            if (isTunnelInitialized && !isTunnelStraight)
            {
                Manager.cornerCount += 1;
                string cornerId = Type.CORNER + " " + Manager.cornerCount;
                newTunnelGO = gameObject.Instantiate(Type.instance.Corner, cellMove.startPosition, Type.instance.TunnelNetwork, directionPair, cellMove.cell, cornerId);
            }
            else // create a straight tunnel
            {
                Manager.straightCount += 1;
                string straightId = Type.STRAIGHT + " " + Manager.straightCount;
                newTunnelGO = gameObject.Instantiate(Type.instance.Straight, cellMove.startPosition, Type.instance.TunnelNetwork, directionPair, cellMove.cell, straightId);
            }

            if (newTunnelGO == null)
            {
                throw new System.Exception("new tunnel shouldn't be null");
            }
            Tunnel newTunnel = newTunnelGO.GetComponent<Tunnel>();
            return newTunnel;
        }
    }
}