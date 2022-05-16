using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.ProBuilder;

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
    public static void ScaleVertices(MeshFilter meshFilter, float factor, Vector3[] baseVertices, bool recalculateNormals)
    {
        Mesh mesh = meshFilter.mesh;
        if (baseVertices == null)
            baseVertices = mesh.vertices;

        var vertices = new Vector3[baseVertices.Length];
        for (var i = 0; i < vertices.Length; i++)
        {
            Vector3 vertex = baseVertices[i];
            vertex.x = vertex.x * factor;
            vertex.y = vertex.y * factor;
            vertex.z = vertex.z * factor;

            vertices[i] = vertex;
        }

        mesh.vertices = vertices;

        if (recalculateNormals)
            mesh.RecalculateNormals();
        mesh.RecalculateBounds();
    }
    public static Vector3[] BaseVertices(MeshFilter meshFilter )
    {
        return meshFilter.mesh.vertices;
    }

    static float ScaleFactor(MeshFilter meshFilter)
    {

        Mesh mesh = meshFilter.mesh;
        Bounds bounds = mesh.bounds;

        var szB = bounds.size;
        float largestMeshDimension = Mathf.Max(szB.x, Mathf.Max(szB.y, szB.z));
        float scaleFactor = 1f / largestMeshDimension;


        return scaleFactor;
    }
}

