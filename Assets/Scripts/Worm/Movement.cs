using UnityEngine;
using System.Collections.Generic;

namespace Worm
{
    /**
     * Worm movement
     */
    public class Movement: WormBody
    {
        private int waypointIndex;

        private List<Waypoint> waypointList;
        private List<Waypoint> nextWaypointList; // queued up waypoint list if turning while navigating a corner
    
        public delegate void CompleteTurn(string wormId, Direction direction); // when turn is completed notify Turn so we can proceed straight
        public event CompleteTurn CompleteTurnEvent;

        public delegate void ExitTurn(Direction direction, Tunnel.Tunnel tunnel); // exiting a turn into a straight segment
        public event ExitTurn ExitTurnEvent;

        public delegate void MoveToWaypoint(Waypoint waypoint); // isTurn flag allows us to override straight move
        public event MoveToWaypoint MoveToWaypointEvent;

        public delegate void BlockInterval(bool isBlockInterval, Vector3Int blockPositionInt, Tunnel.Straight tunnel);
        public event BlockInterval BlockIntervalEvent;

        public delegate void DecisionProcessing(bool isDecisionProcessing, Waypoint waypoint);
        public event DecisionProcessing DecisionProcessingEvent;

        public delegate void AddForce(Rigidbody rigidbody, Vector3 forceVector);
        public event AddForce ForceEvent;

        public delegate void ReachJunctionExit();
        public event ReachJunctionExit ReachJunctionExitEvent;

        public delegate void Telemetry(Vector3 position, Quaternion quaternion);
        public event Telemetry TelemetryEvent;

        private new void Awake()
        {
            base.Awake();
            waypointList = new List<Waypoint>();
            nextWaypointList = new List<Waypoint>();
            transform.position = wormBase.initialCell;
        }

        private void OnEnable() 
        {
            ForceEvent += GetComponent<Force>().onForce;
            ExitTurnEvent += GetComponent<Turn>().onExitTurn;
            MoveToWaypointEvent += GetComponent<Turn>().onMoveToWaypoint;
            DecisionProcessingEvent += GetComponent<InputProcessor>().onDecisionProcessing;
            DecisionProcessingEvent += GetComponent<WormTunnelBroker>().onDecisionProcessing;
            if (FindObjectOfType<TunnelMaker>()) // applies to AI
            {
                DecisionProcessingEvent += FindObjectOfType<TunnelMaker>().onDecisionProcessing;
            }
        }

        private void FixedUpdate()
        {
            if (wormBase.direction != Direction.None)
            {
                Vector3 forceDir = -head.transform.up; // negative because worm's forward vector is opposite world space forward vector
                Debug.Log("go in the direction" + forceDir);
                ForceEvent(head, forceDir); // make the worm go striaght ahead
            }
        }

        private void clearWaypoints(List<Waypoint> waypoints)
        {
            waypoints.Clear();
        }

        /**
         * Go straight through junction to opposite end of junction
         */
        public void goStraightThroughJunction(Tunnel.Tunnel tunnel)
        {
            Vector3 egressPosition = Tunnel.Tunnel.getOffsetPosition(wormBase.direction, tunnel.center);
            print("egress position of straight junction is " + egressPosition);
            DirectionPair straightDirPair = new DirectionPair(wormBase.direction, wormBase.direction);
            Waypoint exitWP = new Waypoint(egressPosition, MoveType.EXIT, straightDirPair);
            List<Waypoint> waypointList = new List<Waypoint>() { exitWP };
            onFollowWaypoint(waypointList, wormBase.direction);
        }

        /** 
         * Finish going straight to the exit point of a junction
         */
        private void completeStraight(Waypoint waypoint)
        {
            Tunnel.Junction junction = (Tunnel.Junction) (Tunnel.Map.getCurrentTunnel(clit.position));  // use body position to get tunnel, because cannot rely on always being in a growing tunnel where block position is received from tunnel
            ReachJunctionExitEvent += junction.onReachJunctionExit;
            ReachJunctionExitEvent();
            ReachJunctionExitEvent -= junction.onReachJunctionExit;
            DirectionPair straightDirPair = new DirectionPair(wormBase.direction, wormBase.direction); // create a straight tunnel after navigating through a junction 
            RaiseChangeDirectionEvent(straightDirPair, junction, wormId);
            waypointList.Clear();
        }

