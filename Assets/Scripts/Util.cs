using UnityEngine;

public static class Util   
{
 
    public static float Round(float f, float n)
    {
        return Mathf.Round(f * n) / n;
    }

}
