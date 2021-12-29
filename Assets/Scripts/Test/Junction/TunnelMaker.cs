using System.Collections.Generic;
using UnityEngine;

namespace Test
{
    /**
     * A testing class that confirms orientation of junctions + # of holes is correct 
     */
    public class TunnelMaker : MonoBehaviour
    {
        Direction initialDir = Direction.Up;
        List<Checkpoint> checkpointList;
        int checkPointIdx = 0;

        public delegate void ChangeDirection(DirectionPair directionPair);
        public event ChangeDirection ChangeDirectionEvent;

        private void OnEnable()
        {
            ChangeDirectionEvent += FindObjectOfType<Worm.Movement>().onChangeDirection;
        }

        /**
         * Initiate movement
         */
        private void Start()
        {
            checkpointList = ExampleNetwork.threeIntersectLoopStraight;

            ChangeDirectionEvent(new DirectionPair(Direction.None, initialDir));
        }

        /**
         * Since block interval events don't include corners which can also be decision cells, need to listen for corner creation
         */
        public void onAddTunnel(Tunnel.Tunnel tunnel, Vector3Int cell, DirectionPair directionPair)
        {
            if (tunnel.tag == Tunnel.Type.CORNER)
            {
                changeDirection(cell);
            }
        }

        public void onBlockInterval(bool isBlockInterval, Vector3Int blockPositionInt, Tunnel.Straight tunnel)
        {
            if (isBlockInterval)
            {
                changeDirection(blockPositionInt);
            }
        }

        /**
         * Execute turn if block matches decision cell
         */
        private void changeDirection(Vector3Int blockPositionInt)
        {
            if (checkPointIdx < checkpointList.Count)
            {
                Checkpoint cp = checkpointList[checkPointIdx];
                if (cp.decisionCell.Equals(blockPositionInt))
                {
                    checkPointIdx++;
                    ChangeDirectionEvent(cp.dirPair);

                    Vector3Int turnCell = cp.decisionCell.getNextVector3Int(cp.dirPair.prevDir);

                    // append straight tunnel segment to the corner unless a consecutive turn is made
                    if (checkPointIdx < checkpointList.Count)
                    {
                        Checkpoint nextCP = checkpointList[checkPointIdx];
                        if (!nextCP.decisionCell.Equals(turnCell))
                        {
                            DirectionPair straightDirPair = new DirectionPair(cp.dirPair.curDir, cp.dirPair.curDir);
                            ChangeDirectionEvent(straightDirPair);
                        }
                    }
                }
            }
        }

        // Start is called before the first frame update
        private void OnDisable()
        {
            if (FindObjectOfType<Tunnel.Manager>())
            {
                ChangeDirectionEvent -= FindObjectOfType<Worm.Movement>().onChangeDirection;
            }
        }
    }

}
