using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Tunnel
{
    public class Env : MonoBehaviour
    {
        [SerializeField]
        public GameObject Straight;

        [SerializeField]
        public GameObject Corner;

        [SerializeField]
        public GameObject ThreewayTunnel;

        [SerializeField]
        public GameObject FourwayTunnel;

        [SerializeField]
        public GameObject FivewayTunnel;

        [SerializeField]
        public GameObject SixwayTunnel;

        [SerializeField]
        public Transform TunnelNetwork;

        public const string STRAIGHT_TUNNEL = "Straight";

        public static Env instance;

        private void Awake()
        {
            instance = this;
        }
    }
}