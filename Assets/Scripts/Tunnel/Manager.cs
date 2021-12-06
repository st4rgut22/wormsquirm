using System.Collections.Generic;
using UnityEngine;

namespace Tunnel
{

    public class Manager : MonoBehaviour
    {
        public static int straightCount;
        public static int cornerCount;

        private List<GameObject> TunnelList; // list consisting of straight tunnels and corner tunnels

        //public delegate void Slice(Tunnel curTunnel, Tunnel nextTunnel, Direction ingressDirection);
        //private event Slice SliceEvent;

        public delegate void InitWormPosition(Vector3 position);
        public event InitWormPosition InitWormPositionEvent;

        public delegate void Decision(bool isStraightTunnel, Direction direction, Tunnel tunnel);
        public event Decision DecisionEvent;

        public delegate void AddTunnel(Tunnel tunnel, Vector3Int cell, DirectionPair directionPair);
        public event AddTunnel AddTunnelEvent;

        public delegate void Grow();
        public event Grow GrowEvent;

        public delegate void Stop();
        public event Stop StopEvent;

        private void Awake()
        {
            TunnelList = new List<GameObject>();
            straightCount = cornerCount = 0;
        }

        protected void OnEnable()
        {
            AddTunnelEvent += FindObjectOfType<Map.Manager>().onAddTunnel;
            AddTunnelEvent += FindObjectOfType<Worm.Movement>().onAddTunnel;
            InitWormPositionEvent += FindObjectOfType<Worm.Movement>().onInitWormPosition;
            DecisionEvent += FindObjectOfType<Turn>().onDecision;
            DecisionEvent += FindObjectOfType<Worm.Movement>().onDecision;
            //SliceEvent += FindObjectOfType<Intersect.Manager>().onSlice;
        }

        private void FixedUpdate()
        {
            // the worm & user input should be dictating growth not tunnel manager

            if (GrowEvent != null)
            {
                GrowEvent();
            }
        }

        private Tunnel getLastTunnel(List<GameObject> tunnelList)
        {
            if (tunnelList.Count > 0)
            {
                return tunnelList[tunnelList.Count - 1].GetComponent<Tunnel>();
            }
            else
            {
                return null;
            }
        }

        /**
         * Make a decision given a position
         */
        public void onPosition(Vector3 position, Direction direction)
        {
            Tunnel LastTunnel = getLastTunnel(TunnelList);

            bool isDecision = ActionPoint.instance.isDecisionBoundaryCrossed(LastTunnel, position, direction);

            if (isDecision)
            {
                bool isStraightTunnel = LastTunnel == null || LastTunnel.type == Type.Name.STRAIGHT; // if this is the first tunnel it should be straight type
                DecisionEvent(isStraightTunnel, direction, LastTunnel);
            }
        }

        /**
         * Tunnel direction change triggers creation or modification of the next tunnel. 
         * 
         * @directionPair indicates direction of travel and determines type of tunnel to create
         */
        public void onChangeDirection(DirectionPair directionPair)
        {
            Direction prevDirection = directionPair.prevDir;
            Direction curDirection = directionPair.curDir;

            if (StopEvent != null)
            {
                StopEvent(); // Stop the last growing tunnel
            }

            // get cell from map, check if tunnel w/ egress at curDirection already exists
            Tunnel tunnel = getLastTunnel(TunnelList);
            print("prev dir is " + prevDirection + " cur dir is " + curDirection);

            CellMove cellMove;
            if (tunnel != null)
            {
                cellMove = new CellMove(tunnel, prevDirection); // previous direction is direction of movement in the current tunnel
            }
            else
            {
                cellMove = new CellMove(curDirection); // on game start, there is no previous direction so use current direction
                InitWormPositionEvent(cellMove.startPosition);
                // send addTunnelEvent for cellMove.nextCell, but we need a reference to nextTunnel to do this
            }

            Tunnel existingTunnel = Map.Manager.getTunnelFromDict(cellMove.cell);
            if (existingTunnel == null)
            {
                Tunnel nextTunnel = Factory.newTunnelFactory.createTunnel(directionPair, gameObject, cellMove);
                print("adding cell " + cellMove.cell);
                AddTunnelEvent(nextTunnel, cellMove.cell, directionPair);
                if (cellMove.isInit)
                {
                    AddTunnelEvent(nextTunnel, cellMove.nextCell, directionPair);
                }
                TunnelList.Add(nextTunnel.gameObject);
                nextTunnel.cellPositionList.Add(cellMove.cell); // initialize the coordinate list of the new tunnel
            }
            //else // tunnel exists where we want to create one. issue slice event
            //{
            //    SliceEvent(tunnel, existingTunnel, );
            //}
        }

        private void OnDisable()
        {
            if (FindObjectOfType<Turn>())
            {
                DecisionEvent -= FindObjectOfType<Turn>().onDecision;
            }
            if (FindObjectOfType<Map.Manager>())
            {
                AddTunnelEvent -= FindObjectOfType<Map.Manager>().onAddTunnel;
            }
            if (FindObjectOfType<Worm.Movement>())
            {
                AddTunnelEvent -= FindObjectOfType<Worm.Movement>().onAddTunnel;
                DecisionEvent -= FindObjectOfType<Worm.Movement>().onDecision;
                InitWormPositionEvent -= FindObjectOfType<Worm.Movement>().onInitWormPosition;
            }
            //if (FindObjectOfType<Intersect.Manager>())
            //{
            //    SliceEvent -= FindObjectOfType<Intersect.Manager>().onSlice;
            //}
        }
    }
}