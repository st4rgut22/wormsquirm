using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class ExtensionMethod
{
    public static GameObject Instantiate(this GameObject thisObj, GameObject original, Vector3 position, Transform parent, DirectionPair directionPair, Vector3Int nextCell, string id)
    {
        GameObject tunnelGO = GameObject.Instantiate(original, position, Quaternion.identity, parent);
        tunnelGO.name = id;

        Tunnel.Tunnel tunnel = tunnelGO.GetComponent<Tunnel.Tunnel>();

        tunnel.setHoleDirections(directionPair);

        tunnel.rotate(directionPair); // the type of rotation is set in Awake eg CornerRotation, StraightRotation

        Direction ingressDirection = directionPair.prevDir;
        if (directionPair.prevDir == Direction.None)
        {
            ingressDirection = directionPair.curDir;
        }

        tunnel.setCenter(Tunnel.Tunnel.BLOCK_SIZE, ingressDirection); // set center as offset in ingress direction

        tunnel.addCellToList(nextCell);
        return tunnelGO;
    } 
}
