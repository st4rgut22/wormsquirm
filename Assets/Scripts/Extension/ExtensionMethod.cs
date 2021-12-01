using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class ExtensionMethod
{
    public static GameObject Instantiate(this GameObject thisObj, GameObject original, Vector3 position, Transform parent, DirectionPair directionPair, Vector3Int nextCell)
    {
        GameObject tunnelGO = GameObject.Instantiate(original, position, Quaternion.identity, parent);
        Tunnel.Tunnel tunnel = tunnelGO.GetComponent<Tunnel.Tunnel>();
        tunnel.setDirectionPair(directionPair);
        tunnel.rotate(directionPair); // the type of rotation is set in Awake eg CornerRotation, StraightRotation
        tunnel.addCellToList(nextCell);
        return tunnelGO;
    } 
}
