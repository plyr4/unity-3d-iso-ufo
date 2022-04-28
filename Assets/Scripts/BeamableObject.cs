using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(ObjectRenderSnapable))]
[DisallowMultipleComponent]
public class BeamableObject : MonoBehaviour
{
    private Rigidbody _rb;
    private List<PixelizerMaterial> _pixelizerMaterials;

    [SerializeField]
    private float RandomRotationTorque = 10f;

    [SerializeField]
    private bool TrackBeamableObjectJointChanges = true;
    private bool isOutlined;
    [SerializeField]
    private bool isTethered;

    [SerializeField]
    private TractorBeam _beam;

    private GameObject _tether;
    private ConfigurableJoint _springJoint;

    LineRenderer line;
    CurvedLineRenderer curvedLine;

    private bool retractJoint;
    private bool hasSpun;

    private GameObject[] _drawPoints;

    private void Start()
    {
        if (_rb == null) _rb = GetComponent<Rigidbody>();
        SetMeshCollidersToConvex();
        gameObject.layer = 8;

        // prepare pixelizer materials
        MeshRenderer[] _meshRenderers = GetComponentsInChildren<MeshRenderer>();
        _pixelizerMaterials = new List<PixelizerMaterial>();
        for (int i = 0; i < _meshRenderers.Length; i++)
        {
            Material[] _materials = _meshRenderers[i].materials;
            for (int j = 0; j < _meshRenderers[i].materials.Length; j++)
            {
                Material _material = _meshRenderers[i].materials[j];
                if (_material.shader.name == "ProPixelizer/SRP/PixelizedWithOutline") _pixelizerMaterials.Add(new PixelizerMaterial(_material));
            }
        }
    }

    private void Update()
    {
        if (Tethered())
        {
            _rb.mass = 0.5f;
            if (TrackBeamableObjectJointChanges) CreateTether(null);

            // debug joint outlines 
            if (GameConstants.Instance()._beamAttributes.BeamDrawJoints) DrawJointLine();
            else if (line != null) ClearJointLine();

            UpdateJointProperties();

            // apply random torque once
            if (!hasSpun)
            {
                // ApplyRandomSpin();
                hasSpun = true;
            }

            // outline object
            if (!isOutlined)
            {
                isOutlined = true;
                ApplyObjectOutline();
            }
            if (retractJoint)
            {
                RetractJoint();
            }
            else
            {
                LockLinearLimit();
            }
        }
        else
        {
            _rb.mass = 1f;
            _rb.useGravity = true;
            // undo outline
            if (isOutlined)
            {
                isOutlined = false;
                RevertObjectOutline();
            }
            ClearJointLine();
        }
    }


    private void Reset()
    {
        if (_rb == null) _rb = GetComponent<Rigidbody>();

        // physics
        _rb.isKinematic = true;
        SetMeshCollidersToConvex();
        gameObject.layer = 8;
    }

    public bool Tethered()
    {
        return isTethered;
    }

    public void Tether(TractorBeam beam)
    {
        isTethered = true;
        _rb.isKinematic = false;
        _beam = beam;
        CreateTether(beam);
        UpdateJointProperties();
    }

    public void Untether()
    {
        isTethered = false;
        _beam = null;
        ConfigurableJoint joint = _springJoint;
        _springJoint = null;
        _rb.velocity = Vector3.zero;
        Destroy(joint);
    }


    public void RetractJoint()
    {
        retractJoint = true;
        _springJoint.connectedAnchor = Vector3.zero;
        var _linearLimit = new SoftJointLimit();
        _linearLimit.limit = _springJoint.linearLimit.limit - Time.deltaTime * GameConstants.Instance()._beamAttributes.BeamRetractionSpeed;
        _springJoint.linearLimit = _linearLimit;
    }

    public bool Retracted()
    {
        return _springJoint.linearLimit.limit <= 0.001f;
    }

    private void LockLinearLimit()
    {
        var _linearLimit = new SoftJointLimit();
        _linearLimit.limit = GameConstants.Instance()._beamAttributes.BeamGrabLinearLimit;
        _springJoint.linearLimit = _linearLimit;
    }

    private float AnchorDepth()
    {
        return _beam._beamDepth * GameConstants.Instance()._beamAttributes.BeamGrabAnchorDepthCoefficient;
    }

