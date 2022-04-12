using UnityEngine;

namespace Tunnel
{    
    public class Junction : TurnableTunnel
    {
        new private void Awake()
        {
            base.Awake();
        }

        public void onReachJunctionExit()
        {
            Destroy(DeadEndInstance);
        }

        /**
         * Add the hole created from the ingress direction to the holes of the previous junction if it hasn't already been added
         */
        public override void setHoleDirections(DirectionPair dirPair)
        {
            Direction newHoleDirection = Dir.Base.getOppositeDirection(dirPair.prevDir);
            if (!holeDirectionList.Contains(newHoleDirection))
            {
                holeDirectionList.Add(newHoleDirection);
            }
            setWallDirections();
        }

        public void OnDisable()
        {
            if (FindObjectOfType<ModTunnelFactory>())
            {
                FindObjectOfType<ModTunnelFactory>().AddTunnelEvent -= onAddTunnel;
            }
        }
    }

}