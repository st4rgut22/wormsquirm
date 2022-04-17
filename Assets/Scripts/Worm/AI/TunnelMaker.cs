using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Worm
{
    public class TunnelMaker : BaseController
    {
        public delegate void RespawnAi(string wormId);
        public event RespawnAi RespawnAiEvent;

        List<Checkpoint> checkpointList;
        int checkPointIdx;

        private int tunnelSegmentCounter;
        Checkpoint currentCheckpoint;

        private bool isReadyToTurn;             // used to time changeDirection events when the worm is ready to turn
        private bool isModifyPath;              // true when a new path should be generated, modifying the existing path. '

        public bool isInitDecision { get; private set; }           // true if decision is made to start going the path

        private Vector3Int turnOnFirstBlock;    // acts like a boolean flag, stores the last position, and the next new position will execute the turn. 
        Vector3Int defaultValue;

        private new void Awake()
        {
            base.Awake();
            tunnelSegmentCounter = 0; // maintains count of added segments onBlockInterval event to decide when to turn
            checkPointIdx = 0; // does not include the initial tunnel
            isModifyPath = false;
            isReadyToTurn = false;
            isInitDecision = false;
            defaultValue = new Vector3Int(1000, 1000, 1000); // temporary default values
            turnOnFirstBlock = defaultValue;
        }

        private new void OnEnable()
        {
            base.OnEnable();
            RespawnAiEvent += FindObjectOfType<Map.AiSpawnGenerator>().onRespawnAi;
            FindObjectOfType<Turn>().ReachWaypointEvent += onReachWaypoint;
        }

        /**
         * Check if a checkpoint list is valid, for example worm cannot go in the opposite direction
         */
        private bool isCheckpointListValid(List<Checkpoint> checkpointList, Direction wormDir)
        {
            if (wormDir != Direction.None)
            {
                Checkpoint firstCheckpoint = checkpointList[0];
                bool isDirectionOpposite = Dir.Base.getOppositeDirection(firstCheckpoint.direction) == wormDir;
                return !isDirectionOpposite;
            }
            return true;
        }

        /**
         * If path is modified while coming out of a turn, handle the checkpoint update on turn completion
         */
        public void onCompleteTurn(string wormId, Direction direction)
        {
            if (isModifyPath)
            {
                print("completed turn now modifying the path");
                updateCheckpoint();
            }
        }

        /**
         * Tunnel maker receives a checkpoint list for a worm to execute on initializing target position
         * OR Tunnel maker receives a checkpoint list showing the path to target's updated position
         * 
         * @isInitPath          if false, the worm has not created a tunnel yet and should emit an initialization event
         * @wormTunnelBroker    worm component that can describes worm-tunnel relationship
         */
        public void onInitCheckpointList(List<Checkpoint> checkpointList, bool isInitPath, WormTunnelBroker wormTunnelBroker)
        {
            Direction curDirection = wormTunnelBroker.getDirection();
            bool checkpointListIsValid = isCheckpointListValid(checkpointList, curDirection);
            if (!checkpointListIsValid)
            {
                return; // dont init new checkpoint list if it is invalid, exit early
            }
            initializePath(checkpointList);

            if (wormBase.isPendingTurn) // if next cell is turn, abort the turn, because it belongs to the old path
            {
                print("raise abort turn event");
                wormTunnelBroker.RaiseAbortTurnEvent();
            }
            if (!isInitPath)
            {
                //print("go dir " + currentCheckpoint.direction + " for length " + currentCheckpoint.length);
                isInitDecision = true;
                RaiseInitDecisionEvent(currentCheckpoint.direction);
            }
            else
            {
                print("go dir received a new checkpoint list");
                isModifyPath = true; // modify the existing path when the next block interval or turn exit is reached
            }
        }

        /**
         * Initialize path variables to start a new path
         * 
         * @checkpoints     the new list of checkpoints the worm should follow
         */
        private void initializePath(List<Checkpoint>checkpoints)
        {
            checkPointIdx = tunnelSegmentCounter = 0;
            checkpointList = checkpoints;
            currentCheckpoint = checkpointList[0];
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
                    if (isModifyPath)
                    {
                        updateCheckpoint();
                        return;
                    }
                    if (tunnel.containsCell(lastBlockPositionInt)) // this condition excludes the last cell adjacent from the corner as counting as a straight tunnel segment
                    {
                        tunnelSegmentCounter += 1;
                        print("go dir " + currentCheckpoint.direction + " on block interval increment tunnel segment counter to " + tunnelSegmentCounter + "," + (currentCheckpoint.length - tunnelSegmentCounter) + " segments to go until turn");
                        if (currentCheckpoint.length == tunnelSegmentCounter)
                        {
                            updateCheckpoint();
                        }
                    }
                }
            }
        }

        /**
         * After a checkpoint is reached update the checkpoint
         */
        private void updateCheckpoint()
        {
            checkPointIdx++;

            if (isModifyPath) // reset checkpoint if new checkpoint list is received
            {
                checkPointIdx = 0;
                isModifyPath = false;
            }
            if (checkPointIdx < checkpointList.Count)
            {
                tunnelSegmentCounter = 0;
                currentCheckpoint = checkpointList[checkPointIdx];
                if (currentCheckpoint.direction != wormBase.direction)
                {
                    RaisePlayerInputEvent(currentCheckpoint.direction);
                }
                else
                {
                    print("breaktime");
                }
                print(gameObject.name + " go dir " + currentCheckpoint.direction + " for length " + currentCheckpoint.length + " current pos is " + ring.transform.position);
            }
            else
            {
                ResetObjective();
            }
        }

        /**
         * After an AI has reached its objective (which depends on type of AI), it needs to decide a new course of action
         */
        private void ResetObjective()
        {
            switch (wormBase.WormDescription.wormType)
            {
                case ObstacleType.AIWorm: // while testing the AI worm will initially reset when reaching its destination
                    RespawnAiEvent(wormBase.wormId);
                    break;
                default:
                    throw new System.Exception("The AI of type " + wormBase.WormDescription.wormType + " has not been implemented yet");
            }
        }

        /**
         * Execute turn if block matches decision cell
         */
        IEnumerator changeDirectionConsecutively()
        {
            if (currentCheckpoint.length == 0) // not the last checkpoint
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

            if (FindObjectOfType<Map.SpawnGenerator>())
            {
                RespawnAiEvent -= FindObjectOfType<Map.AiSpawnGenerator>().onRespawnAi;
            }
            if (FindObjectOfType<Turn>())
            {
                FindObjectOfType<Turn>().ReachWaypointEvent -= onReachWaypoint;
            }
        }
    }
}
