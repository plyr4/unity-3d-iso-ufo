using UnityEngine;

public static class Util   
{
 
    public static float Round(float f, float n)
    {
        return Mathf.Round(f * n) / n;
    }
    public static string TrimName(string name)
    {
        int index = name.IndexOf(" ");
        if (index >= 0) name = name.Substring(0, index);
        name = name.Replace('-', ' ');
        return name;
    }

    public static void SetLayerRecursively(this Transform parent, int layer)
    {
        parent.gameObject.layer = layer;
 
        for (int i = 0, count = parent.childCount; i < count; i++)
        {
            parent.GetChild(i).SetLayerRecursively(layer);
        }
    }
}
