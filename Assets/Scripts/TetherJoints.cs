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
        UpdateConstantJointProperties(joint, false);
        UpdateJointProperties(joint, beam, false);
        return joint;
    }


    public static void UpdateConstantJointProperties(ConfigurableJoint joint, bool isRetracting)
    {
        if (joint == null) return;
        joint.axis = GameConstants.Instance()._beamAttributes.BeamableObjectJoint.axis;
        joint.autoConfigureConnectedAnchor = GameConstants.Instance()._beamAttributes.BeamableObjectJoint.autoConfigureConnectedAnchor;
        joint.secondaryAxis = GameConstants.Instance()._beamAttributes.BeamableObjectJoint.secondaryAxis;

        joint.xMotion = GameConstants.Instance()._beamAttributes.BeamableObjectJoint.xMotion;
        joint.yMotion = GameConstants.Instance()._beamAttributes.BeamableObjectJoint.yMotion;
        joint.zMotion = GameConstants.Instance()._beamAttributes.BeamableObjectJoint.zMotion;
        joint.angularXMotion = GameConstants.Instance()._beamAttributes.BeamableObjectJoint.angularXMotion;
        joint.angularYMotion = GameConstants.Instance()._beamAttributes.BeamableObjectJoint.angularYMotion;
        joint.angularZMotion = GameConstants.Instance()._beamAttributes.BeamableObjectJoint.angularZMotion;

        joint.linearLimitSpring = GameConstants.Instance()._beamAttributes.BeamableObjectJoint.linearLimitSpring;

        if (!isRetracting)
        {
            joint.linearLimit = GameConstants.Instance()._beamAttributes.BeamableObjectJoint.linearLimit;
        }


        joint.angularXLimitSpring = GameConstants.Instance()._beamAttributes.BeamableObjectJoint.angularXLimitSpring;
        joint.lowAngularXLimit = GameConstants.Instance()._beamAttributes.BeamableObjectJoint.lowAngularXLimit;
        joint.highAngularXLimit = GameConstants.Instance()._beamAttributes.BeamableObjectJoint.highAngularXLimit;
        joint.angularYZLimitSpring = GameConstants.Instance()._beamAttributes.BeamableObjectJoint.angularYZLimitSpring;
        joint.angularYLimit = GameConstants.Instance()._beamAttributes.BeamableObjectJoint.angularYLimit;
        joint.angularZLimit = GameConstants.Instance()._beamAttributes.BeamableObjectJoint.angularZLimit;



        // angular drive
        joint.angularXDrive = GameConstants.Instance()._beamAttributes.BeamableObjectJoint.angularXDrive;
        joint.angularYZDrive = GameConstants.Instance()._beamAttributes.BeamableObjectJoint.angularYZDrive;

        joint.targetPosition = GameConstants.Instance()._beamAttributes.BeamableObjectJoint.targetPosition;
        joint.targetVelocity = GameConstants.Instance()._beamAttributes.BeamableObjectJoint.targetVelocity;

        // positional drive
        joint.xDrive = GameConstants.Instance()._beamAttributes.BeamableObjectJoint.xDrive;
        joint.yDrive = GameConstants.Instance()._beamAttributes.BeamableObjectJoint.yDrive;
        joint.zDrive = GameConstants.Instance()._beamAttributes.BeamableObjectJoint.zDrive;

        joint.targetRotation = GameConstants.Instance()._beamAttributes.BeamableObjectJoint.targetRotation;
        joint.targetAngularVelocity = GameConstants.Instance()._beamAttributes.BeamableObjectJoint.targetAngularVelocity;
        joint.rotationDriveMode = GameConstants.Instance()._beamAttributes.BeamableObjectJoint.rotationDriveMode;
        joint.angularXDrive = GameConstants.Instance()._beamAttributes.BeamableObjectJoint.angularXDrive;
        joint.angularYZDrive = GameConstants.Instance()._beamAttributes.BeamableObjectJoint.angularYZDrive;
        joint.slerpDrive = GameConstants.Instance()._beamAttributes.BeamableObjectJoint.slerpDrive;
        joint.projectionMode = GameConstants.Instance()._beamAttributes.BeamableObjectJoint.projectionMode;
        joint.projectionDistance = GameConstants.Instance()._beamAttributes.BeamableObjectJoint.projectionDistance;
        joint.projectionAngle = GameConstants.Instance()._beamAttributes.BeamableObjectJoint.projectionAngle;
        joint.configuredInWorldSpace = GameConstants.Instance()._beamAttributes.BeamableObjectJoint.configuredInWorldSpace;
        joint.swapBodies = GameConstants.Instance()._beamAttributes.BeamableObjectJoint.swapBodies;
        joint.breakForce = GameConstants.Instance()._beamAttributes.BeamableObjectJoint.breakForce;
        joint.breakTorque = GameConstants.Instance()._beamAttributes.BeamableObjectJoint.breakTorque;
        joint.enableCollision = GameConstants.Instance()._beamAttributes.BeamableObjectJoint.enableCollision;
        joint.enablePreprocessing = GameConstants.Instance()._beamAttributes.BeamableObjectJoint.enablePreprocessing;

        // mass
        joint.massScale = GameConstants.Instance()._beamAttributes.BeamableObjectJoint.massScale;
        joint.connectedMassScale = GameConstants.Instance()._beamAttributes.BeamableObjectJoint.connectedMassScale;
    }

    public static void UpdateJointProperties(ConfigurableJoint joint, TractorBeam beam, bool isRetracting)
    {
        // set the object anchor to the mesh center
        Vector3 _meshCenter = MeshFilters.GetAnchorPivotPosition(joint.gameObject.GetComponent<MeshFilter>());
        joint.anchor = _meshCenter;

        // set connected anchor to the beam collection depth
        if (!isRetracting) joint.connectedAnchor = -Vector3.up * beam.AnchorDepth();

        // set the mass scale to be proportional between the anchor and the object
        joint.massScale = joint.connectedBody.mass / joint.gameObject.GetComponent<Rigidbody>().mass;
        joint.connectedMassScale = 1f;
    }

    public static void RetractJoint(ConfigurableJoint joint)
    {
        // var _linearLimit = new SoftJointLimit();
        // _linearLimit.limit = joint.linearLimit.limit - Time.deltaTime * GameConstants.Instance()._beamAttributes.BeamRetractionSpeed;
        // joint.linearLimit = _linearLimit;
        SoftJointLimitSpring linearLimitSpring = new SoftJointLimitSpring();
        linearLimitSpring.spring = 2f;
        linearLimitSpring.damper = 1f;
        joint.linearLimitSpring = linearLimitSpring;
        joint.connectedAnchor = Vector3.Lerp(joint.connectedAnchor, Vector3.zero, Time.deltaTime * GameConstants.Instance()._beamAttributes.BeamRetractionSpeed);

    }

    public static LineRenderer DrawBeamAnchorLine(TractorBeam beam, GameObject obj, LineRenderer line)
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
        Vector3[] _drawTransforms = BeamAnchorLineTransforms(obj, beam);
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


    public static Vector3[] BeamAnchorLineTransforms(GameObject obj, TractorBeam beam)
    {
        return new Vector3[2] {
                obj.transform.position,
                obj.transform.position + (-Vector3.up * beam.AnchorDepth()),
            };
    }

    public static Vector3[] JointObjectLineTransforms(GameObject obj, ConfigurableJoint joint)
    {
        return new Vector3[2] {
                obj.transform.position + obj.transform.TransformDirection(joint.anchor),
                joint.connectedBody.transform.position + joint.connectedAnchor,
            };
    }

    public static void ClearJointLine(LineRenderer line)
    {
        if (line == null) return;
        line.enabled = false;
    }
}
