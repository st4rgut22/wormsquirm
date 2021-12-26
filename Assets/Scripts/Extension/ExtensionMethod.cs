using UnityEngine;
using System.Collections.Generic;

public static class ExtensionMethod
{

    public static GameObject instantiate(this GameObject thisObj, Vector3 position, Transform parent, Transform prefabParent, DirectionPair directionPair, List<Direction> originalHoleDirectionList, string id)
    {
        Direction ingressDir = directionPair.prevDir == Direction.None ? directionPair.curDir : directionPair.prevDir;

        Rotation.Rotation rotation = Tunnel.Manager.GetPrefabFromHoleList(ingressDir, originalHoleDirectionList, prefabParent).GetComponent<Rotation.Rotation>();
        GameObject prefab = rotation.rotatedPrefab;

        GameObject tunnelGO = GameObject.Instantiate(prefab, position, Quaternion.identity, parent);
        tunnelGO.name = id;

        Tunnel.Tunnel tunnel = tunnelGO.GetComponent<Tunnel.Tunnel>();

        tunnel.holeDirectionList = new List<Direction>(originalHoleDirectionList);
        tunnel.setHoleDirections(directionPair);

        rotation.rotate(ingressDir, originalHoleDirectionList, tunnel.transform); // the type of rotation is set in Awake eg CornerRotation, StraightRotation

        if (directionPair.prevDir == Direction.None)
        {
            ingressDir = directionPair.curDir;
        }

        tunnel.setCenter(Tunnel.Tunnel.BLOCK_SIZE, ingressDir); // set center as offset in ingress direction

        return tunnelGO;
    } 
}