        /**
         * Finish a turn and check if any turns are queued up. If so return the first waypoint of the next turn 
         * 
         * @waypoint The waypoint that completes the turnonf
         * @returns a list of waypoints for the next turn
         */
        private void completeTurn(Waypoint waypoint)
        {
            if (waypointIndex < waypointList.Count - 1)
            {
                throw new System.Exception("completing turn should be the last action in waypoint list");
            }

            Tunnel.Tunnel tunnel = Tunnel.Map.getCurrentTunnel(clit.position);

            CompleteTurnEvent += ((Tunnel.TurnableTunnel)tunnel).onCompleteTurn;
            CompleteTurnEvent(wormId, wormBase.direction); // set the new direction as the exit direction
            CompleteTurnEvent -= ((Tunnel.TurnableTunnel)tunnel).onCompleteTurn;

            clearWaypoints(waypointList);
            if (nextWaypointList.Count > 0) // additional turns
            {
                List<Waypoint> newWaypointList = new List<Waypoint>(nextWaypointList);
                clearWaypoints(nextWaypointList);
                onFollowWaypoint(newWaypointList, wormBase.direction); // set this immediate turn waypoint list as the next waypoints to follow
            }
            else // no immediate turn
            {
                if (tunnel.type == Tunnel.Type.Name.STRAIGHT)
                {
                    throw new System.Exception("not a turning tunnel. it is " + tunnel.name);
                }
                ExitTurnEvent(wormBase.direction, tunnel);
            }
        }

        /**
         * When rigidbody reaches waypoint go to next waypoint or complete waypoints
         */
        public void onReachWaypoint(Waypoint waypoint)
        {
            if (waypoint.move == MoveType.EXIT)
            {
                if (wormBase.isStraight)
                {
                    print("complete straight");
                    completeStraight(waypoint);
                }
                else
                {
                    completeTurn(waypoint); // complete the current turn and determine what next move is (navigating out of turn or turning again) :)
                }
            }
            else
            {
                if (waypoint.move == MoveType.ENTRANCE)
                {
                    DecisionProcessingEvent(true, waypoint); // no turns allowed when entering a turn (from current position to center of the turn tunnel segment)
                }
                else if (waypoint.move == MoveType.CENTER)
                {
                    print("apply up force to the ring rgbdy");
                    DecisionProcessingEvent(false, waypoint); // allow decisions to be made again when center of tunnel is reached (eg a consecutive turn)
                }
                else
                {
                    throw new System.Exception("not a valid waypoint type " + waypoint.move);
                }
                moveToNextWaypoint(waypoint);
            }
        }

        /**
         * Move to the next waypoint if it is not the last waypoint
         * 
         * @prevWaypoint the waypoint we have reached
         */
        private void moveToNextWaypoint(Waypoint prevWaypoint)
        {
            waypointIndex = waypointList.FindIndex(wp => prevWaypoint.Equals(wp)); // EXIT pos of prevCell equals the ENTER pos of current cell
            print("waypoint index is " + waypointIndex);
            if (waypointIndex == waypointList.Count - 1)
            {
                throw new System.Exception("last waypoint should not execute moveToNextWaypoint. Last waypoint should be of type EXIT not " + prevWaypoint.move);
            } 
            waypointIndex += 1;
            Waypoint curWaypoint = waypointList[waypointIndex]; // exit waypoint equals center waypoint WHY?
            MoveToWaypointEvent(curWaypoint);
        }

        /**
         * Follow waypoints when navigating corners. If already following waypoints, queue up the next waypoints list
         * 
         * @waypointList a list of coordinates the worm follows to navigate a corner
         */
        public void onFollowWaypoint(List<Waypoint> waypointList, Direction egressDirection)
        {
            print("Egress dir is " + egressDirection);
            if (this.waypointList.Count > 0) // queue up waypoint list if currently following one
            {
                nextWaypointList = waypointList;
            }
            else // if not following waypoint list, follow the new waypoint list
            {
                this.waypointList = waypointList;
                Waypoint firstWaypoint = this.waypointList[0];
                MoveToWaypointEvent(firstWaypoint);
            }
        }

        private new void OnDisable()
        {
            ForceEvent -= GetComponent<Force>().onForce;
            ExitTurnEvent -= GetComponent<Turn>().onExitTurn;
            MoveToWaypointEvent -= GetComponent<Turn>().onMoveToWaypoint;

            DecisionProcessingEvent -= GetComponent<InputProcessor>().onDecisionProcessing;

            if (GetComponent<TunnelMaker>()) // applies to AI
            {
                DecisionProcessingEvent -= GetComponent<TunnelMaker>().onDecisionProcessing;
            }
            if (GetComponent<WormTunnelBroker>())
            {
                DecisionProcessingEvent -= GetComponent<WormTunnelBroker>().onDecisionProcessing;
            }
        }
    }
}
