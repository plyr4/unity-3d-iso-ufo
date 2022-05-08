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
    public static void ScaleObjectToMeshBounds(MeshFilter meshFilter)
    {

        Mesh _mesh = meshFilter.mesh;
        Bounds bounds = _mesh.bounds;
        var szA = Vector3.one;
        var szB = bounds.size;
        var xR = szA.x / szB.x;
        var yR = szA.y / szB.y;
        var zR = szA.z / szB.z;

        var scale = meshFilter.transform.localScale;

        if (xR < zR)
        {
            if (xR < yR)
            {
                // xR min
                var rrR = meshFilter.transform.localScale.x * xR;
                scale = new Vector3(xR, meshFilter.transform.localScale.y * rrR, meshFilter.transform.localScale.z * rrR);
            }
            else
            {
                // yR min
                var rrR = meshFilter.transform.localScale.y * yR;
                scale = new Vector3(meshFilter.transform.localScale.x * rrR, yR, meshFilter.transform.localScale.z * rrR);
            }

        }
        else
        {
            if (zR < yR)
            {
                // zR min
                var rrR = meshFilter.transform.localScale.z * zR;
                scale = new Vector3(meshFilter.transform.localScale.x * rrR, meshFilter.transform.localScale.y * rrR, zR);
            }
            else
            {
                // yR min
                var rrR = meshFilter.transform.localScale.y * yR;
                scale = new Vector3(meshFilter.transform.localScale.x * rrR, yR, meshFilter.transform.localScale.z * rrR);
            }
        }



        meshFilter.transform.localScale = scale;
    }
}

