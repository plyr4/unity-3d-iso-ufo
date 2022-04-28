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
        _drawPoints = new GameObject[3];

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

            UpdateJointProperties();

            // apply random torque once
            if (!hasSpun)
            {
                ApplyRandomSpin();
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
        }
        DrawTether();
    }

    private void FixedUpdate()
    {
    }

    private void DrawTether()
    {
        if (Tethered())
        {
            // TODO: reimplement this - performance bottleneck
            //
            if (GameConstants.Instance()._beamAttributes.BeamDrawJoints) DrawJointLine();
            else if (GameConstants.Instance()._beamAttributes.BeamDrawJointsCurved) DrawJointLineCurved();
            else DestroyJointLine();
        }
        else
        {
            DestroyJointLine();
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
        hasSpun = false;
        ConfigurableJoint joint = _springJoint;
        _springJoint = null;
        _rb.velocity = _rb.velocity * 0.5f;
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

        _springJoint.anchor = transform.TransformDirection(AnchorPivotPosition());
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
        float xzModifier = 3f;

        float i = Random.Range(1f, 10f);
        int sign = Random.Range(0, 2) == 0 ? -1 : 1;
        float xRotation = 10 + 2 * i * sign * xzModifier;

        sign = Random.Range(0, 2) == 0 ? -1 : 1;
        i = Random.Range(1f, 10f);
        float yRotation = 5 + 2 * i * sign;

        sign = Random.Range(0, 2) == 0 ? -1 : 1;
        i = Random.Range(1f, 10f);
        float zRotation = 10 + 2 * i * sign * xzModifier;

        var rot = new Vector3(xRotation, yRotation, zRotation);
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
        // initialize line renderer
        if (line == null)
        {
            line = gameObject.AddComponent<LineRenderer>();
        }

        // set default line renderer properties
        line.startWidth = GameConstants.Instance()._beamAttributes.BeamGrabJointRenderStartWidth;
        line.endWidth = GameConstants.Instance()._beamAttributes.BeamGrabJointRenderEndWidth;
        line.material = GameConstants.Instance()._beamAttributes.BeamGrabJointRenderMaterial;
        line.startColor = GameConstants.Instance()._beamAttributes.BeamGrabJointRenderColor;
        line.endColor = GameConstants.Instance()._beamAttributes.BeamGrabJointRenderColor;

        // create or update the draw points
        Vector3[] _drawTransforms = DrawTransforms();
        line.positionCount = _drawTransforms.Length;
        for (int i = 0; i < _drawTransforms.Length; i++)
        {
            line.SetPosition(i, _drawTransforms[i]);
        }
    }

    
    private void DrawJointLineCurved()
    {
        // initialize line renderer
        if (line == null)
        {
            line = gameObject.AddComponent<LineRenderer>();

            // set default line renderer properties
            line.startWidth = GameConstants.Instance()._beamAttributes.BeamGrabJointRenderStartWidth;
            line.endWidth = GameConstants.Instance()._beamAttributes.BeamGrabJointRenderEndWidth;
            line.material = GameConstants.Instance()._beamAttributes.BeamGrabJointRenderMaterial;
            line.startColor = GameConstants.Instance()._beamAttributes.BeamGrabJointRenderColor;
            line.endColor = GameConstants.Instance()._beamAttributes.BeamGrabJointRenderColor;
        }

        // create or update the draw points
        SetCurvedDrawPoints();

        // create or update the curved line renderer
        if (curvedLine == null) curvedLine = gameObject.AddComponent<CurvedLineRenderer>();
        else curvedLine.ManualUpdate();
    }

    private Vector3 AnchorPivotPosition()
    {
        MeshFilter _meshFilter = GetComponent<MeshFilter>();
        if (_meshFilter == null)
        {
            return Vector3.zero;
        }
        return _meshFilter.mesh.bounds.center;
    }

    private Vector3[] DrawTransforms()
    {
        return new Vector3[3] {
                _beam.transform.position,
                _beam.transform.position + _springJoint.connectedAnchor,
                transform.position + transform.TransformDirection(AnchorPivotPosition()),
            };
    }

    private void SetCurvedDrawPoints()
    {
        // transform locations to curve the line around
        Vector3[] _drawTransforms = DrawTransforms();

        for (int i = 0; i < _drawPoints.Length; i++)
        {
            // create the point if necessary
            if (_drawPoints[i] == null) _drawPoints[i] = CreateDrawPoint();

            // draw the point location
            _drawPoints[i].transform.position = _drawTransforms[i];
        }
    }


    GameObject CreateDrawPoint()
    {
        // create curved line point
        GameObject point = new GameObject("Curved Line Point", typeof(CurvedLinePoint));

        // assign the point as a child of the curved line
        point.transform.parent = transform;

        return point;
    }

    private void DestroyDrawPoints()
    {
        if (_drawPoints != null)
        {
            for (int i = _drawPoints.Length - 1; i >= 0; i--)
            {
                Destroy(_drawPoints[i]);
            }
        }
    }
    private void DestroyJointLine()
    {
        DestroyDrawPoints();
        if (curvedLine != null) Destroy(curvedLine);
        if (line != null) Destroy(line);
    }
}
