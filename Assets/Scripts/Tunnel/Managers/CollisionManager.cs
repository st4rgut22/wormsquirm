using UnityEngine;

namespace Tunnel
{
    public class CollisionManager : GenericSingletonClass<CollisionManager>
    {
        public delegate void SliceTunnel(Straight collidedTunnel, Direction ingressDirection, Vector3 contactPosition);
        private event SliceTunnel SliceTunnelEvent; // fired when current tunnel intersects another tunnel

        public delegate void InitWormPosition(Vector3 position, Direction direction);
        public event InitWormPosition InitWormPositionEvent;

        public delegate void Stop();
        public event Stop StopEvent;

        public delegate void CreateJunction(Tunnel collisionTunnel, DirectionPair dirPair, CellMove cellMove);
        private event CreateJunction CreateJunctionEvent;

        public delegate void CreateTunnel(CellMove cellMove, DirectionPair directionPair);
        public event CreateTunnel CreateTunnelEvent;

        public bool isCreatingNewTunnel = false;

        // Start is called before the first frame update
        protected void OnEnable()
        {
            CreateTunnelEvent += FindObjectOfType<NewTunnelFactory>().onCreateTunnel;
            CreateTunnelEvent += FindObjectOfType<Worm.InputProcessor>().onCreateTunnel;
            SliceTunnelEvent += FindObjectOfType<Intersect.Slicer>().sliceTunnel;

            CreateJunctionEvent += FindObjectOfType<ModTunnelFactory>().onCreateJunction;
            CreateJunctionEvent += FindObjectOfType<Worm.Movement>().onCreateJunction;
            CreateJunctionEvent += FindObjectOfType<Worm.InputProcessor>().onCreateJunction;

            InitWormPositionEvent += FindObjectOfType<Worm.Movement>().onInitWormPosition;
        }

        /**
         * Event listener for the tunnel collision. Tunnel.Tunnel name is name of GO that caused collision
         * 
         * On intersect with a tunnel segment, create a junction and slice segment (if necessary)
         * Notify worm about new tunnel so it can keep track of blockInterval instead of tunnel
         */
        public void onCollide(DirectionPair directionPair, Tunnel curTunnel, Tunnel nextTunnel)
        {
            Direction exitDirection = directionPair.curDir;

            Vector3 contactPosition = curTunnel.getContactPosition(directionPair);
            CellMove cellMove = CellMove.getCellMove(curTunnel, directionPair);
            if (!cellMove.startPosition.Equals(contactPosition))
            {
                throw new System.Exception("vectors are not equivalent");
            }

            if (nextTunnel.type == Type.Name.STRAIGHT)
            {
                SliceTunnelEvent((Straight)nextTunnel, exitDirection, contactPosition);
            }
            else
            {
                Destroy(nextTunnel.gameObject);
            }
            CreateJunctionEvent(nextTunnel, directionPair, cellMove);
            StopEvent();
        }

        /**
         * The first decision made will initialize a tunnel by emitting a Change Direction event
         */
        public void onInitDecision(Direction direction)
        {
            CellMove cellMove = CellMove.getInitialCellMove(direction);
            InitWormPositionEvent(cellMove.startPosition, direction);

            DirectionPair sameDirPair = new DirectionPair(direction, direction);
            CreateTunnelEvent(cellMove, sameDirPair);
        }

        /**
         * Tunnel direction change triggers creation or modification of the next tunnel. 
         * 
         * @directionPair indicates direction of travel and determines type of tunnel to create
         */
        public void onChangeDirection(DirectionPair directionPair)
        {
            Tunnel tunnel = TunnelManager.Instance.getLastTunnel();
            // get cell from map, check if tunnel w/ egress at curDirection already exists
            CellMove cellMove = CellMove.getCellMove(tunnel, directionPair);
            if (StopEvent != null)
            {
                StopEvent(); // Stop the last growing tunnel
            }
            Tunnel existingTunnel = Map.getTunnelFromDict(cellMove.cell);

            if (existingTunnel == null)
            {
                CreateTunnelEvent(cellMove, directionPair);
            }
            else // tunnel exists where we want to create a corner. 
            {
                onCollide(directionPair, tunnel, existingTunnel);
            }
        }

        private void OnDisable()
        {
            if (FindObjectOfType<Intersect.Slicer>())
            {
                SliceTunnelEvent -= FindObjectOfType<Intersect.Slicer>().sliceTunnel;
            }
            if (FindObjectOfType<ModTunnelFactory>())
            {
                CreateJunctionEvent -= FindObjectOfType<ModTunnelFactory>().onCreateJunction;
            }
            if (FindObjectOfType<Worm.Movement>())
            {
                CreateJunctionEvent -= FindObjectOfType<Worm.Movement>().onCreateJunction;
                InitWormPositionEvent -= FindObjectOfType<Worm.Movement>().onInitWormPosition;
            }
            if (FindObjectOfType<Worm.InputProcessor>())
            {
                CreateJunctionEvent += FindObjectOfType<Worm.InputProcessor>().onCreateJunction;
                CreateTunnelEvent += FindObjectOfType<Worm.InputProcessor>().onCreateTunnel;
            }
            if (FindObjectOfType<Factory>())
            {
                CreateTunnelEvent -= FindObjectOfType<NewTunnelFactory>().onCreateTunnel;
            }
        }
    }

}