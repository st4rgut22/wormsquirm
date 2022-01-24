using UnityEngine;

public class GeneralUtility : MonoBehaviour
{
    public static Transform findChildRecursively(string childName, Transform parentObject)
    {
        if (parentObject.name == childName)
        {
            return parentObject;
        }

        foreach (Transform childTransform in parentObject)
        {
            Transform transform =  findChildRecursively(childName, childTransform);
            if (transform != null)
            {
                return transform;
            }
        }

        return null;
    }
}
