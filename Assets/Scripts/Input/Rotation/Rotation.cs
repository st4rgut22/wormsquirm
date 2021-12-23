using System.Collections.Generic;
using UnityEngine;

namespace Rotation
{
    public class DirectionList : IEqualityComparer<List<Direction>>
    {
        public bool Equals(List<Direction> x, List<Direction> y)
        {
            if (x.Count != y.Count)
            {
                throw new System.Exception("Not a valid comparison of direciton lists");
            }
            for (int i=0;i<x.Count;i++)
            {                
                if (!x.Contains(y[i]))
                {
                    return false;
                }
            }
            return true;
        }

        public int GetHashCode(List<Direction> obj)
        {
            string key = string.Join("", obj.ToArray());
            return key.GetHashCode();
        }
    }

    public class Rotation: MonoBehaviour
    {
        [SerializeField]
        public GameObject rotatedPrefab;

        public Dictionary<Direction, Dictionary<List<Direction>, Quaternion>> rotationDict;

        protected void Awake()
        {
            // Dictionary<ingress hole direction, Dictionary<egress hold direction, rotation>>()            
             rotationDict = new Dictionary<Direction, Dictionary<List<Direction>, Quaternion>>();
        }

        public void rotate(Direction ingressDir, List<Direction> egressDirList, Transform tunnel)
        {
            Quaternion rotation = getRotation(ingressDir, egressDirList);
            tunnel.rotation = rotation;
        }

        public virtual Quaternion getRotation(Direction ingressDir, List<Direction>egressDirList)
        {
            return rotationDict[ingressDir][egressDirList];
        }

        public virtual bool isRotationInRotationDict(Direction prevTunnelDirection, List<Direction> otherDirList)
        {
            return rotationDict[prevTunnelDirection].ContainsKey(otherDirList);
        }

        /**
         * Use the previous and current tunnel directiosn to get rotation and offset of the corner piece
         */
        public Quaternion getRotationFromDirection(Direction prevTunnelDirection, List<Direction> otherDirList)
        {
            Quaternion rotation = rotationDict[prevTunnelDirection][otherDirList];
            Debug.Log("cur " + prevTunnelDirection + " prev " + prevTunnelDirection + " euler angles is " + rotation.eulerAngles);
            return rotation;
        }
    }

}