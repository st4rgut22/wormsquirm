using UnityEngine;
using System.Collections.Generic;

namespace Worm
{
    /**
     * Worm movement
     */
    public class Movement: WormBody
    {
        private Direction egressWaypointDirection; // direction exiting a corner, saved on receipt of followWaypoints event

        private int waypointIndex;

        private List<Waypoint> waypointList;
        private List<Waypoint> nextWaypointList; // queued up waypoint list if turning while navigating a corner

        public delegate void CompleteTurn(string wormId, Direction direction); // when turn is completed notify Turn so we can proceed straight
        public event CompleteTurn CompleteTurnEvent;

        public delegate void ExitTurn(Direction direction); // exiting a turn into a straight segment
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

        private new void Awake()
        {
            base.Awake();
            waypointList = new List<Waypoint>();
            nextWaypointList = new List<Waypoint>();
            transform.position = initialCell;
        }

        private new void OnEnable() 
        {
            Tunnel.CollisionManager.Instance.InitWormPositionEvent += onInitWormPosition;

            ForceEvent += GetComponent<Force>().onForce;
            ExitTurnEvent += GetComponent<Turn>().onExitTurn;
            MoveToWaypointEvent += GetComponent<Turn>().onMoveToWaypoint;
            DecisionProcessingEvent += GetComponent<InputProcessor>().onDecisionProcessing;
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

        public void onInitWormPosition(Vector3 initPos, Direction direction)
        {
            wormBase.setDirection(direction);
            float offset = Tunnel.TunnelManager.Instance.START_TUNNEL_RING_OFFSET;
            Vector3 offsetVector = Dir.CellDirection.getUnitVectorFromDirection(direction);
            ring.position = initPos + offset * offsetVector;
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
            Vector3 egressPosition = Tunnel.Tunnel.getEgressPosition(wormBase.direction, tunnel.center);
            print("egress position of straight junction is " + egressPosition);
            DirectionPair straightDirPair = new DirectionPair(wormBase.direction, wormBase.direction);
            Waypoint exitWP = new Waypoint(egressPosition, MoveType.EXIT, straightDirPair);
            List<Waypoint> waypointList = new List<Waypoint>() { exitWP };
            DirectionPair dirPair = new DirectionPair(wormBase.direction, wormBase.direction);
            onFollowWaypoint(waypointList, dirPair);
        }

        /** 
         * Finish going straight to the exit point of a junction
         */
        private void completeStraight(Waypoint waypoint)
        {
            Tunnel.Junction junction = (Tunnel.Junction) (Tunnel.Map.getCurrentTunnel(clit.position)); // use the head, because ring will be on border of cell
            ReachJunctionExitEvent += junction.onReachJunctionExit;
            ReachJunctionExitEvent();
            ReachJunctionExitEvent -= junction.onReachJunctionExit;
            DirectionPair straightDirPair = new DirectionPair(wormBase.direction, wormBase.direction); // create a straight tunnel after navigating through a junction 
            RaiseChangeDirectionEvent(straightDirPair, wormId);
        }

        /**
         * Finish a turn and check if any turns are queued up 
         * 
         * @waypoint The waypoint that completes the turnonf
         */
        private void completeTurn(Waypoint waypoint)
        {
            if (waypointIndex < waypointList.Count - 1)
            {
                throw new System.Exception("completing turn should be the last action in waypoint list");
            }

            Tunnel.Tunnel tunnel = Tunnel.Map.getCurrentTunnel(clit.position);

            if (nextWaypointList.Count > 0) // additional turns
            {
                waypointList = new List<Waypoint>(nextWaypointList);
                clearWaypoints(nextWaypointList);
            }
            else // no immediate turn
            {
                clearWaypoints(waypointList);

                if (tunnel.type == Tunnel.Type.Name.STRAIGHT)
                {
                    throw new System.Exception("not a turning tunnel, wrong tunnel selected");
                }
                //wormBase.direction = egressWaypointDirection; // <-- redundant, we will do this when worm reaches CENTER waypoint
                ExitTurnEvent(egressWaypointDirection);
            }
            CompleteTurnEvent += ((Tunnel.TurnableTunnel)tunnel).onCompleteTurn;
            CompleteTurnEvent(wormId, egressWaypointDirection); // set the new direction as the exit direction
            CompleteTurnEvent -= ((Tunnel.TurnableTunnel)tunnel).onCompleteTurn;
        }

        /**
         * When rigidbody reaches waypoint go to next waypoint or complete waypoints
         */
        public void onReachWaypoint(Waypoint waypoint)
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
            else if (waypoint.move == MoveType.EXIT)
            {
                if (wormBase.isStraight)
                {
                    print("complete straight");
                    completeStraight(waypoint);
                }
                else
                {
                    completeTurn(waypoint); // complete the current turn and determine what next move is (navigating out of turn or turning again)
                }
            }
            //else if (waypoint.move == MoveType.OFFSET)
            //{
            //    clearWaypoints(waypointList);
            //}
            else
            {
                return;
            }

            waypointIndex = waypointList.FindIndex(wp => waypoint.position.Equals(wp.position)); // EXIT pos of prevCell equals the ENTER pos of current cell
            if (waypointIndex < waypointList.Count - 1) // if not the last waypoint in the list
            {
                waypointIndex += 1;
                Waypoint nextWaypoint = waypointList[waypointIndex];
                MoveToWaypointEvent(nextWaypoint);
            }
        }

        /**
         * Follow waypoints when navigating corners. If already following waypoints, queue up the next waypoints list
         * 
         * @waypointList a list of coordinates the worm follows to navigate a corner
         */
        public void onFollowWaypoint(List<Waypoint> waypointList, DirectionPair directionPair)
        {
            egressWaypointDirection = directionPair.curDir; // save the last egress directionPair
            Vector3 unitVector = Dir.CellDirection.getUnitVectorFromDirection(egressWaypointDirection);

            if (this.waypointList.Count > 0)
            {
                nextWaypointList = waypointList;
            }
            else
            {
                this.waypointList = waypointList;
            }
            Waypoint firstWaypoint = this.waypointList[0];
            MoveToWaypointEvent(firstWaypoint);
        }

        private new void OnDisable()
        {
            Tunnel.CollisionManager.Instance.InitWormPositionEvent -= onInitWormPosition;

 
            ForceEvent -= GetComponent<Force>().onForce;
            ExitTurnEvent -= GetComponent<Turn>().onExitTurn;
            MoveToWaypointEvent -= GetComponent<Turn>().onMoveToWaypoint;

            DecisionProcessingEvent -= GetComponent<InputProcessor>().onDecisionProcessing;

            if (GetComponent<TunnelMaker>()) // applies to AI
            {
                DecisionProcessingEvent -= GetComponent<TunnelMaker>().onDecisionProcessing;
            }
        }
    }
}
