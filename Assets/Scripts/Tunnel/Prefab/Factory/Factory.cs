using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Tunnel
{
    public class Factory : MonoBehaviour
    {
        public static NewTunnelFactory newTunnelFactory;
        public static ModTunnelFactory modTunnelFactory;

        private void Awake()
        {
            newTunnelFactory = new NewTunnelFactory();
            modTunnelFactory = new ModTunnelFactory();
        }

    }

}