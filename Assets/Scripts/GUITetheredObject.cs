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

    public void HandleUntetherEvent(GameObject obj, Beamable beamable)
    {
        if (beamable._gameObject != _tetheredObject) return;

        // reset tethered object
        _tetheredObject = null;

        // destroy existing mesh
        if (_mesh != null) Destroy(_mesh);

        // disable backdrop
        _backdrop?.SetActive(false);

        // reset name
        _name.enabled = false;
        _name.text = "";

        // reset size
        _size.enabled = false;
        _size.text = "";
    }

    public void HandleTetherEvent(GameObject obj, Beamable next)
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
        foreach (Transform child in _container.transform) if (child.gameObject != null) Destroy(child.gameObject);

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
        meshFilter.transform.localPosition =  -centerPos * meshFilter.transform.localScale.x;
        displayObject.transform.localPosition = Vector3.zero;
        
        // object name
        _name.text = Util.TrimName(next._gameObject.name);
        _name.enabled = true;

        // object size
        _size.text = string.Format("{0}m", next.Size());
        _size.enabled = true;

        // apply updated objects
        _tetheredObject = next._gameObject;
        _mesh = displayObject;

        // enable backdrop
        _backdrop.SetActive(true);

        // enable mesh
        _mesh.gameObject.SetActive(true);
    }

    public void HandleUpdateTetheredObjectEvent(GameObject obj, Beamable tethered)
    {
        if (_tetheredObject != null && _tetheredObject == tethered._gameObject)
        {
            Rigidbody rb = _mesh.GetComponent<Rigidbody>();
            if (rb != null) rb.angularVelocity = tethered._rb.angularVelocity;
        }
    }
}
