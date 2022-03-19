using System.Collections;
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

        private Obstacle targetObstacle;
        private const int RETARGET_INTERVAL = 5; // time interval to re-lockon the target

        private string playerId;

        private void OnEnable()
        {
            InitObjectiveEvent += FindObjectOfType<Map.Astar>().onInitObjective;
            FollowPathEvent += FindObjectOfType<Map.Astar>().onFollowPath;
        }

        private void Start()
        {
            StartCoroutine(BroadcastBeacon());
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
         * Listener is triggered when a target has been acquired
         * @targetId    the id of the target
         */
        public void onTargetReceived(string targetId)
        {
            targetObstacle = new Obstacle(targetId);
        }

        /**
         * Add a worm to the dictionary of worms when it is first instantiated
         */
        public void onInitPlayerWorm(Worm worm, Map.Astar wormAstar, string wormId)
        {
            playerId = wormId;
            onTargetReceived(playerId); // TODO: Testing code, target should be acquired on some event like picking up a reward or getting within range of enemy
        }

        /**
         * Command all AI to chase a obstacle
         * 
         * @obstacleId      the id of the obstacle being chased (eg another worm)
         */
        private IEnumerator BroadcastBeacon()
        {
            string wormPlayerId = WormManager.Instance.getPlayerWormId();

            // wait until the target has been received
            while (targetObstacle == null)
            {
                yield return null;
            }

            while (true) // TODO: add end condition here
            {
                Vector3Int lockOnCell = Map.ObstacleGenerator.swappedObstacleDict[targetObstacle];
                print("lock on cell is " + lockOnCell);
                WormManager.Instance.WormIdList.ForEach((wormId) => // TODO: Set conditions for notifying AIworms such as distance from worm
                {
                    if (wormId != targetObstacle.obstacleId && wormId != wormPlayerId) // dont chase yourself, cannot command player to follow a path
                    {
                        setAiPath(wormId, targetObstacle.obstacleId, lockOnCell);
                    }
                });
                yield return new WaitForSeconds(RETARGET_INTERVAL);
            }
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