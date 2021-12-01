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
        public Tunnel getTunnel(DirectionPair directionPair, GameObject gameObject, CellMove cellMove)
        {

            GameObject newTunnelGO = null;

            bool isTunnelStraight = directionPair.prevDir == directionPair.curDir;
            bool isTunnelInitialized = directionPair.prevDir != Direction.None;

            if (isTunnelInitialized && !isTunnelStraight)
            {
                newTunnelGO = gameObject.Instantiate(Env.instance.Corner, cellMove.startPosition, Env.instance.TunnelNetwork, directionPair, cellMove.nextCell);
            }
            else // create a straight tunnel
            {
                newTunnelGO = gameObject.Instantiate(Env.instance.Straight, cellMove.startPosition, Env.instance.TunnelNetwork, directionPair, cellMove.nextCell);
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