using System.Collections.Generic;
using UnityEngine;

namespace Tunnel
{

    public class Manager : MonoBehaviour
    {
        private static int cornerCount;

        [SerializeField]
        private bool bypassWormInput;

        private List<GameObject> TunnelList; // list consisting of straight tunnels and corner tunnels
        private List<GameObject> StraightTunnelList;

        public delegate void Decision(bool isStraightTunnel, Direction direction, Tunnel tunnel);
        public event Decision DecisionEvent;

        public delegate void AddTunnel(Tunnel tunnel, Vector3Int cell, DirectionPair directionPair);
        public event AddTunnel AddTunnelEvent;

        public delegate void Grow();
        public event Grow GrowEvent;

        public delegate void Stop();
        public event Stop StopEvent;

        public delegate void SliceTunnel(Tunnel tunnel, Tunnel nextTunnel, DirectionPair dirPair);
        public event SliceTunnel SliceTunnelEvent;

        private void Awake()
        {
            cornerCount = 0;
            TunnelList = new List<GameObject>();
            StraightTunnelList = new List<GameObject>();
            bypassWormInput = true;
        }

        protected void OnEnable()
        {
            SliceTunnelEvent += FindObjectOfType<Slicer>().onSlice; // slicer is listening for collide events
            AddTunnelEvent += FindObjectOfType<Map.Manager>().onAddTunnel;
            AddTunnelEvent += FindObjectOfType<Worm.Movement>().onAddTunnel;
            DecisionEvent += FindObjectOfType<Turn>().onDecision;
            DecisionEvent += FindObjectOfType<Worm.Movement>().onDecision;
        }

        private void FixedUpdate()
        {
            // the worm & user input should be dictating growth not tunnel manager

            if (GrowEvent != null)
            {
                GrowEvent();
            }
        }

        /**
         * When the destination cell is already occupied, it needs to be replaced or spliced
         */
        private Tunnel replaceTunnel(Tunnel nextTunnel, Tunnel tunnel, DirectionPair directionPair, CellMove cellMove)
        {
            Tunnel replacementTunnel = Factory.modTunnelFactory.getTunnel(directionPair, tunnel, gameObject, cellMove);

            if (nextTunnel.tag == Env.STRAIGHT_TUNNEL)
            {
                SliceTunnelEvent(tunnel, replacementTunnel, directionPair); // the replacementTunnel will be spliced into the straight tunnel
            }
            else
            {
                Destroy(nextTunnel.gameObject); // destroy the tunnel that's being replaced
            }
            return replacementTunnel;
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
                bool isStraightTunnel = LastTunnel == null || LastTunnel.tag == Env.STRAIGHT_TUNNEL; // if this is the first tunnel it should be straight type
                DecisionEvent(isStraightTunnel, direction, LastTunnel);

            }
        }

        public void onSlice(Tunnel tunnelGO, Collision collision)
        {
            if (StopEvent != null && collision.gameObject.name == "Tunnel 0" && tunnelGO.name == "Tunnel 3")
            {
                print("stop tunnel 0");
                StopEvent(); // Stop the last growing tunnel if a collision occurred
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

            CellMove cellMove = new CellMove(tunnel, prevDirection); // previous direction is direction of movement in the current tunnel

            Tunnel nextTunnel;

            if (Map.Manager.containsCell(cellMove.nextCell))
            {
                nextTunnel = Map.Manager.TunnelMapDict[cellMove.nextCell];
                nextTunnel = replaceTunnel(nextTunnel, tunnel, directionPair, cellMove);
            }
            else
            {
                nextTunnel = Factory.newTunnelFactory.getTunnel(directionPair, gameObject, cellMove);
            }

            AddTunnelEvent(nextTunnel, cellMove.nextCell, directionPair);

            TunnelList.Add(nextTunnel.gameObject);
            nextTunnel.cellPositionList.Add(cellMove.nextCell); // initialize the coordinate list of the new tunnel
        }

        private void OnDisable()
        {
            if (FindObjectOfType<Turn>())
            {
                DecisionEvent -= FindObjectOfType<Turn>().onDecision;
            }
            if (FindObjectOfType<Slicer>())
            {
                SliceTunnelEvent -= FindObjectOfType<Slicer>().onSlice;
            }
            if (FindObjectOfType<Map.Manager>())
            {
                AddTunnelEvent -= FindObjectOfType<Map.Manager>().onAddTunnel;
            }
            if (FindObjectOfType<Worm.Movement>())
            {
                AddTunnelEvent -= FindObjectOfType<Worm.Movement>().onAddTunnel;
                DecisionEvent -= FindObjectOfType<Worm.Movement>().onDecision;
            }
        }
    }
}