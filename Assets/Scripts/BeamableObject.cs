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

    [SerializeField]
    private bool isGrabbed;
    private bool hasSpun;
    private bool isOutlined;
    private bool isTethered;

    private TractorBeam grabbedBy;

    private GameObject _tether;

    private float MaxVelocity = 2f;

    void Start()
    {
        if (_rb == null) _rb = GetComponent<Rigidbody>();

        // physics
        _rb.isKinematic = true;
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

        // variables
        hasSpun = false;
        isGrabbed = false;
        isOutlined = false;
    }

    void Update()
    {
        if (isGrabbed)
        {
            // TODO: joints do not like this
            // _rb.useGravity = false;
            _rb.isKinematic = false;



            // TODO: joints do not like this
            //  this should stop their movement but not their rotation

            _rb.velocity = new Vector3(Mathf.Clamp(_rb.velocity.x, -MaxVelocity, MaxVelocity), Mathf.Clamp(_rb.velocity.y, -MaxVelocity, MaxVelocity), Mathf.Clamp(_rb.velocity.z, -MaxVelocity, MaxVelocity));

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
        }
        else
        {
            _rb.useGravity = true;
            // undo outline
            if (isOutlined)
            {
                isOutlined = false;
                RevertObjectOutline();
            }
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

    public bool IsGrabbed()
    {
        return isGrabbed;
    }
    public void SetGrabbed(bool grabbed, TractorBeam beam)
    {
        isGrabbed = grabbed;
        grabbedBy = beam;
        if (!isTethered)
        {
            isTethered = true;

            SpringJoint _springJoint = gameObject.GetComponent<SpringJoint>();
            if (_springJoint == null) _springJoint = gameObject.AddComponent<SpringJoint>();
            _springJoint.spring = 100;
            _springJoint.damper = 100;
            _springJoint.anchor = Vector3.zero;
            _springJoint.connectedBody = beam._tetherAnchor;
            _springJoint.autoConfigureConnectedAnchor = false;
            _springJoint.connectedAnchor = Vector3.zero;
        }
    }

    public void CreateTether()
    {

    }

    public void ApplyRandomSpin()
    {
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
}
