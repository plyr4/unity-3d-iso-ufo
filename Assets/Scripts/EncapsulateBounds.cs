using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EncapsulateBounds : MonoBehaviour
{
    [SerializeField] private GameObject _target;
    [SerializeField] private MeshFilter _meshFilter;
    void Start()
    {
        _meshFilter = _target.GetComponent<MeshFilter>();
        MeshFilters.ScaleObjectToMeshBounds(_meshFilter);
    }

}
