using System.Collections.Generic;
using UnityEngine;

namespace Tunnel
{
    public class CollisionManager : GenericSingletonClass<CollisionManager>
    {
        public delegate void SliceTunnel(Straight collidedTunnel, Direction ingressDirection, Vector3 contactPosition);
        private event SliceTunnel SliceTunnelEvent; // fired when current tunnel intersects another tunnel

        public delegate void InitWormPosition(Vector3 position, Direction direction);
        public event InitWormPosition InitWormPositionEvent;

        public delegate void Stop(Straight tunnel);
        public event Stop StopEvent;

        public delegate void CreateJunctionOnCollision(Tunnel collisionTunnel, DirectionPair dirPair, CellMove cellMove, string wormId);
        public event CreateJunctionOnCollision CreateJunctionOnCollisionEvent;

        public delegate void CreateJunctionOnInit(DirectionPair dirPair, CellMove cellMove, Direction ingressDirection, List<Direction> allHoleDirections, string wormId);
        public event CreateJunctionOnInit CreateJunctionOnInitEvent;

        public delegate void CreateTunnel(CellMove cellMove, DirectionPair directionPair, Tunnel tunnel, string wormId);
        public event CreateTunnel CreateTunnelEvent;

        // Start is called before the first frame update
        protected void OnEnable()
        {
            CreateTunnelEvent += FindObjectOfType<NewTunnelFactory>().onCreateTunnel;

            SliceTunnelEvent += FindObjectOfType<Intersect.Slicer>().sliceTunnel;

            CreateJunctionOnCollisionEvent += FindObjectOfType<ModTunnelFactory>().onCreateJunctionOnCollision;

            CreateJunctionOnInitEvent += FindObjectOfType<ModTunnelFactory>().onCreateJunctionOnInit;

            StopEvent += TunnelManager.Instance.onStop;
        }

        /**
         * Event listener for the tunnel collision.
         * On intersect with a tunnel segment, create a junction and slice segment (if necessary)
         * 
         * @directionPair       the current and next direction of the player
         * @curTunnel           the tunnel the player is currently in   
         * @nextTunnel          the next tunnel the player is colliding with
         * @isCreatingTunnel    is the player creating a new tunnel or in existing one
         * @collisionCell       the cell the collision occured in
         * @isTunnelNew         if the tunnel is growing or if it already existed
         */

        // need to pass in the worm id if in existing tunnel
        public void onCollide(DirectionPair directionPair, Tunnel curTunnel, Tunnel nextTunnel, Vector3Int collisionCell, bool isTunnelNew)
        {
            if (!nextTunnel.isDirectionPairInHoleList(directionPair)) // the holes of the collided tunnel dont line up with the worm's path so we need to modify tunnel by creating a junction
            {
                CellMove cellMove;
                if (isTunnelNew) // if new tunnel then curTunnel's curCell is the leading cell that collided with another tunnel
                {
                    cellMove = CellMove.getCellMove(curTunnel, directionPair);
                }
                else // we dont know where in the curTunnel player is so use collisionCell
                {
                    cellMove = new CellMove(directionPair.prevDir, collisionCell);
                }

                if (Type.isTypeStraight(nextTunnel.type))
                {
                    Vector3 contactPosition = curTunnel.getContactPosition(directionPair);
                    //if (!cellMove.startPosition.Equals(contactPosition))
                    //{
                    //    throw new System.Exception("vectors are not equivalent");
                    //}
                    Direction ingressDirection = directionPair.prevDir;
                    SliceTunnelEvent((Straight)nextTunnel, ingressDirection, contactPosition);
                }
                else
                {
                    Destroy(nextTunnel.gameObject);
                }

                if (isTunnelNew && StopEvent != null && Type.isTypeStraight(curTunnel.type)) // it may be the case where StopEvent is already unsubscribed because tunnel has already been stopped. For example onChangeDirection
                {
                    Straight straightTunnel = (Straight)curTunnel;
                    if (!straightTunnel.isStopped)
                    {
                        StopEvent((Straight)curTunnel);
                    }
                }

                CreateJunctionOnCollisionEvent(nextTunnel, directionPair, cellMove, curTunnel.wormCreatorId);
            }
        }

        /**
         * The first decision made will initialize a tunnel of type 6-way junction (??)
         */
        public void onInitDecision(Direction direction, string wormId, Vector3Int initialCell)
        {
            print("init decision event in direction " + direction);
            CellMove cellMove = CellMove.getInitialCellMove(direction, initialCell);
            InitWormPositionEvent(cellMove.startPosition, direction);

            DirectionPair sameDirPair = new DirectionPair(direction, direction);
            CreateTunnelEvent(cellMove, sameDirPair, null, wormId);
        }

        /**
         * Tunnel direction change triggers creation or modification of the next tunnel. 
         * Note: direction may not change at all (eg straight tunnel created after a corner)
         * 
         * @directionPair indicates direction of travel and determines type of tunnel to create
         */
        public void onChangeDirection(DirectionPair directionPair, Tunnel prevTunnel, string wormId, CellMove cellMove, bool isCreatingTunnel)
        {
            Tunnel existingTunnel = Map.getTunnelFromDict(cellMove.cell);

            if (existingTunnel == null) // if tunnel does not exist at cell then no longer in existing tunnel
            {
                CreateTunnelEvent(cellMove, directionPair, prevTunnel, wormId);
            }
            if (isCreatingTunnel)
            {
                if (StopEvent != null && prevTunnel.type == Type.Name.STRAIGHT)
                {
                    StopEvent((Straight)prevTunnel); // Stop the last growing tunnel
                }
                if (existingTunnel != null)
                {
                    onCollide(directionPair, prevTunnel, existingTunnel, Vector3Int.zero, isCreatingTunnel);
                }
            }
            else if (existingTunnel != null) 
            {
                prevTunnel.setWormCreatorId(wormId);
                onCollide(directionPair, prevTunnel, existingTunnel, cellMove.cell, isCreatingTunnel);
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
                CreateJunctionOnCollisionEvent -= FindObjectOfType<ModTunnelFactory>().onCreateJunctionOnCollision;
                CreateJunctionOnCollisionEvent -= FindObjectOfType<ModTunnelFactory>().onCreateJunctionOnCollision;
            }
            if (FindObjectOfType<Factory>())
            {
                CreateTunnelEvent -= FindObjectOfType<NewTunnelFactory>().onCreateTunnel;
            }
            if (FindObjectOfType<TunnelManager>())
            {
                StopEvent -= TunnelManager.Instance.onStop;
            }
        }
    }

}