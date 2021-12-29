using System.Collections.Generic;
using UnityEngine;

namespace Tunnel
{

    public class Manager : CollisionManager
    {
        private List<GameObject> TunnelList; // list consisting of straight tunnels and corner tunnels

        public delegate void Grow();
        public event Grow GrowEvent;

        public new delegate void Stop();
        public new event Stop StopEvent;

        public static Vector3Int initialCell = Vector3Int.zero; // initial cell

        private void Awake()
        {
            TunnelList = new List<GameObject>();
        }

        protected new void OnEnable()
        {
            base.OnEnable();
            CreateTunnelEvent += FindObjectOfType<NewTunnelFactory>().onCreateTunnel;
            InitWormPositionEvent += FindObjectOfType<Worm.Movement>().onInitWormPosition;
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
    }
}