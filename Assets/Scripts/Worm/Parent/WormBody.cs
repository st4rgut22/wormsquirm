using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Worm
{
    public class WormBody : MonoBehaviour
    {
        public delegate void SaveWorm(string wormId, GameObject wormGO);
        public event SaveWorm SaveWormEvent;

        [SerializeField]
        protected Rigidbody ring;

        [SerializeField]
        protected Rigidbody head;

        [SerializeField]
        protected Rigidbody clit;

        public string wormId;

        protected WormBase wormBase; // stores shared variables across worm prefab classes        

        public static float WORM_BODY_THICKNESS = 0.1f;

        public float turnSpeed;

        public delegate void ChangeDirection(DirectionPair directionPair, Tunnel.Tunnel tunnel, string wormId, Tunnel.CellMove cellMove, bool isCreatingTunnel);
        public event ChangeDirection ChangeDirectionEvent;

        public delegate void RemoveSelf(string wormId, GameObject wormGO);
        public event RemoveSelf RemoveSelfEvent;

        public delegate void ObjectiveReached();
        public event ObjectiveReached ObjectiveReachedEvent;

        public delegate void Spawn(string wormId);
        public event Spawn SpawnEvent;

        protected void Awake()
        {
            wormBase = GetComponent<WormBase>();
        }

        /**
         * Add a worm to the worm manager immediately after creating the worm
         */
        public void RaiseSaveWormEvent()
        {
            SaveWormEvent += WormManager.Instance.onSave;
            SaveWormEvent(wormId, gameObject);
            SaveWormEvent -= WormManager.Instance.onSave;
        }

        /**
         * Worm has reached a goal indicating the game is over
         */
        protected void RaiseObjectiveReachedEvent()
        {
            ObjectiveReachedEvent += GameManager.Instance.onObjectiveReached;
            ObjectiveReachedEvent();
            ObjectiveReachedEvent -= GameManager.Instance.onObjectiveReached;
        }

        /**
         * Tells the worm to destroy itself
         */
        protected void RaiseRemoveSelfEvent()
        {
            RemoveSelfEvent += WormManager.Instance.onRemoveWorm;
            RemoveSelfEvent(wormId, gameObject);
            RemoveSelfEvent -= WormManager.Instance.onRemoveWorm;
        }

        /**
         * When a worm is destroy it, issue a respawn event
         */
        protected void RaiseSpawnEvent()
        {
            SpawnEvent += FindObjectOfType<Factory.WormFactory>().onCreateWorm;
            SpawnEvent(gameObject.tag);
            SpawnEvent -= FindObjectOfType<Factory.WormFactory>().onCreateWorm;
        }

        /**
         * Emit change direction event
         * 
         * @directionPair       the current and next direction of the player
         * @tunnel              the current tunnel player is in
         * @wormId              the id of the player
         */
        protected void RaiseChangeDirectionEvent(DirectionPair directionPair, Tunnel.Tunnel tunnel, string wormId)
        {
            if (directionPair.isStraight())
            {
                wormBase.setChangingDirection(false);
            }
            else
            {
                wormBase.setChangingDirection(true);
            }

            Tunnel.CellMove cellMove;
            if (wormBase.isCreatingTunnel) // if new tunnel use the tunnel's leading cell to get the Cell info
            {
                cellMove = Tunnel.CellMove.getCellMove(tunnel, directionPair); // get cell from map, check if cell in the egress direction is already occupied
            }
            else // if existing tunnel use the worm position to get the Cell info
            {
                if (wormBase.isChangingDirection) // if making a consecutive turn, ring position will be one cell behind the turn we want turn to happen in so use clit position
                {
                    Vector3Int curCell = Tunnel.Map.getCellPos(clit.position);
                    Vector3Int nextCurCell = Dir.Vector.getNextCellFromDirection(curCell, directionPair.prevDir);
                    cellMove = new Tunnel.CellMove(directionPair.curDir, nextCurCell); 
                }
                else
                {
                    Vector3Int curCell = Tunnel.Map.getCellPos(ring.position);
                    cellMove = new Tunnel.CellMove(wormBase.direction, curCell); // if not making a consecutive turn, ring position will be in the same cell as the turn
                }
            }
            ChangeDirectionEvent += Tunnel.CollisionManager.Instance.onChangeDirection;
            ChangeDirectionEvent(directionPair, tunnel, wormId, cellMove, wormBase.isCreatingTunnel);
            ChangeDirectionEvent -= Tunnel.CollisionManager.Instance.onChangeDirection;
        }
    }
}
