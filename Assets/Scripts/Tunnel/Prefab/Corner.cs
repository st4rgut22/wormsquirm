using UnityEngine;

namespace Tunnel
{
    public class Corner : Tunnel
    {
        private void Awake()
        {
            isStopped = true;

            rotation = new Rotation.Corner();
        }
    }
}