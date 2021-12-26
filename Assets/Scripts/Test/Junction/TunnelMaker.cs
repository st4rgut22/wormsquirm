using System.Collections;
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
            ChangeDirectionEvent += FindObjectOfType<Tunnel.Manager>().onChangeDirection;
        }

        /**
         * Initiate movement
         */
        private void Start()
        {
            checkpointList = ExampleNetwork.threeIntersectLoop;

            ChangeDirectionEvent(new DirectionPair(Direction.None, initialDir));
        }

        public void onBlockInterval(bool isBlockInterval, Vector3Int blockPositionInt, Tunnel.Straight tunnel)
        {
            if (isBlockInterval && checkPointIdx < checkpointList.Count)
            {
                Checkpoint cp = checkpointList[checkPointIdx];
                if (cp.decisionCell.Equals(blockPositionInt))
                {
                    ChangeDirectionEvent(cp.dirPair);

                    DirectionPair straightDirPair = new DirectionPair(cp.dirPair.curDir, cp.dirPair.curDir);
                    ChangeDirectionEvent(straightDirPair);

                    checkPointIdx++;
                }
            }
        }

        // Start is called before the first frame update
        private void OnDisable()
        {
            if (FindObjectOfType<Tunnel.Manager>())
            {
                ChangeDirectionEvent -= FindObjectOfType<Tunnel.Manager>().onChangeDirection;
            }
        }
    }

}
