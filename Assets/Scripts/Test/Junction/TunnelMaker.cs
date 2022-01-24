using System.Collections.Generic;
using UnityEngine;

namespace Test
{
    /**
     * A testing class that confirms orientation of junctions + # of holes is correct 
     */
    public class TunnelMaker : MonoBehaviour
    {      
        List<Checkpoint> checkpointList;
        int checkPointIdx;

        string wormId = "wormId";
        private const float INSTANT_TURN = 1.0f;
        private int tunnelSegmentCounter;
        Checkpoint currentCheckpoint;

        public delegate void ChangeDirection(DirectionPair directionPair);
        public event ChangeDirection ChangeDirectionEvent;

        public delegate void InitDecision(Direction direction, string wormId);
        public event InitDecision InitDecisionEvent;

        public delegate void PlayerInput(Direction direction);
        public event PlayerInput PlayerInputEvent;

        private void Awake()
        {
            tunnelSegmentCounter = 1; // maintains count of added segments onBlockInterval event to decide when to turn
            checkPointIdx = 0; // does not include the initial tunnel
            Worm.InputProcessor.INPUT_SPEED = INSTANT_TURN; // turn instantly on one player input
        }

        private void OnEnable()
        {
            PlayerInputEvent += FindObjectOfType<Worm.InputProcessor>().onPlayerInput;
            InitDecisionEvent += Tunnel.CollisionManager.Instance.onInitDecision;
            InitDecisionEvent += FindObjectOfType<Worm.Turn>().onInitDecision;
        }

        /**
         * Initiate movement
         */
        private void Start()
        {
            checkpointList = ExampleNetwork.threeIntersectLoopCorner;
            currentCheckpoint = checkpointList[0];

            InitDecisionEvent(currentCheckpoint.direction, wormId);
        }

        /**
         * Since block interval events don't include corners which can also be decision cells, need to listen for corner creation
         */
        public void onAddTunnel(Tunnel.Tunnel tunnel, Vector3Int cell, DirectionPair directionPair, string wormId)
        {
            if (tunnel.tag == Tunnel.Type.CORNER)
            {
                changeDirection();
            }
        }

        public void onBlockInterval(bool isBlockInterval, Vector3Int blockPositionInt, Tunnel.Straight tunnel)
        {
            if (isBlockInterval)
            {
                tunnelSegmentCounter += 1;
                print("Added tunnel segment " + tunnelSegmentCounter);
                changeDirection();
            }
        }

        /**
         * Execute turn if block matches decision cell
         */
        private void changeDirection()
        {
            if (currentCheckpoint.length == tunnelSegmentCounter)
            {
                checkPointIdx++;
                if (checkPointIdx < checkpointList.Count)
                {
                    currentCheckpoint = checkpointList[checkPointIdx];

                    tunnelSegmentCounter = 1; // reset the counter
                    PlayerInputEvent(currentCheckpoint.direction); // go in new direction, but corner block wont be created until straight block has reached an interval length
                }
            }
        }

        // Start is called before the first frame update
        private void OnDisable()
        {
            if (FindObjectOfType<Worm.InputProcessor>())
            {
                PlayerInputEvent -= FindObjectOfType<Worm.InputProcessor>().onPlayerInput;
            }
            if (FindObjectOfType<Tunnel.Map>())
            {
                InitDecisionEvent -= Tunnel.CollisionManager.Instance.onInitDecision;
            }
            if (FindObjectOfType<Worm.Turn>())
            {
                InitDecisionEvent += FindObjectOfType<Worm.Turn>().onInitDecision;
            }
        }
    }

}
