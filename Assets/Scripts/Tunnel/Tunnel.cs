using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Tunnel
{
    public abstract class Tunnel : MonoBehaviour
    {
        public const int BLOCK_SIZE = 2;
        public const float OFFSET_SIZE = 0.5f;

        public Vector3 ingressPosition { get; protected set; }
        public Vector3 egressPosition { get; protected set; }

        public Direction ingressDirection { get; protected set; }
        public Direction egressDirection { get; protected set; }
    }

}