    private void CreateTether(TractorBeam beam)
    {
        if (_springJoint == null) _springJoint = gameObject.AddComponent<ConfigurableJoint>();
        if (beam != null) _springJoint.connectedBody = beam.GetTetherAnchor();
        _springJoint.xMotion = GameConstants.Instance()._beamAttributes.BeamableObjectJoint.xMotion;
        _springJoint.yMotion = GameConstants.Instance()._beamAttributes.BeamableObjectJoint.yMotion;
        _springJoint.zMotion = GameConstants.Instance()._beamAttributes.BeamableObjectJoint.zMotion;
        _springJoint.angularXMotion = GameConstants.Instance()._beamAttributes.BeamableObjectJoint.angularXMotion;
        _springJoint.angularYMotion = GameConstants.Instance()._beamAttributes.BeamableObjectJoint.angularYMotion;
        _springJoint.angularZMotion = GameConstants.Instance()._beamAttributes.BeamableObjectJoint.angularZMotion;

        _springJoint.anchor = GameConstants.Instance()._beamAttributes.BeamableObjectJoint.anchor;
        _springJoint.autoConfigureConnectedAnchor = GameConstants.Instance()._beamAttributes.BeamableObjectJoint.autoConfigureConnectedAnchor;

        _springJoint.xDrive = GameConstants.Instance()._beamAttributes.BeamableObjectJoint.xDrive;
        _springJoint.yDrive = GameConstants.Instance()._beamAttributes.BeamableObjectJoint.yDrive;
        _springJoint.zDrive = GameConstants.Instance()._beamAttributes.BeamableObjectJoint.zDrive;

        _springJoint.angularXDrive = GameConstants.Instance()._beamAttributes.BeamableObjectJoint.angularXDrive;
        _springJoint.angularYZDrive = GameConstants.Instance()._beamAttributes.BeamableObjectJoint.angularYZDrive;

        _springJoint.linearLimitSpring = GameConstants.Instance()._beamAttributes.BeamableObjectJoint.linearLimitSpring;
        _springJoint.angularYZDrive = GameConstants.Instance()._beamAttributes.BeamableObjectJoint.angularYZDrive;

        _springJoint.massScale = GameConstants.Instance()._beamAttributes.BeamableObjectJoint.massScale;
        _springJoint.connectedMassScale = GameConstants.Instance()._beamAttributes.BeamableObjectJoint.connectedMassScale;
    }

    private void UpdateJointProperties()
    {
        if (!retractJoint)
        {
            SoftJointLimit _linearLimit = new SoftJointLimit();
            _linearLimit.limit = GameConstants.Instance()._beamAttributes.BeamGrabLinearLimit;
            _springJoint.linearLimit = _linearLimit;
        }
        _springJoint.connectedAnchor = Vector3.zero - Vector3.up * AnchorDepth();
    }

    public void ApplyRandomSpin()
    {
        // TODO: improve this
        float min = -20f;
        float max = 20f;
        float xzModifier = 5f;
        var rot = new Vector3(Random.Range(min, max) * xzModifier, Random.Range(min, max), Random.Range(min, max) * xzModifier);
        rot.Normalize();
        _rb.AddTorque(rot * RandomRotationTorque);
    }

    private void ApplyObjectOutline()
    {
        for (int i = 0; i < _pixelizerMaterials.Count; i++) _pixelizerMaterials[i].ApplyOutline(GameConstants.Instance()._beamAttributes.BeamGrabOutlineColor, 69);
    }

    private void RevertObjectOutline()
    {
        for (int i = 0; i < _pixelizerMaterials.Count; i++) _pixelizerMaterials[i].RevertOutline();
    }

    private void SetMeshCollidersToConvex()
    {
        MeshCollider[] _meshColliders = GetComponentsInChildren<MeshCollider>();
        for (int i = 0; i < _meshColliders.Length; i++)
        {
            _meshColliders[i].convex = true;
        }
    }

    private void DrawJointLine()
    {
        // initialize line renderers
        if (line == null)
        {
            line = gameObject.AddComponent<LineRenderer>();

            // recreate the curved line renderer
            if (curvedLine != null) Destroy(curvedLine);
            curvedLine = gameObject.AddComponent<CurvedLineRenderer>();
        }

        // set line renderer properties
        line.startWidth = GameConstants.Instance()._beamAttributes.BeamGrabJointRenderWidth;
        line.endWidth = GameConstants.Instance()._beamAttributes.BeamGrabJointRenderWidth;
        line.material = GameConstants.Instance()._beamAttributes.BeamGrabJointRenderMaterial;
        line.startColor = GameConstants.Instance()._beamAttributes.BeamGrabJointRenderColor;
        line.endColor = GameConstants.Instance()._beamAttributes.BeamGrabJointRenderColor;

        // create the curved line points
        DrawJointPoints();
    }

    private void DrawJointPoints()
    {
        if (_drawPoints == null || _drawPoints.Length == 0) _drawPoints = new GameObject[3];
        Vector3[] _drawTransforms = GetJointDrawPointTransforms();

        for (int i = 0; i < _drawPoints.Length; i++)
        {
            if (_drawPoints[i] == null) _drawPoints[i] = CreateJointDrawPoint(_drawTransforms[i]);
            GameObject _o = _drawPoints[i];
            _o.transform.position = _drawTransforms[i];
            _drawPoints[i] = _o;
        }
    }

    private Vector3[] GetJointDrawPointTransforms()
    {
        return new Vector3[3] { _beam.transform.position, _beam.transform.position + _springJoint.connectedAnchor, transform.position };
    }

    GameObject CreateJointDrawPoint(Vector3 drawPos)
    {
        GameObject _gameObject = new GameObject("Curved Line Point", typeof(CurvedLinePoint));
        _gameObject.transform.parent = transform;
        _gameObject.transform.position = drawPos;
        return _gameObject;
    }

    void RedrawJointDrawPoint(GameObject drawPoint, Vector3 drawPos)
    {
        drawPoint.transform.position = drawPos;
    }

    private void ClearJointLine()
    {
        if (_drawPoints != null)
        {
            for (int i = _drawPoints.Length - 1; i >= 0; i--)
            {
                Destroy(_drawPoints[i]);
            }
        }
        if (curvedLine != null) Destroy(curvedLine);
        if (line != null)
        {
            line.endWidth = 0f;
            line.startWidth = 0f;
            Destroy(line);
        }
    }
}
