using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Tunnel
{
    public class JctTester : MonoBehaviour
    {
        List<Vector3> ingressPosList;

        [SerializeField]
        private Transform jctNetwork;

        private Dictionary<Vector3, Vector3> raycastOriginDict;
        private Dictionary<Vector3, string> raycastDirNameDict;
        private Dictionary<Vector3, string> ingressPosDict;

        private Dictionary<string, Vector3> rotationDict;
        private Dictionary<string, string> jctNameDict;
        private Dictionary<string, List<Vector3>> raycastOffsetDict;
        private Dictionary<string, string> oppDirDict;

        private Vector3 downPos = Vector3.zero;
        private Vector3 upPos = new Vector3(0, 1f, 0);
        private Vector3 rightPos = new Vector3(.5f, .5f, 0);
        private Vector3 leftPos = new Vector3(-.5f, .5f, 0);
        private Vector3 forwardPos = new Vector3(0, .5f, .5f);
        private Vector3 backPos = new Vector3(0, .5f, -.5f);

        private Vector3 startPos = new Vector3(-10, -10, 10);

        private const float OFFSET_MULTIPLIER = .1f;
        private const float CUBE_EDGE_MULTIPLIER = .375f;

        private List<Vector3> dirList = new List<Vector3>() { Vector3.left, Vector3.right, Vector3.up, Vector3.down, Vector3.forward, Vector3.back };

        private List<int> eulerAngleList;

        private void Awake()
        {
            ingressPosList = new List<Vector3>() { downPos, upPos, rightPos, leftPos, forwardPos, backPos };
            rotationDict = new Dictionary<string, Vector3>();
            jctNameDict = new Dictionary<string, string>();

            raycastOffsetDict = new Dictionary<string, List<Vector3>>
            {
                {"up", new List<Vector3>(){ Vector3.right, Vector3.left, Vector3.back, Vector3.forward, Vector3.right+Vector3.back, Vector3.right+Vector3.forward, Vector3.left+Vector3.back, Vector3.left+ Vector3.forward } },
                {"down", new List<Vector3>(){ Vector3.right, Vector3.left, Vector3.back, Vector3.forward, Vector3.right + Vector3.back, Vector3.right + Vector3.forward, Vector3.left + Vector3.back, Vector3.left + Vector3.forward } },
                {"left", new List<Vector3>(){ Vector3.back, Vector3.forward, Vector3.up, Vector3.down, Vector3.back+Vector3.up, Vector3.back+Vector3.down, Vector3.forward+Vector3.up, Vector3.forward+Vector3.down } },
                {"right", new List<Vector3>(){ Vector3.back, Vector3.forward, Vector3.up, Vector3.down, Vector3.back + Vector3.up, Vector3.back + Vector3.down, Vector3.forward + Vector3.up, Vector3.forward + Vector3.down } },
                {"forward", new List<Vector3>(){ Vector3.down, Vector3.up, Vector3.left, Vector3.right, Vector3.down+Vector3.left, Vector3.down+Vector3.right, Vector3.up+Vector3.left, Vector3.up+Vector3.right } },
                {"back", new List<Vector3>(){ Vector3.down, Vector3.up, Vector3.left, Vector3.right, Vector3.down + Vector3.left, Vector3.down + Vector3.right, Vector3.up + Vector3.left, Vector3.up + Vector3.right } },
            };

            oppDirDict = new Dictionary<string, string>
            {
                { "up", "down" },
                { "down", "up" },
                { "left", "right" },
                { "right", "left" },
                { "forward", "back" },
                { "back", "forward" }
            };

            raycastOriginDict = new Dictionary<Vector3, Vector3>
                    {
                        { Vector3.up, new Vector3(0, -.5f, 0) },
                        { Vector3.down, new Vector3(0, 1.5f, 0) },
                        { Vector3.right, new Vector3(-1, .5f, 0) },
                        { Vector3.left, new Vector3(1, .5f, 0) },
                        { Vector3.forward, new Vector3(0, .5f, -1) },
                        { Vector3.back, new Vector3(0, .5f, 1) },
                    };

            raycastDirNameDict = new Dictionary<Vector3, string>
                    {
                        { Vector3.up, "up" },
                        { Vector3.down, "down" },
                        { Vector3.right, "right" },
                        { Vector3.left, "left" },
                        { Vector3.forward, "forward" },
                        { Vector3.back, "back" },
                    };

            // <center of cube side, worm direction>
            ingressPosDict = new Dictionary<Vector3, string>
                    {
                        { upPos, "down" },
                        { downPos, "up" },
                        { leftPos, "right" },
                        { rightPos, "left" },
                        { backPos, "forward" },
                        { forwardPos, "back" },
                    };

            eulerAngleList = new List<int>() { 0, 90, 180, 270 };
        }

        // Start is called before the first frame update
        void Start()
        {
            StartCoroutine(setJctTransform(jctNetwork));
        }


        IEnumerator setJctTransform(Transform jctNetwork)
        {
            foreach (Transform jct in jctNetwork)
            {
                if (jct.gameObject.activeSelf)
                {
                    foreach (Vector3 ingressPos in ingressPosList)
                    {

                        foreach (float x in eulerAngleList)
                        {
                            foreach (float y in eulerAngleList)
                            {
                                foreach (float z in eulerAngleList)
                                {
                                    jct.localPosition = ingressPos;
                                    string ingressDir = ingressPosDict[ingressPos];

                                    Quaternion rotation = Quaternion.Euler(x, y, z);
                                    jct.rotation = rotation;
                                    yield return new WaitForSeconds(.2f);
                                    fireRaycastAtHoles(jct, ingressDir);
                                }
                            }
                        }
                    }
                    jct.transform.position = startPos;
                }
            }
            outputRotation();
        }

        void outputRotation()
        {
            print("in total there are " + rotationDict.Count + " possible entries");
            foreach (KeyValuePair<string, Vector3> entry in rotationDict)
            {
                string[] ingressArr = entry.Key.Split(' ');
                string ingressDir = ingressArr[0];
                string remainingIngress = "";
                for (int i=1;i<ingressArr.Length;i++)
                {
                    remainingIngress += " ";
                    remainingIngress += ingressArr[i];
                }
                Vector3 rot = entry.Value;
                string jctName = jctNameDict[entry.Key];

                print("ingress dir is " + ingressDir + " remaining ingress list is (" + remainingIngress + ") rotation is " + rot + " for gameobject " + jctName);
            }
        }

        /**
         * fire raycast in 6 possible directions
         * 
         * @holeCount # of holes for jct segment
         */
        void fireRaycastAtHoles(Transform jctTransform, string ingressDir)
        {
            Junction junction = jctTransform.gameObject.GetComponent<Junction>();
            List<string> otherIngressDirList = new List<string>();

            int noHitCount = 0;
            List<Vector3> hitDirList = new List<Vector3>();

            foreach (Vector3 raycastDir in dirList)
            {
                Vector3 raycastOrigin = raycastOriginDict[raycastDir]; // set origin pointing toward center of junction going toward an hole
                List<Vector3> offsetList = raycastOffsetDict[raycastDirNameDict[raycastDir]];

                bool anyIsHit = false;
                Vector3 raycastHitOrigin = raycastOrigin + CUBE_EDGE_MULTIPLIER * offsetList[0];

                foreach (Vector3 offset in offsetList)
                {
                    RaycastHit hit;

                    Vector3 offsetOrigin = raycastOrigin + OFFSET_MULTIPLIER * offset;
                    bool isHit = Physics.Raycast(offsetOrigin, raycastDir, out hit, maxDistance: 1f);
                    //if (raycastDir == Vector3.forward || raycastDir == Vector3.back)
                    //{
                    //    Debug.DrawRay(offsetOrigin, raycastDir, Color.green, 30);
                    //}
                    if (isHit)
                    {
                        anyIsHit = true;
                    }
                }
                RaycastHit hitEdge;
                bool hitCubeEdge = Physics.Raycast(raycastHitOrigin, raycastDir, out hitEdge, maxDistance: 1);

                if (!anyIsHit && hitCubeEdge) // clear path, holes are aligned hopefully  
                {
                    noHitCount += 1;
                    string otherIngressDir = raycastDirNameDict[raycastDir];
                    if (otherIngressDir == ingressDir)
                    {
                        otherIngressDirList.Insert(0, ingressDir);
                    }
                    else
                    {
                        string holeLoc = oppDirDict[otherIngressDir]; // opp of ingress dir is hole loc
                        otherIngressDirList.Add(otherIngressDir);
                    }
                    hitDirList.Add(raycastDir);
                }
            }

            if (junction.holeCount == noHitCount)
            {
                addValidTransform(jctTransform, hitDirList, otherIngressDirList);
            }
        }

        /**
         * when raycast non-hits === # of holes, then this is a correct alignment
        */
        void addValidTransform(Transform transform, List<Vector3>hitDirList, List<string> holeLocs)
        {
            string key = string.Join(" ", holeLocs.ToArray());

            //print("a valid transform for junction " + transform.name + " with rotation " + transform.eulerAngles + " position " + transform.position);
            if (!rotationDict.ContainsKey(key))
            {
                rotationDict.Add(key, transform.eulerAngles);
                jctNameDict.Add(key, transform.gameObject.name);
            }
        }
    }
}
