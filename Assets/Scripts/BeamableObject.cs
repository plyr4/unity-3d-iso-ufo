using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(ObjectRenderSnapable))]
[DisallowMultipleComponent]
public class BeamableObject : MonoBehaviour
{
    private Rigidbody _rb;

    [SerializeField]
    private float RandomRotationTorque = 10f;

    [SerializeField]
    private bool TrackBeamableObjectJointChanges = true;

    [SerializeField]
    private bool isTethered;

    [SerializeField]
    private TractorBeam _beam;

    private GameObject _tether;
    public ConfigurableJoint _springJoint;

    LineRenderer line;

    private bool retractJoint;
    private bool hasSpun;


    public void Start()
    {
        return;
        if (_rb == null) _rb = GetComponent<Rigidbody>();
        MeshColliders.SetMeshCollidersToConvex(gameObject);
        gameObject.layer = 8;
    }

    private void Update()
    {
        return;
        if (Tethered())
        {
            _rb.mass = 0.5f;
            if (TrackBeamableObjectJointChanges) _springJoint = TetherJoints.CreateTether(gameObject, null);

            TetherJoints.UpdateJointProperties(_springJoint, retractJoint, _beam.AnchorDepth());

            // apply random torque once
            if (!hasSpun)
            {
                _rb.AddTorque(RandomSpin.GetRandomSpin() * RandomRotationTorque);
                hasSpun = true;
            }

            if (retractJoint)
            {
                TetherJoints.RetractJoint(_springJoint);
            }
            else
            {
                TetherJoints.SetLinearLimit(_springJoint, GameConstants.Instance()._beamAttributes.BeamGrabLinearLimit);
            }
        }
        else
        {
            _rb.mass = 1f;
            _rb.useGravity = true;
        }
        DrawTether();
    }


    private void DrawTether()
    {
        if (Tethered())
        {
            // TODO: reimplement this - performance bottleneck
            //
            // if (GameConstants.Instance()._beamAttributes.BeamDrawJoints) line = TetherJoints.DrawJointLine(gameObject, line, _springJoint);
            // else TetherJoints.ClearJointLine(line);
        }
        else
        {
            TetherJoints.ClearJointLine(line);
        }
    }

    public void Reset()
    {
        return;
        if (_rb == null) _rb = GetComponent<Rigidbody>();

        // physics
        _rb.isKinematic = true;
        MeshColliders.SetMeshCollidersToConvex(gameObject);
        gameObject.layer = 8;
    }

    public bool Tethered()
    {
        return isTethered;
    }

    public void Tether(GameObject obj, TractorBeam beam)
    {
        return;
        _beam = beam;
        _rb.isKinematic = false;    
        MeshColliders.SetMeshCollidersToConvex(gameObject);
        _springJoint = TetherJoints.CreateTether(gameObject, beam);
        TetherJoints.UpdateJointProperties(_springJoint, retractJoint, beam.AnchorDepth());

        // start tether updates when ready
        isTethered = true;
    }

    public void Untether()
    {
                return;

        // untether to stop updates
        isTethered = false;

        _beam = null;
        hasSpun = false;
        ConfigurableJoint joint = _springJoint;
        _springJoint = null;
        _rb.velocity = _rb.velocity * 0.5f;
        Destroy(joint);
    }

    public void SetRetractJoint() {
                return;

        retractJoint = true;
    }
}
