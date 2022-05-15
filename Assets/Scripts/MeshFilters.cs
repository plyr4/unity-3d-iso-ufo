using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class MeshFilters
{
    public static float GetObjectLargestBound(GameObject obj)
    {   
        MeshFilter meshFilter = obj.GetComponent<MeshFilter>();
        return meshFilter == null ? 0 : Mathf.Max(meshFilter.mesh.bounds.size.x, Mathf.Max(meshFilter.mesh.bounds.size.y, meshFilter.mesh.bounds.size.z));
    }


    public static float GetMeshFilterLargestBound(MeshFilter meshFilter)
    {   
        return meshFilter == null ? 0 : Mathf.Max(meshFilter.mesh.bounds.size.x, Mathf.Max(meshFilter.mesh.bounds.size.y, meshFilter.mesh.bounds.size.z));
    }

    public static Vector3 GetAnchorPivotPosition(MeshFilter meshFilter)
    {
        return meshFilter == null ? Vector3.zero : meshFilter.mesh.bounds.center;
    }

    public static void ScaleChildObjectToMeshBounds(GameObject obj)
    {
        MeshFilter meshFilter = obj.GetComponentInChildren<MeshFilter>();

        // nothing to scale to
        if (meshFilter == null) return;

        Mesh mesh = meshFilter.mesh;
        Bounds bounds = mesh.bounds;
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

    
    public static void ScaleChildObjectToStaticBounds(GameObject obj, float size)
    {
        // TODO: use input size scale
        MeshFilter meshFilter = obj.GetComponentInChildren<MeshFilter>();

        if (meshFilter == null) return;

        Mesh mesh = meshFilter.mesh;
        Bounds bounds = mesh.bounds;
        
        // TODO: use input size scale
        var szA = new Vector3 (size, size, size);
        
        var szB = bounds.size;

        var scale = meshFilter.transform.localScale;

        float largestMeshDimension = Mathf.Max(szB.x, Mathf.Max(szB.y, szB.z));
        float largestScaleDimension = Mathf.Max(scale.x, Mathf.Max(scale.y, scale.z));

        // TODO: make this smarter
        if (largestMeshDimension < 1f) return;
        if (largestScaleDimension < 1f) return;

        // TODO: use input size scale
        float scaleFactor = 1f / largestMeshDimension;

        scale *= scaleFactor;

        meshFilter.transform.localScale = scale;
    }
}

