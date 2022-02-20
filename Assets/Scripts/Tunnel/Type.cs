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

        public static bool isTypeCorner(Name name)
        {
            return name == Name.CORNER;
        }

        public static bool isTypeStraight(Name name)
        {
            return name == Name.STRAIGHT;
        }

        /**
         * Junction consists of multiple names
         */
        public static bool isTypeJunction(Name name)
        {
            return name == Name.THREEWAY_ADJ || name == Name.THREEWAY_OPP || name == Name.FOURWAY_ADJ || name == Name.FOURWAY_OPP || name == Name.FIVEWAY ||
                name == Name.SIXWAY;
        }

        private void Awake()
        {
            instance = this;
        }
    }
}