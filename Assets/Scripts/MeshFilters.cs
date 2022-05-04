using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class MeshFilters
{
   
    public static Vector3 GetAnchorPivotPosition(MeshFilter meshFilter)
    {
        if (meshFilter == null)
        {
            return Vector3.zero;
        }
        return meshFilter.mesh.bounds.center;
    }
}
