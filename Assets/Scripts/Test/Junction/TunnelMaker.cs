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
        [SerializeField]
        private Rigidbody head;

        List<Checkpoint> checkpointList;
        int checkPointIdx;

        string wormId = "fakeId";
        private const float INSTANT_TURN = 1.0f;
        private int tunnelSegmentCounter;
        Checkpoint currentCheckpoint;

        public delegate void ChangeDirection(DirectionPair directionPair);
        public event ChangeDirection ChangeDirectionEvent;

        public delegate void InitDecision(Direction direction, string wormId);
        public event InitDecision InitDecisionEvent;

        public delegate void PlayerInput(Direction direction);
        public event PlayerInput PlayerInputEvent;

        private bool isReadyToTurn; // used to time changeDirection events when the worm is ready to turn

        private void Awake()
        {
            tunnelSegmentCounter = 1; // maintains count of added segments onBlockInterval event to decide when to turn
            checkPointIdx = 0; // does not include the initial tunnel
            Worm.InputProcessor.INPUT_SPEED = INSTANT_TURN; // turn instantly on one player input
            isReadyToTurn = false;
        }

        private void OnEnable()
        {
            if (FindObjectOfType<Worm.InputProcessor>())
            {
                PlayerInputEvent += FindObjectOfType<Worm.InputProcessor>().onPlayerInput;
            }
            InitDecisionEvent += Tunnel.CollisionManager.Instance.onInitDecision;
            InitDecisionEvent += FindObjectOfType<Worm.Turn>().onInitDecision;
        }

        /**
         * Tunnel maker receives a checkpoint list for a worm to execute
         * 
         * @TODO: worm id specifies which worm should follow the checkpoints
         */
        public void onInitCheckpointList(List<Checkpoint> checkpointList)
        {
            this.checkpointList = checkpointList;
            currentCheckpoint = this.checkpointList[0];
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


        /**
         * Flag set when the worm enters a turn and reaches a point in the turn where it is ready to receive another changeDirection event
         */
        public void onDecisionProcessing(bool isDecisionProcessing)
        {
            print("set decision processing to " + isDecisionProcessing);
            this.isReadyToTurn = !isDecisionProcessing;
        }

        public void onBlockInterval(bool isBlockInterval, Vector3Int blockPositionInt, Tunnel.Straight tunnel)
        {
            if (isBlockInterval)
            {
                tunnelSegmentCounter += 1;
                print("Added tunnel segment " + tunnelSegmentCounter);
                StartCoroutine(changeDirection());
            }
        }

        /**
         * After a checkpoint is reached update the checkpoint
         */
        private void updateCheckpoint()
        {
            checkPointIdx++;
            currentCheckpoint = checkpointList[checkPointIdx];
            tunnelSegmentCounter = 1; // reset the counter
            PlayerInputEvent(currentCheckpoint.direction); // go in new direction, but corner block wont be created until straight block has reached an interval length
        }

        /**
         * Execute turn if block matches decision cell
         */
        IEnumerator changeDirection()
        {
            while (currentCheckpoint.length == 1) // consecutive turn is made
            {
                if (isReadyToTurn) // break when the decision is done begin processed (eg worm reaches center of turn tunnel)
                {
                    updateCheckpoint();
                    isReadyToTurn = false;
                    break;
                }
                yield return null;
            }
            if (currentCheckpoint.length != 1 && currentCheckpoint.length == tunnelSegmentCounter)
            {
                if (checkPointIdx < checkpointList.Count)
                {
                    updateCheckpoint();
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
                InitDecisionEvent -= FindObjectOfType<Worm.Turn>().onInitDecision;
            }
        }
    }

}
