using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WormUtility
{
    private class VectorDirPair
    {
        public float dist { get; private set; }
        public Direction dir { get; private set; }
        public VectorDirPair(Vector3 referenceVector, Vector3 dirVector, Direction dir)
        {
            float distance = Vector3.Distance(referenceVector, dirVector);
            dist = distance;
            this.dir = dir;
        }
    }

    public static Direction getDirection(Vector3 headPosition, Vector3 neckPosition)
    {
        float minDist = float.MaxValue;
        Direction closestDirection = Direction.None;

        List<VectorDirPair> vectorDirPairList = new List<VectorDirPair>();

        Vector3 normalizedVector = (headPosition - neckPosition).normalized;
        vectorDirPairList.Add(new VectorDirPair(Vector3.up, normalizedVector, Direction.Up));
        vectorDirPairList.Add(new VectorDirPair(Vector3.down, normalizedVector, Direction.Down));
        vectorDirPairList.Add(new VectorDirPair(Vector3.left, normalizedVector, Direction.Left));
        vectorDirPairList.Add(new VectorDirPair(Vector3.right, normalizedVector, Direction.Right));
        vectorDirPairList.Add(new VectorDirPair(Vector3.forward, normalizedVector, Direction.Forward));
        vectorDirPairList.Add(new VectorDirPair(Vector3.back, normalizedVector, Direction.Back));

        foreach (VectorDirPair vectorDirPair in vectorDirPairList)
        {
            if (vectorDirPair.dist < minDist)
            {
                closestDirection = vectorDirPair.dir;
                minDist = vectorDirPair.dist;
            }
        }
        Debug.Log("closest direction is " + closestDirection);
        return closestDirection;
    }
}
