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

        public delegate void InitDecision(Direction direction, string wormId, Vector3Int initialCell);
        public event InitDecision InitDecisionEvent;

        public delegate void PlayerInput(Direction direction);
        public event PlayerInput PlayerInputEvent;

        private bool isReadyToTurn; // used to time changeDirection events when the worm is ready to turn
        private string currentTunnelName;

        Vector3Int initialCell = Vector3Int.zero;

        private void Awake()
        {
            tunnelSegmentCounter = 1; // maintains count of added segments onBlockInterval event to decide when to turn
            checkPointIdx = 0; // does not include the initial tunnel
            Worm.InputProcessor.INPUT_SPEED = INSTANT_TURN; // turn instantly on one player input
            isReadyToTurn = false;
            currentTunnelName = "";
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
            print("go dir " + currentCheckpoint.direction + " for length " + currentCheckpoint.length);
            InitDecisionEvent(currentCheckpoint.direction, wormId, initialCell);
        }

        /**
         * Since block interval events don't include corners which can also be decision cells, need to listen for corner creation
         */
        public void onAddTunnel(Tunnel.Tunnel tunnel, Vector3Int cell, DirectionPair directionPair, string wormId)
        {
            if (tunnel.tag == Tunnel.Type.CORNER)
            {
                changeDirectionConsecutively();
            }
        }

        /**
         * Flag set when the worm enters a turn and reaches a point in the turn where it is ready to receive another changeDirection event
         */
        public void onDecisionProcessing(bool isDecisionProcessing)
        {
            print("set decision processing to " + isDecisionProcessing);
            isReadyToTurn = !isDecisionProcessing;
        }

        public void onBlockInterval(bool isBlockInterval, Vector3Int blockPositionInt, Tunnel.Straight tunnel)
        {
            if (isBlockInterval)
            {
                if (tunnel.containsCell(blockPositionInt))
                {
                    tunnelSegmentCounter += 1;

                    if (currentCheckpoint.length == tunnelSegmentCounter && checkPointIdx < checkpointList.Count)
                    {
                        updateCheckpoint();
                    }
                }
            }
            else if (currentCheckpoint.length == 1 && tunnel.name != currentTunnelName) // start of next tunnel
            {
                updateCheckpoint();
            }

            currentTunnelName = tunnel.name;
        }

        /**
         * After a checkpoint is reached update the checkpoint
         */
        private void updateCheckpoint()
        {
            checkPointIdx++;
            tunnelSegmentCounter = 1;
            currentCheckpoint = checkpointList[checkPointIdx];
            PlayerInputEvent(currentCheckpoint.direction); // go in new direction, but corner block wont be created until straight block has reached an interval length
            print("go dir " + currentCheckpoint.direction + " for length " + currentCheckpoint.length);
            if (currentCheckpoint.length == 0)
            {
                StartCoroutine(changeDirectionConsecutively());
            }
        }

        /**
         * Execute turn if block matches decision cell
         */
        IEnumerator changeDirectionConsecutively()
        {
            if (currentCheckpoint.length == 0)
            {
                while (true)
                {
                    if (isReadyToTurn) // break when the decision is done begin processed (eg worm reaches center of turn tunnel)
                    {
                        updateCheckpoint();
                        isReadyToTurn = false;
                        break;
                    }
                    yield return null;
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
