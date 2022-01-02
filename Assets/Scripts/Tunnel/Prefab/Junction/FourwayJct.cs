using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Tunnel
{
    public class FourwayJct : Junction
    {

        new private void Awake()
        {
            holeCount = 4;
        }
    }

}