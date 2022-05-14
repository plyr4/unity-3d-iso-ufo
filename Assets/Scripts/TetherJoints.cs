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
        joint.axis = GlobalObjects.Instance()._tetherJoint.axis;
        joint.autoConfigureConnectedAnchor = GlobalObjects.Instance()._tetherJoint.autoConfigureConnectedAnchor;
        joint.secondaryAxis = GlobalObjects.Instance()._tetherJoint.secondaryAxis;

        joint.xMotion = GlobalObjects.Instance()._tetherJoint.xMotion;
        joint.yMotion = GlobalObjects.Instance()._tetherJoint.yMotion;
        joint.zMotion = GlobalObjects.Instance()._tetherJoint.zMotion;
        joint.angularXMotion = GlobalObjects.Instance()._tetherJoint.angularXMotion;
        joint.angularYMotion = GlobalObjects.Instance()._tetherJoint.angularYMotion;
        joint.angularZMotion = GlobalObjects.Instance()._tetherJoint.angularZMotion;

        joint.linearLimitSpring = GlobalObjects.Instance()._tetherJoint.linearLimitSpring;

        if (!isRetracting)
        {
            joint.linearLimit = GlobalObjects.Instance()._tetherJoint.linearLimit;
        }


        joint.angularXLimitSpring = GlobalObjects.Instance()._tetherJoint.angularXLimitSpring;
        joint.lowAngularXLimit = GlobalObjects.Instance()._tetherJoint.lowAngularXLimit;
        joint.highAngularXLimit = GlobalObjects.Instance()._tetherJoint.highAngularXLimit;
        joint.angularYZLimitSpring = GlobalObjects.Instance()._tetherJoint.angularYZLimitSpring;
        joint.angularYLimit = GlobalObjects.Instance()._tetherJoint.angularYLimit;
        joint.angularZLimit = GlobalObjects.Instance()._tetherJoint.angularZLimit;



        // angular drive
        joint.angularXDrive = GlobalObjects.Instance()._tetherJoint.angularXDrive;
        joint.angularYZDrive = GlobalObjects.Instance()._tetherJoint.angularYZDrive;

        joint.targetPosition = GlobalObjects.Instance()._tetherJoint.targetPosition;
        joint.targetVelocity = GlobalObjects.Instance()._tetherJoint.targetVelocity;

        // positional drive
        joint.xDrive = GlobalObjects.Instance()._tetherJoint.xDrive;
        joint.yDrive = GlobalObjects.Instance()._tetherJoint.yDrive;
        joint.zDrive = GlobalObjects.Instance()._tetherJoint.zDrive;

        joint.targetRotation = GlobalObjects.Instance()._tetherJoint.targetRotation;
        joint.targetAngularVelocity = GlobalObjects.Instance()._tetherJoint.targetAngularVelocity;
        joint.rotationDriveMode = GlobalObjects.Instance()._tetherJoint.rotationDriveMode;
        joint.angularXDrive = GlobalObjects.Instance()._tetherJoint.angularXDrive;
        joint.angularYZDrive = GlobalObjects.Instance()._tetherJoint.angularYZDrive;
        joint.slerpDrive = GlobalObjects.Instance()._tetherJoint.slerpDrive;
        joint.projectionMode = GlobalObjects.Instance()._tetherJoint.projectionMode;
        joint.projectionDistance = GlobalObjects.Instance()._tetherJoint.projectionDistance;
        joint.projectionAngle = GlobalObjects.Instance()._tetherJoint.projectionAngle;
        joint.configuredInWorldSpace = GlobalObjects.Instance()._tetherJoint.configuredInWorldSpace;
        joint.swapBodies = GlobalObjects.Instance()._tetherJoint.swapBodies;
        joint.breakForce = GlobalObjects.Instance()._tetherJoint.breakForce;
        joint.breakTorque = GlobalObjects.Instance()._tetherJoint.breakTorque;
        joint.enableCollision = GlobalObjects.Instance()._tetherJoint.enableCollision;
        joint.enablePreprocessing = GlobalObjects.Instance()._tetherJoint.enablePreprocessing;

        // mass
        joint.massScale = GlobalObjects.Instance()._tetherJoint.massScale;
        joint.connectedMassScale = GlobalObjects.Instance()._tetherJoint.connectedMassScale;
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

    public static void RetractJoint(TractorBeam beam, ConfigurableJoint joint)
    {
        // var _linearLimit = new SoftJointLimit();
        // _linearLimit.limit = joint.linearLimit.limit - Time.deltaTime * GlobalObjects.Instance()._beamAttributes.BeamRetractionSpeed;
        // joint.linearLimit = _linearLimit;
        SoftJointLimitSpring linearLimitSpring = new SoftJointLimitSpring();
        linearLimitSpring.spring = 3f;
        linearLimitSpring.damper = 1f;
        joint.linearLimitSpring = linearLimitSpring;
        joint.connectedAnchor = Vector3.Lerp(joint.connectedAnchor, Vector3.zero, Time.deltaTime * beam.RetractionSpeed);

    }

    public static LineRenderer DrawBeamAnchorLine(TractorBeam beam, GameObject obj, LineRenderer line)
    {
        // initialize line renderer
        if (line == null) line = obj.GetComponent<LineRenderer>();
        if (line == null) line = obj.AddComponent<LineRenderer>();

        // set default line renderer properties
        line.enabled = true;
        line.startWidth = beam.DrawJointWidth.x;
        line.endWidth = beam.DrawJointWidth.y;
        line.material = beam.DrawJointMaterial;
        line.startColor = beam.GrabSpotLightColor;
        line.endColor = beam.GrabSpotLightColor;

        // create or update the draw points
        Vector3[] _drawTransforms = BeamAnchorLineTransforms(obj, beam);
        line.positionCount = _drawTransforms.Length;
        for (int i = 0; i < _drawTransforms.Length; i++)
        {
            line.SetPosition(i, _drawTransforms[i]);
        }
        return line;
    }


    public static LineRenderer DrawObjectJointLine(TractorBeam beam, GameObject obj, LineRenderer line, ConfigurableJoint joint)
    {
        // initialize line renderer
        if (line == null) line = obj.GetComponent<LineRenderer>();
        if (line == null) line = obj.AddComponent<LineRenderer>();

        // set default line renderer properties
        line.enabled = true;
        line.startWidth = beam.DrawJointWidth.x;
        line.endWidth = beam.DrawJointWidth.y;
        line.material = beam.DrawJointMaterial;
        line.startColor = beam.GrabSpotLightColor;
        line.endColor = beam.GrabSpotLightColor;

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
