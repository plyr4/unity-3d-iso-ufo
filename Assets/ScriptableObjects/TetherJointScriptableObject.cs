using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

[System.Serializable]
[CreateAssetMenu(menuName = "Tether Joint", fileName = "New Tether Joint")]
public class TetherJointScriptableObject : ScriptableObject
{
    [Header("Configurable Joint")]
    [SerializeField]
    public Rigidbody _connectedBody;
    [SerializeField]
    public ArticulationBody _connectedArticulationBody;
    [SerializeField]
    public Vector3 _anchor = Vector3.zero;
    [SerializeField]
    public Vector3 _axis = new Vector3(1, 0, 0);
    [SerializeField]
    public bool _autoConfigureConnectedAnchor = false;
    [SerializeField]
    public Vector3 _connectedAnchor = Vector3.zero;
    [SerializeField]
    public Vector3 _secondaryAxis = new Vector3(0, 1, 0);
    [SerializeField]
    public ConfigurableJointMotion _xMotion = ConfigurableJointMotion.Limited;
    [SerializeField]
    public ConfigurableJointMotion _yMotion = ConfigurableJointMotion.Limited;
    [SerializeField]
    public ConfigurableJointMotion _zMotion = ConfigurableJointMotion.Limited;
    [SerializeField]
    public ConfigurableJointMotion _angularXMotion = ConfigurableJointMotion.Free;
    [SerializeField]
    public ConfigurableJointMotion _angularYMotion = ConfigurableJointMotion.Free;
    [SerializeField]
    public ConfigurableJointMotion _angularZMotion = ConfigurableJointMotion.Free;
    [SerializeField]
    public SerializedSoftJointLimitSpring _linearLimitSpring;
    [SerializeField]
    public SerializedSoftJointLimit _linearLimit;
    [SerializeField]
    public SerializedSoftJointLimitSpring _angularXLimitSpring;
    [SerializeField]
    public SerializedSoftJointLimit _lowAngularXLimit;
    [SerializeField]
    public SerializedSoftJointLimit _highAngularXLimit;
    [SerializeField]
    public SerializedSoftJointLimitSpring _angularYZLimitSpring;
    [SerializeField]
    public SerializedSoftJointLimit _angularYLimit;
    [SerializeField]
    public SerializedSoftJointLimit _angularZLimit;
    [SerializeField]
    public Vector3 _targetPosition;
    [SerializeField]
    public Vector3 _targetVelocity;
    [SerializeField]
    public SerializedJointDrive _xDrive;
    [SerializeField]
    public SerializedJointDrive _yDrive;
    [SerializeField]
    public SerializedJointDrive _zDrive;
    [SerializeField]
    public Quaternion _targetRotation = Quaternion.identity;
    [SerializeField]
    public Vector3 _targetAngularVelocity;
    [SerializeField]
    public RotationDriveMode _rotationDriveMode = RotationDriveMode.XYAndZ;
    [SerializeField]
    public SerializedJointDrive _angularXDrive;
    [SerializeField]
    public SerializedJointDrive _angularYZDrive;
    [SerializeField]
    public SerializedJointDrive _slerpDrive;
    [SerializeField]
    public JointProjectionMode _projectionMode = JointProjectionMode.PositionAndRotation;
    [SerializeField]
    public float _projectionDistance = 0.01f;
    [SerializeField]
    public float _projectionAngle = 1f;
    [SerializeField]
    public bool _configuredInWorldSpace = true;
    [SerializeField]
    public bool _swapBodies = false;
    [SerializeField]
    public float _breakForce = float.PositiveInfinity;
    [SerializeField]
    public float _breakTorque = float.PositiveInfinity;
    [SerializeField]
    public bool _enableCollision = false;
    [SerializeField]
    public bool _enablePreprocessing = true;
    [SerializeField]
    public float _massScale = 10f;
    [SerializeField]
    public float _connectedMassScale = 1f;


    public ConfigurableJoint CreateTether(GameObject beamableObject, TractorBeam beam)
    {
        // attempt to fetch a tether
        ConfigurableJoint tetherJoint = beamableObject.GetComponent<ConfigurableJoint>();

        // add the tether if necessary
        if (tetherJoint == null) tetherJoint = beamableObject.AddComponent<ConfigurableJoint>();

        // apply the constant properties
        ApplyTetherJointProperties(tetherJoint);

        // apply the dynamic properties
        UpdateDynamicTetherJointProperties(tetherJoint, beam, false);

        // return the joint
        return tetherJoint;
    }


