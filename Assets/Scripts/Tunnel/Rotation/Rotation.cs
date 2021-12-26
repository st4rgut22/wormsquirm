using System.Collections.Generic;
using UnityEngine;

namespace Rotation
{
    public class DirectionList : IEqualityComparer<List<Direction>>
    {
        public bool Equals(List<Direction> x, List<Direction> y) // used to check for equality when there is a has collision
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

        public int GetHashCode(List<Direction> obj) // used to get an object with a key
        {
            obj.Sort();
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

        private void Start()
        {
            if (rotationDict.Count > 0) // ignores straight tunnel which doesnt have a rotation dict
            {
                sortRotationDict();
            }
        }

        /**
         * Sort the direction list
         */
        protected void sortRotationDict()
        {
            Dictionary<List<Direction>, Quaternion> upDict = rotationDict[Direction.Up];
            Dictionary<List<Direction>, Quaternion> downDict = rotationDict[Direction.Down];
            Dictionary<List<Direction>, Quaternion> leftDict = rotationDict[Direction.Left];
            Dictionary<List<Direction>, Quaternion> rightDict = rotationDict[Direction.Right];
            Dictionary<List<Direction>, Quaternion> forwardDict = rotationDict[Direction.Back];
            Dictionary<List<Direction>, Quaternion> backDict = rotationDict[Direction.Forward];
            updateLists(upDict);
            updateLists(downDict);
            updateLists(leftDict);
            updateLists(rightDict);
            updateLists(forwardDict);
            updateLists(backDict);
        }

        private void updateLists(Dictionary<List<Direction>, Quaternion>dirListDict)
        {
            List<List<Direction>> dirListsToUpdate = new List<List<Direction>>();
            foreach (KeyValuePair<List<Direction>, Quaternion> keyValuePair in dirListDict)
            {
                dirListsToUpdate.Add(keyValuePair.Key);
            }
            sortDirectionListKey(dirListDict, dirListsToUpdate);
        }

        private void sortDirectionListKey(Dictionary<List<Direction>, Quaternion> dirListDict, List<List<Direction>> listsToUpdate)
        {
            foreach (List<Direction> dirListToUpdate in listsToUpdate)
            {
                List<Direction> updatedList = new List<Direction>(dirListToUpdate);
                Quaternion savedRotation = dirListDict[dirListToUpdate];
                updatedList.Sort();
                dirListDict.Remove(dirListToUpdate);
                dirListDict.Add(updatedList, savedRotation);
            }
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