using UnityEngine;

namespace Worm
{
    public class AIWormTunnelBroker : WormTunnelBroker
    {
        private new void OnEnable()
        {
            base.OnEnable();
            WormIntervalEvent += GetComponent<TunnelMaker>().onBlockInterval;
        }

        /**
         * Player sends event inside an existing tunnel
         * 
         * @isBlockInterval         flag is true if the player position reached a new block
         * @blockPositionInt        the cell coordinate in integers that the player has entered
         * @tunnel                  the tunnel the player has entered
         */
        protected override void sendWormIntervalEvent(bool isBlockInterval, Vector3Int blockPositionInt, Vector3Int lastBlockPositionInt, Tunnel.Tunnel tunnel)
        {
            base.sendWormIntervalEvent(isBlockInterval, blockPositionInt, lastBlockPositionInt, tunnel);
            RaiseWormIntervalEvent(isBlockInterval, blockPositionInt, lastBlockPositionInt, tunnel);
        }

        private new void OnDisable()
        {
            base.OnDisable();
            WormIntervalEvent -= GetComponent<TunnelMaker>().onBlockInterval;
        }
    }
}