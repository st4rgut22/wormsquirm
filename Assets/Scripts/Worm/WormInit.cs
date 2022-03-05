using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Worm
{
    public class WormInit : WormBody
    {
        Dictionary<Direction, Quaternion> initWormRotateDict = new Dictionary<Direction, Quaternion>() // <Direction, Initial worm rotation>
            {
                { Direction.Up, Quaternion.Euler(0,0,-90) },
                { Direction.Left, Quaternion.Euler(0,0,0) },
                { Direction.Right, Quaternion.Euler(0,0,180) },
                { Direction.Back, Quaternion.Euler(-90, 0, -90) },
                { Direction.Forward, Quaternion.Euler(90, 0, -90) },
                { Direction.Down, Quaternion.Euler(180, 0, -90) }

            }; 

        private new void OnEnable()
        {
            base.OnEnable();
            Tunnel.CollisionManager.Instance.InitWormPositionEvent += onInitWormPosition;
        }

        public void onInitWormPosition(Vector3 initPos, Direction direction)
        {
            wormBase.setDirection(direction);
            transform.position = initPos; // add offset if necessary
            transform.rotation = initWormRotateDict[direction];
        }

        private new void OnDisable()
        {
            base.OnDisable();
            Tunnel.CollisionManager.Instance.InitWormPositionEvent -= onInitWormPosition;
        }
    }

}