using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
public class GUITetheredObject : MonoBehaviour
{
    private GameObject _tetheredObject;
    [SerializeField]
    private GameObject _container, _backdrop, _mesh;

    [SerializeField]
    private TextMeshPro _name, _size;

    public void HandleBeamInitializeEvent(TractorBeam beam)
    {
        // clear object meshes
        DestroyMesh();

        // disable backdrop
        DisableBackdrop();

        // reset name
        ClearName();

        // reset size
        ClearSize();
    }

    public void HandleUntetherEvent(TractorBeam beam, Beamable beamable)
    {
        if (beamable._gameObject != _tetheredObject) return;

        // reset tethered object
        _tetheredObject = null;

        // destroy existing mesh
        if (_mesh != null) Destroy(_mesh);

        // disable backdrop
        DisableBackdrop();

        // reset name
        ClearName();

        // reset size
        ClearSize();
    }


    public void HandleTetherEvent(TractorBeam beam, Beamable next)
    {
        // nothing to update with
        if (next == null) return;

        // nothing to build the ui from
        if (next._gameObject == null) return;

        // same object
        if (next._gameObject == _tetheredObject) return;

        // the same type of object was tethered
        // TODO: name mapping
        if (_tetheredObject != null && (Util.TrimName(next._gameObject.name) == Util.TrimName(_tetheredObject.name))) return;

        // destroy all previous meshes
        DestroyMesh();

        GameObject displayObject = next.DisplayObject();

        displayObject.transform.SetParent(_container.transform);

        // assign UI layer
        // TODO: constants
        displayObject.transform.SetLayerRecursively(5);

        // reset rotation
        displayObject.transform.localEulerAngles = Vector3.zero;

        // reset scale
        displayObject.transform.localScale = Vector3.one;

        // scale mesh to fit within bounds
        MeshFilters.ScaleChildObjectToMeshBounds(displayObject);


        // realign mesh to center pivot
        MeshFilter meshFilter = displayObject.GetComponentInChildren<MeshFilter>();
        Vector3 centerPos = MeshFilters.GetAnchorPivotPosition(meshFilter);
        meshFilter.transform.localPosition = -centerPos * meshFilter.transform.localScale.x;
        displayObject.transform.localPosition = Vector3.zero;

        // object name
        SetName(Util.TrimName(next._gameObject.name));

        // object size
        SetSize(string.Format("{0}m", next.Size()));

        // apply updated objects
        _tetheredObject = next._gameObject;
        _mesh = displayObject;

        // enable backdrop
        EnableBackdrop();

        // enable mesh
        _mesh.gameObject.SetActive(true);
    }

    public void HandleBeamableFixedUpdateEvent(TractorBeam beam, Beamable beamable)
    {
        if (_tetheredObject != null && _tetheredObject == beamable._gameObject)
        {
            Rigidbody rb = _mesh.GetComponent<Rigidbody>();
            if (rb != null) rb.angularVelocity = beamable._rb.angularVelocity;
        }
    }

    private void DestroyMesh()
    {
        foreach (Transform child in _container.transform) if (child.gameObject != null) Destroy(child.gameObject);
    }

    private void ClearName()
    {
        _name.text = "";
        _name.enabled = false;
    }

    private void ClearSize()
    {
        _size.text = "";
        _size.enabled = false;
    }


    private void SetName(string name)
    {
        _name.text = name;
        _name.enabled = true;
    }

    private void SetSize(string size)
    {
        _size.text = size;
        _size.enabled = true;
    }

    private void DisableBackdrop()
    {
        _backdrop?.SetActive(false);
    }

    private void EnableBackdrop()
    {
        _backdrop?.SetActive(true);
    }
}
