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
    private int GrabbedOutlineID = 69;
    private bool hasSpun;
    private bool isOutlined;
    [SerializeField]
    private bool isTethered;

    [SerializeField]
    private GameObject grabbedBy;

    private GameObject _tether;
    private ConfigurableJoint _springJoint;

    private float MaxVelocity = 2f;

    LineRenderer line;
    CurvedLineRenderer curvedLine;

    private bool retractJoint;

    [SerializeField]
    private bool TrackBeamableObjectJointChanges = true;
    GameObject curvedLinePoint0, curvedLinePoint1, curvedLinePoint2;
    void Start()
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

    void Update()
    {
        if (Tethered())
        {
            // TODO: joints do not like this
            // _rb.useGravity = false;
            _rb.mass = 0.5f;
            if (TrackBeamableObjectJointChanges) CreateTether(null);

            // debug joint outlines 
            if (GameConstants.Instance()._beamAttributes.BeamDrawJoints) DrawJointLine();
            else if (line != null) ClearJointLine();
            // _springJoint.connectedAnchor = Vector3.zero - Vector3.up * AnchorDepth();
            SetDynamicJointAttributes();

            // TODO: joints do not like this
            //  this should stop their movement but not their rotation

            // _rb.velocity = new Vector3(Mathf.Clamp(_rb.velocity.x, -MaxVelocity, MaxVelocity), Mathf.Clamp(_rb.velocity.y, -MaxVelocity, MaxVelocity), Mathf.Clamp(_rb.velocity.z, -MaxVelocity, MaxVelocity));

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


    void Reset()
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
        grabbedBy = beam.gameObject;
        CreateTether(beam);
        SetDynamicJointAttributes();
    }

    public void Untether()
    {
        isTethered = false;
        grabbedBy = null;
        ConfigurableJoint joint = _springJoint;
        _springJoint = null;
        _rb.velocity = Vector3.zero;
        Destroy(joint);
    }


    public void LockLinearLimit()
    {
        var _linearLimit = new SoftJointLimit();
        _linearLimit.limit = GameConstants.Instance()._beamAttributes.BeamGrabLinearLimit;
        _springJoint.linearLimit = _linearLimit;
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

    public float AnchorDepth()
    {
        return grabbedBy.GetComponent<TractorBeam>()._beamDepth * GameConstants.Instance()._beamAttributes.BeamGrabAnchorDepthCoefficient;
    }

    public void CreateTether(TractorBeam beam)
    {
        if (_springJoint == null) _springJoint = gameObject.AddComponent<ConfigurableJoint>();
        if (beam != null) _springJoint.connectedBody = beam._tetherAnchor;
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

    public void SetDynamicJointAttributes()
    {
        if (!retractJoint)
        {
            SoftJointLimit _linearLimit = new SoftJointLimit();
            _linearLimit.limit = GameConstants.Instance()._beamAttributes.BeamGrabLinearLimit;
            _springJoint.linearLimit = _linearLimit;
        }
        _springJoint.connectedAnchor = Vector3.zero - Vector3.up * AnchorDepth();
    }

    public ConfigurableJoint CreateTetherBACKUP()
    {
        if (_springJoint == null) _springJoint = gameObject.AddComponent<ConfigurableJoint>();

        // var xMotion = new 
        _springJoint.xMotion = ConfigurableJointMotion.Limited;
        _springJoint.yMotion = ConfigurableJointMotion.Limited;
        _springJoint.zMotion = ConfigurableJointMotion.Limited;
        _springJoint.angularXMotion = ConfigurableJointMotion.Limited;
        _springJoint.angularYMotion = ConfigurableJointMotion.Limited;
        _springJoint.angularZMotion = ConfigurableJointMotion.Limited;

        _springJoint.anchor = Vector3.zero;
        // _springJoint.connectedBody = grabbedBy.GetComponentInChildren<Rigidbody>();
        _springJoint.autoConfigureConnectedAnchor = false;


        var _limitSpring = new SoftJointLimitSpring();
        _limitSpring.spring = 100;
        _limitSpring.damper = 100;
        _springJoint.linearLimitSpring = _limitSpring;



        var xDrive = new JointDrive();
        xDrive.positionSpring = 200;
        xDrive.positionDamper = 200;
        xDrive.maximumForce = float.MaxValue;
        _springJoint.xDrive = xDrive;


        var zDrive = new JointDrive();
        zDrive.positionSpring = 200;
        zDrive.positionDamper = 200;
        zDrive.maximumForce = float.MaxValue;
        _springJoint.zDrive = zDrive;


        var angularXDrive = new JointDrive();
        angularXDrive.positionSpring = 200;
        angularXDrive.positionDamper = 200;
        angularXDrive.maximumForce = float.MaxValue;
        _springJoint.angularXDrive = angularXDrive;

        var angularYZDrive = new JointDrive();
        angularYZDrive.positionSpring = 200;
        angularYZDrive.positionDamper = 200;
        angularYZDrive.maximumForce = float.MaxValue;
        _springJoint.angularYZDrive = angularYZDrive;

        // dynamic properties
        var _linearLimit = new SoftJointLimit();
        _springJoint.connectedAnchor = Vector3.zero - Vector3.up * AnchorDepth();
        _linearLimit.limit = GameConstants.Instance()._beamAttributes.BeamGrabLinearLimit;
        _springJoint.linearLimit = _linearLimit;
        return _springJoint;
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

    void ApplyObjectOutline()
    {
        for (int i = 0; i < _pixelizerMaterials.Count; i++) _pixelizerMaterials[i].ApplyOutline(GameConstants.Instance()._beamAttributes.BeamGrabOutlineColor, 69);
    }

    void RevertObjectOutline()
    {
        for (int i = 0; i < _pixelizerMaterials.Count; i++) _pixelizerMaterials[i].RevertOutline();
    }

    void SetMeshCollidersToConvex()
    {
        MeshCollider[] _meshColliders = GetComponentsInChildren<MeshCollider>();
        for (int i = 0; i < _meshColliders.Length; i++)
        {
            _meshColliders[i].convex = true;
        }
    }

    void DrawJointLine()
    {
        if (line == null)
        {
            line = this.gameObject.AddComponent<LineRenderer>();
            curvedLine = this.gameObject.AddComponent<CurvedLineRenderer>();
        }
        line.startWidth = GameConstants.Instance()._beamAttributes.BeamGrabJointRenderWidth;
        line.endWidth = GameConstants.Instance()._beamAttributes.BeamGrabJointRenderWidth;
        line.material = GameConstants.Instance()._beamAttributes.BeamGrabJointRenderMaterial;
        line.startColor = GameConstants.Instance()._beamAttributes.BeamGrabJointRenderColor;
        line.endColor = GameConstants.Instance()._beamAttributes.BeamGrabJointRenderColor;

        if (curvedLinePoint0 == null)
        {
            curvedLinePoint0 = new GameObject("Curved Line Point", typeof(CurvedLinePoint));
            curvedLinePoint0.transform.parent = this.gameObject.transform;
        }
        curvedLinePoint0.transform.position = grabbedBy.gameObject.transform.position;

        if (curvedLinePoint1 == null)
        {
            curvedLinePoint1 = new GameObject("Curved Line Point", typeof(CurvedLinePoint));
            curvedLinePoint1.transform.parent = this.gameObject.transform;
        }
        Vector3 mm =  new Vector3(_springJoint.connectedAnchor.x, _springJoint.connectedAnchor.y , _springJoint.connectedAnchor.z);
        curvedLinePoint1.transform.position = grabbedBy.gameObject.transform.position + mm;


        if (curvedLinePoint2 == null)
        {
            curvedLinePoint2 = new GameObject("Curved Line Point", typeof(CurvedLinePoint));
            curvedLinePoint2.transform.parent = this.gameObject.transform;
        }
        curvedLinePoint2.transform.position = transform.position;

        // line.positionCount = 3;
        // line.SetPosition(0, grabbedBy.gameObject.transform.position);
        // line.SetPosition(1, grabbedBy.gameObject.transform.position + _springJoint.connectedAnchor);
        // line.SetPosition(2, transform.position);
    }

    void ClearJointLine()
    {
        if (line == null) return;
        line.endWidth = 0f;
        line.startWidth = 0f;
        if (curvedLinePoint0 != null) Destroy(curvedLinePoint0);
        if (curvedLinePoint1 != null) Destroy(curvedLinePoint1);
        if (curvedLinePoint2 != null) Destroy(curvedLinePoint2);
    }
}
