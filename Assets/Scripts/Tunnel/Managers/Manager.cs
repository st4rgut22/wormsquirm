using System.Collections.Generic;
using UnityEngine;

namespace Tunnel
{

    public class Manager : CollisionManager
    {
        private List<GameObject> TunnelList; // list consisting of straight tunnels and corner tunnels

        public delegate void CreateTunnel(CellMove cellMove, DirectionPair directionPair);
        public event CreateTunnel CreateTunnelEvent;

        public delegate void InitWormPosition(Vector3 position);
        public event InitWormPosition InitWormPositionEvent;

        public delegate void Decision(bool isStraightTunnel, Direction direction, Tunnel tunnel);
        public event Decision DecisionEvent;

        public delegate void Grow();
        public event Grow GrowEvent;

        public new delegate void Stop();
        public new event Stop StopEvent;

        public static Vector3Int initialCell = Vector3Int.zero; // initial cell

        private bool isInit = true;

        private void Awake()
        {
            TunnelList = new List<GameObject>();
        }

        protected new void OnEnable()
        {
            base.OnEnable();
            DecisionEvent += FindObjectOfType<Turn>().onDecision;
            DecisionEvent += FindObjectOfType<Map>().onDecision;
            DecisionEvent += FindObjectOfType<Worm.Movement>().onDecision;
            CreateTunnelEvent += FindObjectOfType<NewTunnelFactory>().onCreateTunnel;
            InitWormPositionEvent += FindObjectOfType<Worm.Movement>().onInitWormPosition;
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

        public void onAddTunnel(Tunnel tunnel, Vector3Int cell, DirectionPair directionPair)
        {
            TunnelList.Add(tunnel.gameObject);
            tunnel.cellPositionList.Add(cell); // initialize the coordinate list of the new tunnel
        }

        /**
         * Tunnel direction change triggers creation or modification of the next tunnel. 
         * 
         * @directionPair indicates direction of travel and determines type of tunnel to create
         */
        public void onChangeDirection(DirectionPair directionPair)
        {
            if (StopEvent != null)
            {
                StopEvent(); // Stop the last growing tunnel
            }

            // get cell from map, check if tunnel w/ egress at curDirection already exists
            Tunnel tunnel = getLastTunnel(TunnelList);
            CellMove cellMove = CellMove.getCellMove(tunnel, directionPair);
            if (isInit)
            {
                InitWormPositionEvent(cellMove.startPosition);
                isInit = false;
            }

            Tunnel existingTunnel = Map.getTunnelFromDict(cellMove.cell);

            if (existingTunnel == null)
            {
                CreateTunnelEvent(cellMove, directionPair);
            }
            else // tunnel exists where we want to create a corner. issue slice event
            {
                collide(directionPair, tunnel, existingTunnel);
            }
        }

        /**
         * Get the prefab with the correct orientation
         */
        public static Transform GetPrefabFromHoleList(Direction ingressDir, List<Direction> holeDirList, Transform rotationParent)
        {

            foreach (Transform prefabOrientation in rotationParent)
            {
                Rotation.Rotation rotation = prefabOrientation.gameObject.GetComponent<Rotation.Rotation>();
                if (rotation.isRotationInRotationDict(ingressDir, holeDirList))
                {
                    return prefabOrientation;
                }
            }
            throw new System.Exception("no prefab exists with ingressdir " + ingressDir + " hole list " + holeDirList.ToString());
        }

        private void OnDisable()
        {
            if (FindObjectOfType<Turn>())
            {
                DecisionEvent -= FindObjectOfType<Turn>().onDecision;
            }
            if (FindObjectOfType<Worm.Movement>())
            {
                DecisionEvent -= FindObjectOfType<Worm.Movement>().onDecision;
            }
            if (FindObjectOfType<Map>())
            {
                DecisionEvent -= FindObjectOfType<Map>().onDecision;
            }
            if (FindObjectOfType<Factory>())
            {
                CreateTunnelEvent -= FindObjectOfType<NewTunnelFactory>().onCreateTunnel;
            }
            if (FindObjectOfType<Worm.Movement>())
            {
                InitWormPositionEvent -= FindObjectOfType<Worm.Movement>().onInitWormPosition;
            }
        }
    }
}