using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Tunnel
{
    public class Type : MonoBehaviour
    {
        [SerializeField]
        public Transform Straight;

        [SerializeField]
        public Transform Corner;

        [SerializeField]
        public Transform ThreewayTunnelOpp;

        [SerializeField]
        public Transform ThreewayTunnelAdj;

        [SerializeField]
        public Transform FourwayTunnelOpp;

        [SerializeField]
        public Transform FourwayTunnelAdj;

        [SerializeField]
        public Transform FivewayTunnel;

        [SerializeField]
        public Transform SixwayTunnel;

        [SerializeField]
        public Transform TunnelNetwork;

        public static Type instance;

        public const string STRAIGHT = "Straight";
        public const string CORNER = "Corner";
        public const string JUNCTION = "Junction";

        public enum Name {
            STRAIGHT,
            CORNER,
            THREEWAY_OPP,
            THREEWAY_ADJ,
            FOURWAY_ADJ,
            FOURWAY_OPP,
            FIVEWAY,
            SIXWAY
        }

        private void Awake()
        {
            instance = this;
        }
    }
}