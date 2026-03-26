using UnityEngine;

public static class Extensions
{
    public static void DestroyAllChildren(this Transform transform, bool includeInactive = false)
    {
        foreach (Transform child in transform)
        {
            if (includeInactive || child.gameObject.activeSelf)
                GameObject.Destroy(child.gameObject);
        }
    }
}