    public void ApplyTetherJointProperties(ConfigurableJoint tetherJoint)
    {
        if (tetherJoint == null) return;

        // Rigidbody
        // tetherJoint.connectedBody = _connectedBody;

        // Articulation Body
        tetherJoint.connectedArticulationBody = _connectedArticulationBody;

        // Vector3
        // tetherJoint.anchor = _anchor;
        tetherJoint.axis = _axis;

        // bool
        tetherJoint.autoConfigureConnectedAnchor = _autoConfigureConnectedAnchor;

        // Vector3
        // tetherJoint.connectedAnchor = _connectedAnchor;
        tetherJoint.secondaryAxis = _secondaryAxis;

        // ConfigurableJointMotion
        tetherJoint.xMotion = _xMotion;
        tetherJoint.yMotion = _yMotion;
        tetherJoint.zMotion = _zMotion;

        // ConfigurableJointMotion
        tetherJoint.angularXMotion = _angularXMotion;
        tetherJoint.angularYMotion = _angularYMotion;
        tetherJoint.angularZMotion = _angularZMotion;

        // SoftJointLimitSpring
        tetherJoint.linearLimitSpring = _linearLimitSpring;

        // SoftJointLimit
        tetherJoint.linearLimit = _linearLimit;

        // SoftJointLimitSpring
        tetherJoint.angularXLimitSpring = _angularXLimitSpring;

        // SoftJointLimit
        tetherJoint.lowAngularXLimit = _lowAngularXLimit;
        tetherJoint.highAngularXLimit = _highAngularXLimit;

        // SoftJointLimitSpring
        tetherJoint.angularYZLimitSpring = _angularYZLimitSpring;

        // SoftJointLimit
        tetherJoint.angularYLimit = _angularYLimit;
        tetherJoint.angularZLimit = _angularZLimit;

        // angular drive
        tetherJoint.angularXDrive = _angularXDrive;
        tetherJoint.angularYZDrive = _angularYZDrive;

        // Vector3 
        tetherJoint.targetPosition = _targetPosition;
        tetherJoint.targetVelocity = _targetVelocity;

        // JointDrive
        tetherJoint.xDrive = _xDrive;
        tetherJoint.yDrive = _yDrive;
        tetherJoint.zDrive = _zDrive;

        // Quaternion
        tetherJoint.targetRotation = _targetRotation;

        // Vector3 
        tetherJoint.targetAngularVelocity = _targetAngularVelocity;

        // RotationDriveMode
        tetherJoint.rotationDriveMode = _rotationDriveMode;

        // JointDrive
        tetherJoint.angularXDrive = _angularXDrive;
        tetherJoint.angularYZDrive = _angularYZDrive;
        tetherJoint.slerpDrive = _slerpDrive;

        // JointProjectionMode
        tetherJoint.projectionMode = _projectionMode;

        // float
        tetherJoint.projectionDistance = _projectionDistance;
        tetherJoint.projectionAngle = _projectionAngle;

        // bool
        tetherJoint.configuredInWorldSpace = _configuredInWorldSpace;
        tetherJoint.swapBodies = _swapBodies;

        // float
        tetherJoint.breakForce = _breakForce;
        tetherJoint.breakTorque = _breakTorque;

        // bool
        tetherJoint.enableCollision = _enableCollision;
        tetherJoint.enablePreprocessing = _enablePreprocessing;

        // float
        tetherJoint.massScale = _massScale;
        tetherJoint.connectedMassScale = _connectedMassScale;
    }


    public void UpdateDynamicTetherJointProperties(ConfigurableJoint tetherJoint, TractorBeam beam, bool retract)
    {
        // set the object anchor to the mesh center
        Vector3 _meshCenter = MeshFilters.GetAnchorPivotPosition(tetherJoint.gameObject.GetComponent<MeshFilter>());
        tetherJoint.anchor = _meshCenter;

        // set the connected body to the beam anchor
        tetherJoint.connectedBody = beam.GetTetherAnchor();

        if (retract)
        {
            // retract the tether
            RetractJoint(tetherJoint, beam);
        }
        else
        {
            // set connected anchor to the beam collection depth
            tetherJoint.connectedAnchor = -Vector3.up * beam.AnchorDepth();
        }

        // set the mass scale to be proportional between the anchor and the object
        tetherJoint.massScale = tetherJoint.connectedBody.mass / tetherJoint.gameObject.GetComponent<Rigidbody>().mass;
        tetherJoint.connectedMassScale = 1f;
    }

    public static void RetractJoint(ConfigurableJoint tetherJoint, TractorBeam beam)
    {
        // assign linear limit
        SoftJointLimitSpring linearLimitSpring = new SoftJointLimitSpring();
        linearLimitSpring.spring = 3f;
        linearLimitSpring.damper = 1f;
        tetherJoint.linearLimitSpring = linearLimitSpring;

        // retract the connected anchor over time
        tetherJoint.connectedAnchor = Vector3.Lerp(tetherJoint.connectedAnchor, Vector3.zero, Time.deltaTime * beam.RetractionSpeed);
    }
}
