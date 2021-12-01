using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Rotation
{
    public interface IRotation
    {
        public Quaternion rotate(DirectionPair dirPair);
    }

}