using UnityEngine;

namespace Worm
{
    public class AIWormTunnelBroker : WormTunnelBroker
    {
        private new void Awake()
        {
            base.Awake();
        }

        private new void OnEnable()
        {
            base.OnEnable();
            WormIntervalEvent += GetComponent<TunnelMaker>().onBlockInterval;
        }

        /**
         * Determines whether to send a worm interval event. In general yes, except for on the start of a new tunnel 
         * (only want to increment tunnel counter at the end of the segment) and when a existing tunnel is first entered
         * 
         * @curCell         the cell player is currently in
         * @isNewCell       exception checking
         */
        protected override bool isSendWormIntervalEvent(Vector3Int curCell, Tunnel.Tunnel curTunnel, bool isNewCell)
        {
            bool isInExistingCell = base.isSendWormIntervalEvent(curCell, curTunnel, isNewCell);

            bool isNewTunnelStart = isNewCell && (prevTunnel != curTunnel);     // if a new tunnel has been added

            if (wormBase.isChangingDirection)                                   
            {
                return false;                                                   // if in the middle of a turn DONT include this cell as a "new" tunnel. only straigh tunnels should be candiates for tunnel intervals
            }
            if (lastReachedWaypoint != null && isNewTunnelStart)
            {
                bool isStraight = lastReachedWaypoint.dirPair.isStraight();
                lastReachedWaypoint = null;
                return isStraight;                                              // if player is not emerging from a turn tunnel segment (is going straight, eg a new junction)
            }
            return isInExistingCell;                                            // if no waypoints were reached, use a single check to see if player has entered existing tunnel
        }

        private new void OnDisable()
        {
            base.OnDisable();
            WormIntervalEvent -= GetComponent<TunnelMaker>().onBlockInterval;
        }
    }
}