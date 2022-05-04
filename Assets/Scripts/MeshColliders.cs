using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MeshColliders : MonoBehaviour
{
    public static void SetMeshCollidersToConvex(GameObject obj)
    {
        MeshCollider[] _meshColliders = obj.GetComponentsInChildren<MeshCollider>();
        for (int i = 0; i < _meshColliders.Length; i++)
        {
            _meshColliders[i].convex = true;
        }
    }
}
