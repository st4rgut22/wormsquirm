using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class VectorExtension
{
    public static Vector3Int castToVector3Int(this Vector3 thisObj)
    {
        Vector3Int castedVector = new Vector3Int((int)thisObj.x, (int)thisObj.y, (int)thisObj.z);
        return castedVector;
    }
}
