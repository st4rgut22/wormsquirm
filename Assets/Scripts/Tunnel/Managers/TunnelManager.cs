using System.Collections.Generic;
using UnityEngine;

namespace Tunnel
{
    public class TunnelManager : GenericSingletonClass<TunnelManager>
    {
        private List<Tunnel> TunnelList; // list consisting of straight tunnels and corner tunnels

        public Vector3Int initialCell = Vector3Int.zero; // initial cell

        [SerializeField]
        public const float HEAD_WORM_OFFSET = 0.5f; // Distance between head of tunnel and worm. Must be beteween 0 and 1s

        public Vector3Int startingCell;

        private new void Awake()
        {
            base.Awake();
            TunnelList = new List<Tunnel>();
        }

        public void onAddTunnel(Tunnel tunnel, Vector3Int cell, DirectionPair directionPair)
        {
            TunnelList.Add(tunnel);
        }

        public float getHeadOffset()
        {
            if (HEAD_WORM_OFFSET >= 1 || HEAD_WORM_OFFSET <= 0)
            {
                throw new System.Exception("Head offset must be between 0 and 1");
            }
            else
            {
                return 1 - HEAD_WORM_OFFSET;
            }
        }

        public Tunnel getLastTunnel()
        {
            if (TunnelList.Count == 0)
            {
                return null;
            }
            else
            {
                return TunnelList[TunnelList.Count - 1];
            }
        }

        /**
         * Get the prefab with the correct orientation
         */
        public Transform GetPrefabFromHoleList(Direction ingressDir, List<Direction> holeDirList, Transform rotationParent)
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
    }
}