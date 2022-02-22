using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Worm
{
    public class TunnelMaker : BaseController
    {

        [SerializeField]
        private Rigidbody head;

        List<Checkpoint> checkpointList;
        int checkPointIdx;

        private int tunnelSegmentCounter;
        Checkpoint currentCheckpoint;

        private bool isReadyToTurn;             // used to time changeDirection events when the worm is ready to turn
        private Vector3Int turnOnFirstBlock;    // acts like a boolean flag, stores the last position, and the next new position will execute the turn. 
        Vector3Int defaultValue;

        private new void Awake()
        {
            base.Awake();
            tunnelSegmentCounter = 1; // maintains count of added segments onBlockInterval event to decide when to turn
            checkPointIdx = 0; // does not include the initial tunnel
            isReadyToTurn = false;
            defaultValue = new Vector3Int(1000, 1000, 1000); // temporary default values
            turnOnFirstBlock = defaultValue;
        }

        private new void OnEnable()
        {
            base.OnEnable();

            FindObjectOfType<Turn>().ReachWaypointEvent += onReachWaypoint;
            ObjectiveReachedEvent += GameManager.Instance.onObjectiveReached;
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
            RaiseInitDecisionEvent(currentCheckpoint.direction);
        }

        public void onReachWaypoint(Waypoint waypoint)
        {
            if (currentCheckpoint.length == 0 && waypoint.move == MoveType.CENTER)
            {
                StartCoroutine(changeDirectionConsecutively());
            }
        }

        /**
         * Flag set when the worm enters a turn and reaches a point in the turn where it is ready to receive another changeDirection event
         */
        public void onDecisionProcessing(bool isDecisionProcessing, Waypoint waypoint)
        {
            print("set decision processing to " + isDecisionProcessing);
            isReadyToTurn = !isDecisionProcessing;
        }

        /**
         * Event received from a growing tunnel or a worm in an existing tunnel
         * 
         * @isBlockInterval         flag if the tunnel/worm has reached a cell interval
         * @blockPositionInt        the integer coordinates of a block tunnel segment
         * @tunnel                  the tunnel being traversed throughs
         */
        public void onBlockInterval(bool isBlockInterval, Vector3Int blockPositionInt, Vector3Int lastBlockPositionInt, Tunnel.Tunnel tunnel)
        {
            if (Tunnel.Type.isTypeStraight(tunnel.type)) // worm may send interval messages in non-straight tunnels, ignore these message
            {
                if (isBlockInterval)
                {
                    if (!isTurnOnFirstBlock())
                    {
                        if (tunnel.containsCell(blockPositionInt)) // this condition excludes the last cell adjacent from the corner as counting as a tunnel segment
                        {
                            tunnelSegmentCounter += 1;

                            if (currentCheckpoint.length == tunnelSegmentCounter)
                            {
                                updateCheckpoint();
                            }
                            if (currentCheckpoint.length == 1) // next checkpoint is of length 1, set a flag
                            {
                                turnOnFirstBlock = blockPositionInt;
                            }
                        }
                    }
                }
                else if (isTurnOnFirstBlock() && !blockPositionInt.Equals(turnOnFirstBlock)) // checkpoint of length 1 is reached, when a new straight tunnel segment is started
                {
                    updateCheckpoint();
                    turnOnFirstBlock = defaultValue; // reset the flag
                }
            }
        }

        /**
         * Set true if player will turn on the first block interval of a straight tunnel segment
         */
        private bool isTurnOnFirstBlock()
        {
            return !turnOnFirstBlock.Equals(defaultValue);
        }

        /**
         * After a checkpoint is reached update the checkpoint
         */
        private void updateCheckpoint()
        {
            checkPointIdx++;
            if (checkPointIdx < checkpointList.Count)
            {
                tunnelSegmentCounter = 1;
                currentCheckpoint = checkpointList[checkPointIdx];
                RaisePlayerInputEvent(currentCheckpoint.direction);                
                print("go dir " + currentCheckpoint.direction + " for length " + currentCheckpoint.length);
            }
            else
            {
                RaiseObjectiveReachedEvent(); // reached the goal
                RaiseRemoveSelfEvent();
                RaiseSpawnEvent();
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
        private new void OnDisable()
        {
            base.OnDisable();
            ObjectiveReachedEvent -= GameManager.Instance.onObjectiveReached;
       
            if (FindObjectOfType<Turn>())
            {
                FindObjectOfType<Turn>().ReachWaypointEvent -= onReachWaypoint;
            }
        }
    }
}
