using System.Collections.Generic;
using UnityEngine;

namespace Worm
{
    public class AiPathFinder : MonoBehaviour
    {
        public delegate void InitObjective(Vector3Int objectiveCell);
        public event InitObjective InitObjectiveEvent;

        public delegate void FollowPath(TunnelMaker tunnelMaker, WormTunnelBroker wormTunnelBroker);
        public event FollowPath FollowPathEvent;

        private void OnEnable()
        {
            InitObjectiveEvent += FindObjectOfType<Map.Astar>().onInitObjective;
            FollowPathEvent += FindObjectOfType<Map.Astar>().onFollowPath;
        }

        /**
         * Command a single AI worm to chase another obstacle
         * 
         * @chaserWormId    the id of the worm that is the chaser
         * @obstacleId      the id of the obstacle being chased (eg another worm, or reward)
         * @destinationCell the cell the worm should go to 
         */
        public void setAiPath(string chaserWormId, string obstacleId, Vector3Int destinationCell)
        {
            InitObjectiveEvent(destinationCell);
            WormTunnelBroker chaserTunnelBroker = WormManager.Instance.WormTunnelBrokerDict[chaserWormId];
            TunnelMaker chaserTunnelMaker = WormManager.Instance.WormTunnelMakerDict[chaserWormId];
            FollowPathEvent(chaserTunnelMaker, chaserTunnelBroker);
        }

        /**
         * Command all AI to chase a obstacle
         * 
         * @obstacleId      the id of the obstacle being chased (eg another worm)
         */
        public void onBroadcastBeacon(string obstacleId)
        {
            WormTunnelBroker wormTunnelBroker = WormManager.Instance.WormTunnelBrokerDict[obstacleId];
            Vector3Int destinationCell = wormTunnelBroker.getCurrentCell();

            WormManager.Instance.WormIdList.ForEach((wormId) =>
            {
                setAiPath(wormId, obstacleId, destinationCell);
            });
        }

        private void OnDisable()
        {
            if (FindObjectOfType<Map.Astar>())
            {
                InitObjectiveEvent -= FindObjectOfType<Map.Astar>().onInitObjective;
                FollowPathEvent -= FindObjectOfType<Map.Astar>().onFollowPath;
            }            
        }
    }
}