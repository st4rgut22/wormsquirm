using UnityEngine;
using System.Collections.Generic;

namespace Tunnel
{
    public static class ExtensionMethod
    {

        public static GameObject instantiate(this GameObject thisObj, Vector3 position, Transform parent, Transform prefabParent, DirectionPair directionPair, List<Direction> egressHoleDirectionList, string id)
        {
            Direction ingressDir = directionPair.prevDir == Direction.None ? directionPair.curDir : directionPair.prevDir;

            Rotation.Rotation rotation = TunnelManager.Instance.GetPrefabFromHoleList(ingressDir, egressHoleDirectionList, prefabParent).GetComponent<Rotation.Rotation>();
            GameObject prefab = rotation.rotatedPrefab;

            GameObject tunnelGO = GameObject.Instantiate(prefab, position, Quaternion.identity, parent);
            tunnelGO.name = id;

            Tunnel tunnel = tunnelGO.GetComponent<Tunnel>();

            tunnel.holeDirectionList = new List<Direction>(egressHoleDirectionList);
            tunnel.setHoleDirections(directionPair);

            rotation.rotate(ingressDir, egressHoleDirectionList, tunnel.transform); // the type of rotation is set in Awake eg CornerRotation, StraightRotation

            if (directionPair.prevDir == Direction.None)
            {
                ingressDir = directionPair.curDir;
            }

            tunnel.setCenter(Tunnel.BLOCK_SIZE, ingressDir); // set center as offset in ingress direction

            return tunnelGO;
        }

        /**
         * When a straight tunnel is sliced, two straight tunnels are created one of which is new
         * 
         * @tunnelParent parent transform
         * @ingressPosition enter hole position of tunnel
         * @holeDirectionList list of directions of entry for tunnel
         */
        public static Tunnel instantiateSliced(this GameObject thisObj, Transform tunnelParent, Vector3 ingressPosition, List<Direction> holeDirectionList)
        {
            GameObject tunnelCopy = GameObject.Instantiate(thisObj, tunnelParent);
            Straight copiedTunnel = tunnelCopy.GetComponent<Straight>();
            copiedTunnel.ingressPosition = ingressPosition;
            copiedTunnel.holeDirectionList = holeDirectionList;
            copiedTunnel.setIsSliced(); // tunnel should not be closed off with a deadend
            return copiedTunnel;
        }
    }

}