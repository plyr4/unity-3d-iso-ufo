using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class TetherJoints
{
    public static ConfigurableJoint CreateTether(GameObject beamableObject, TractorBeam beam)
    {
        ConfigurableJoint joint = beamableObject.GetComponent<ConfigurableJoint>();
        if (joint == null) joint = beamableObject.AddComponent<ConfigurableJoint>();
        if (beam != null) joint.connectedBody = beam.GetTetherAnchor();
        joint.anchor = beamableObject.transform.TransformDirection(MeshFilters.GetAnchorPivotPosition(beamableObject.GetComponent<MeshFilter>()));

        UpdateConstantJointProperties(joint);
        UpdateJointProperties(joint, false, beam.AnchorDepth());
        return joint;
    }


    public static void UpdateConstantJointProperties(ConfigurableJoint joint)
    {
        if (joint == null) return;

        joint.xMotion = GameConstants.Instance()._beamAttributes.BeamableObjectJoint.xMotion;
        joint.yMotion = GameConstants.Instance()._beamAttributes.BeamableObjectJoint.yMotion;
        joint.zMotion = GameConstants.Instance()._beamAttributes.BeamableObjectJoint.zMotion;
        joint.angularXMotion = GameConstants.Instance()._beamAttributes.BeamableObjectJoint.angularXMotion;
        joint.angularYMotion = GameConstants.Instance()._beamAttributes.BeamableObjectJoint.angularYMotion;
        joint.angularZMotion = GameConstants.Instance()._beamAttributes.BeamableObjectJoint.angularZMotion;

        joint.autoConfigureConnectedAnchor = GameConstants.Instance()._beamAttributes.BeamableObjectJoint.autoConfigureConnectedAnchor;

        // positional drive
        joint.xDrive = GameConstants.Instance()._beamAttributes.BeamableObjectJoint.xDrive;
        joint.yDrive = GameConstants.Instance()._beamAttributes.BeamableObjectJoint.yDrive;
        joint.zDrive = GameConstants.Instance()._beamAttributes.BeamableObjectJoint.zDrive;

        // angular drive
        joint.angularXDrive = GameConstants.Instance()._beamAttributes.BeamableObjectJoint.angularXDrive;
        joint.angularYZDrive = GameConstants.Instance()._beamAttributes.BeamableObjectJoint.angularYZDrive;

        // linear limit
        joint.linearLimitSpring = GameConstants.Instance()._beamAttributes.BeamableObjectJoint.linearLimitSpring;

        // mass
        joint.massScale = GameConstants.Instance()._beamAttributes.BeamableObjectJoint.massScale;
        joint.connectedMassScale = GameConstants.Instance()._beamAttributes.BeamableObjectJoint.connectedMassScale;
    }

    public static void UpdateJointProperties(ConfigurableJoint joint, bool isRetracting, float depth)
    {
        if (!isRetracting)
        {
            var _linearLimit = new SoftJointLimit();
            _linearLimit.limit = GameConstants.Instance()._beamAttributes.BeamGrabLinearLimit;
            joint.linearLimit = _linearLimit;
        }

        joint.anchor = joint.gameObject.transform.TransformDirection(MeshFilters.GetAnchorPivotPosition(joint.gameObject.GetComponent<MeshFilter>()));

        Debug.Log("updateing depth to " + depth);
        joint.connectedAnchor = -Vector3.up * depth;
    }

    public static void SetLinearLimit(ConfigurableJoint joint, float limit)
    {
        var _linearLimit = new SoftJointLimit();
        _linearLimit.limit = limit;
        joint.linearLimit = _linearLimit;
    }

    public static void RetractJoint(ConfigurableJoint joint)
    {
        joint.connectedAnchor = Vector3.zero;
        var _linearLimit = new SoftJointLimit();
        _linearLimit.limit = joint.linearLimit.limit - Time.deltaTime * GameConstants.Instance()._beamAttributes.BeamRetractionSpeed;
        joint.linearLimit = _linearLimit;
    }

    public static LineRenderer DrawBeamAnchorLine(GameObject obj, LineRenderer line, float depth)
    {
        // initialize line renderer
        if (line == null) line = obj.GetComponent<LineRenderer>();
        if (line == null) line = obj.AddComponent<LineRenderer>();

        // set default line renderer properties
        line.enabled = true;
        line.startWidth = GameConstants.Instance()._beamAttributes.BeamGrabJointRenderStartWidth;
        line.endWidth = GameConstants.Instance()._beamAttributes.BeamGrabJointRenderEndWidth;
        line.material = GameConstants.Instance()._beamAttributes.BeamGrabJointRenderMaterial;
        line.startColor = GameConstants.Instance()._beamAttributes.BeamGrabJointRenderColor;
        line.endColor = GameConstants.Instance()._beamAttributes.BeamGrabJointRenderColor;

        // create or update the draw points
        Vector3[] _drawTransforms = BeamAnchorLineTransforms(obj, depth);
        line.positionCount = _drawTransforms.Length;
        for (int i = 0; i < _drawTransforms.Length; i++)
        {
            line.SetPosition(i, _drawTransforms[i]);
        }
        return line;
    }


    public static LineRenderer DrawObjectJointLine(GameObject obj, LineRenderer line, ConfigurableJoint joint)
    {
        // initialize line renderer
        if (line == null) line = obj.GetComponent<LineRenderer>();
        if (line == null) line = obj.AddComponent<LineRenderer>();

        // set default line renderer properties
        line.enabled = true;
        line.startWidth = GameConstants.Instance()._beamAttributes.BeamGrabJointRenderEndWidth;
        line.endWidth = GameConstants.Instance()._beamAttributes.BeamGrabJointRenderStartWidth;
        line.material = GameConstants.Instance()._beamAttributes.BeamGrabJointRenderMaterial;
        line.startColor = GameConstants.Instance()._beamAttributes.BeamGrabJointRenderColor;
        line.endColor = GameConstants.Instance()._beamAttributes.BeamGrabJointRenderColor;

        // create or update the draw points
        Vector3[] _drawTransforms = JointObjectLineTransforms(obj, joint);
        line.positionCount = _drawTransforms.Length;
        for (int i = 0; i < _drawTransforms.Length; i++)
        {
            line.SetPosition(i, _drawTransforms[i]);
        }
        return line;
    }


    public static Vector3[] BeamAnchorLineTransforms(GameObject obj, float depth)
    {
        return new Vector3[2] {
                obj.transform.position,
                obj.transform.position-Vector3.up * depth,
            };
    }

    public static Vector3[] JointObjectLineTransforms(GameObject obj, ConfigurableJoint joint)
    {
        return new Vector3[2] {
                obj.transform.position + obj.transform.TransformDirection(MeshFilters.GetAnchorPivotPosition(obj.GetComponent<MeshFilter>())),
                joint.connectedBody.transform.position + joint.connectedAnchor,
            };
    }

    public static void ClearJointLine(LineRenderer line)
    {
        if (line == null) return;
        line.enabled = false;
    }
}
