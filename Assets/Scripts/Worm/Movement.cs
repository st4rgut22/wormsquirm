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
        private Direction direction; // direction of worm travel

        private int waypointIndex;

        private List<Waypoint> waypointList;
        private List<Waypoint> nextWaypointList; // queued up waypoint list if turning while navigating a corner

        public delegate void CompleteTurn(Direction direction); // when turn is completed notify Turn so we can proceed straight
        public event CompleteTurn CompleteTurnEvent;

        public delegate void MoveToWaypoint(Waypoint waypoint); // isTurn flag allows us to override straight move
        public event MoveToWaypoint MoveToWaypointEvent;

        public delegate void BlockInterval(bool isBlockInterval, Vector3Int blockPositionInt, Tunnel.Straight tunnel);
        public event BlockInterval BlockIntervalEvent;

        public delegate void DecisionProcessing(bool isDecisionProcessing);
        public event DecisionProcessing DecisionProcessingEvent;

        public delegate void AddForce(Rigidbody rigidbody, Vector3 forceVector);
        public event AddForce ForceEvent;

        private new void Awake()
        {
            waypointList = new List<Waypoint>();
            nextWaypointList = new List<Waypoint>();
            transform.position = Tunnel.TunnelManager.Instance.initialCell;
            direction = Direction.None;
        }

        private void OnEnable() 
        {
            ForceEvent += GetComponent<Force>().onForce;
            CompleteTurnEvent += GetComponent<Turn>().onCompleteTurn;
            MoveToWaypointEvent += GetComponent<Turn>().onMoveToWaypoint;
            DecisionProcessingEvent += GetComponent<InputProcessor>().onDecisionProcessing;

            CompleteTurnEvent += FindObjectOfType<Controller>().onCompleteTurn;

            Tunnel.CollisionManager.Instance.InitWormPositionEvent += onInitWormPosition;
        }

        private void FixedUpdate()
        {
            if (direction != Direction.None)
            {
                Vector3 forceDir = -head.transform.up; // negative because worm's forward vector is opposite world space forward vector
                Debug.Log("go in the direction" + forceDir);
                ForceEvent(head, forceDir); // make the worm go striaght ahead
            }
        }

        public void onInitWormPosition(Vector3 initPos, Direction direction)
        {
            this.direction = direction;
            float offset = Tunnel.TunnelManager.Instance.START_TUNNEL_RING_OFFSET;
            Vector3 offsetVector = Dir.Vector.getUnitVectorFromDirection(direction);
            ring.position = initPos + offset * offsetVector;
        }

        private void clearWaypoints(List<Waypoint> waypoints)
        {
            waypoints.Clear();
        }

        public void setCompleteTurnDelegate(Tunnel.Corner corner)
        {
            CompleteTurnEvent += corner.onCompleteTurn;
        }

        /**
         * Finish a turn and check if any turns are queued up 
         * 
         * @waypoint The waypoint that completes the turn
         */
        private void completeTurn(Waypoint waypoint)
        {
            if (waypointIndex < waypointList.Count - 1)
            {
                throw new System.Exception("completing turn should be the last action in waypoint list");
            }

            if (nextWaypointList.Count > 0) // additional turns
            {
                waypointList = new List<Waypoint>(nextWaypointList);
                clearWaypoints(nextWaypointList);
            }
            else
            {
                clearWaypoints(waypointList);

                Tunnel.Tunnel tunnel = GetComponent<WormTunnelBroker>().getCurTunnel(direction);
                if (tunnel.type != Tunnel.Type.Name.CORNER)
                {
                    throw new System.Exception("not a corner tunnel, wrong tunnel selected");
                }
                direction = egressWaypointDirection; // set the new direction as the exit direction
                CompleteTurnEvent(egressWaypointDirection); // goes straight

                CompleteTurnEvent -= ((Tunnel.Corner)tunnel).onCompleteTurn;
            }
        }

        /**
         * When rigidbody reaches waypoint go to next waypoint or complete waypoints
         */
        public void onReachWaypoint(Waypoint waypoint)
        {
            if (waypoint.move == MoveType.ENTRANCE)
            {
                DecisionProcessingEvent(true); // no turns allowed when entering a turn (from current position to center of the turn tunnel segment)
            }
            else if (waypoint.move == MoveType.CENTER)
            {
                print("apply up force to the ring rgbdy");
                DecisionProcessingEvent(false); // allow decisions to be made again when center of tunnel is reached (eg a consecutive turn)
            }
            else if (waypoint.move == MoveType.EXIT)
            {
                completeTurn(waypoint); // complete the current turn and determine what next move is (navigating out of turn or turning again)
            }
            else if (waypoint.move == MoveType.OFFSET)
            {
                clearWaypoints(waypointList);
            }
            else
            {
                return;
            }

            waypointIndex = waypointList.FindIndex(wp => waypoint.position.Equals(wp.position));
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
            Vector3 unitVector = Dir.Vector.getUnitVectorFromDirection(egressWaypointDirection);

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

        private void OnDisable()
        {
            if (GetComponent<Turn>())
            {
                ForceEvent -= GetComponent<Force>().onForce;
                CompleteTurnEvent -= GetComponent<Turn>().onCompleteTurn;
                MoveToWaypointEvent -= GetComponent<Turn>().onMoveToWaypoint;
            }
            if (FindObjectOfType<Controller>())
            {
                CompleteTurnEvent -= FindObjectOfType<Controller>().onCompleteTurn;
            }
            if (FindObjectOfType<InputProcessor>())
            {
                DecisionProcessingEvent -= FindObjectOfType<InputProcessor>().onDecisionProcessing;
            }
            if (Tunnel.CollisionManager.Instance != null)
            {
                Tunnel.CollisionManager.Instance.InitWormPositionEvent -= onInitWormPosition;
            }
        }
    }
}
