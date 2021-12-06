using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Tunnel
{
    public class Type : MonoBehaviour
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

        public static Type instance;

        public const string STRAIGHT = "straight";
        public const string CORNER = "corner";

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