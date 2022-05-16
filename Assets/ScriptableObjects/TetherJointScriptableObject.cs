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

    [Header("Retraction Settings")]
    [SerializeField]
    private float RetractObjectShrinkSpeed = 3f;
    [SerializeField]
    public SerializedSoftJointLimitSpring _retractionLinearLimitSpring;
    [SerializeField]
    public SerializedSoftJointLimit _retractionLinearLimit;
    [SerializeField]
    public SerializedJointDrive _retractionXDrive;
    [SerializeField]
    public SerializedJointDrive _retractionYDrive;
    [SerializeField]
    public SerializedJointDrive _retractionZDrive;

    public void CreateTether(TractorBeam beam, Beamable beamable)
    {
        // attempt to fetch a tether
        beamable._tether = beamable._gameObject.GetComponent<ConfigurableJoint>();

        // add the tether if necessary
        if (beamable._tether == null) beamable._tether = beamable._gameObject.AddComponent<ConfigurableJoint>();

        if (beam != null) beamable._tether.connectedBody = beam.GetTetherAnchor();


        // apply the constant properties
        ApplyTetherJointProperties(beamable._tether);

        // apply the dynamic properties
        UpdateDynamicTetherJointProperties(beam, beamable);
    }


    public void ApplyTetherJointProperties(ConfigurableJoint tetherJoint)
    {
        if (tetherJoint == null) return;

        // Vector3
        tetherJoint.axis = _axis;

        // bool
        tetherJoint.autoConfigureConnectedAnchor = _autoConfigureConnectedAnchor;

        // Vector3
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


    public void UpdateDynamicTetherJointProperties(TractorBeam beam, Beamable beamable)
    {
        // set the object anchor to the mesh center
        Vector3 _meshCenter = MeshFilters.GetAnchorPivotPosition(beamable._meshFilter);

        // TODO: this added as a fix to retraction
        //  TODO: ensure this isnt bugged
        float largestScaleDimension = Mathf.Max(beamable._gameObject.transform.localScale.x, Mathf.Max(beamable._gameObject.transform.localScale.y, beamable._gameObject.transform.localScale.z));
        beamable._tether.anchor = _meshCenter * largestScaleDimension;

        // Debug.Log(_meshCenter);
        // set the connected body to the beam anchor
        beamable._tether.connectedBody = beam.GetTetherAnchor();

        if (beamable._retract)
        {
            // retract the tether
            RetractJoint(beamable._tether, beam);

            // TODO: scale the mesh down to fit in a sphere of some size
            // SOMEHOW do not interfere with the anchor position
            //    remove mesh, create child object, scale mesh?
            beamable.ScaleDown(RetractObjectShrinkSpeed);
        }
        else
        {
            // set connected anchor to the beam collection depth
            beamable._tether.connectedAnchor = -Vector3.up * beam.AnchorDepth();
        }

        // set the mass scale to be proportional between the anchor and the object
        beamable._tether.massScale = beamable._tether.connectedBody.mass / beamable._tether.gameObject.GetComponent<Rigidbody>().mass;
        beamable._tether.connectedMassScale = 1f;
    }

    public void RetractJoint(ConfigurableJoint tetherJoint, TractorBeam beam)
    {
        // assign linear limit
        // SoftJointLimitSpring linearLimitSpring = new SoftJointLimitSpring();
        // linearLimitSpring.spring = 5f;
        // linearLimitSpring.damper = 1f;
        tetherJoint.linearLimitSpring = _retractionLinearLimitSpring;

        // SoftJointLimit linearLimit = new SoftJointLimit();
        // linearLimit.limit = 0;
        tetherJoint.linearLimit = _retractionLinearLimit;


        // JointDrive xDrive = tetherJoint.xDrive;
        // xDrive.positionSpring = 10f;
        tetherJoint.xDrive = _retractionXDrive;

        // JointDrive yDrive = tetherJoint.yDrive;
        // yDrive.positionSpring = 10f;
        tetherJoint.yDrive = _retractionYDrive;


        // JointDrive zDrive = tetherJoint.zDrive;
        // zDrive.positionSpring = 10f;
        tetherJoint.zDrive = _retractionZDrive;


        float largestScaleDimension = Mathf.Max(tetherJoint.transform.localScale.x, Mathf.Max(tetherJoint.transform.localScale.y, tetherJoint.transform.localScale.z));
        Vector3 _meshCenter = MeshFilters.GetAnchorPivotPosition(tetherJoint.gameObject.GetComponent<MeshFilter>());
        // tetherJoint.anchor = _meshCenter * largestScaleDimension * -1f;
        // Vector3 destination = _meshCenter * largestScaleDimension;
        Vector3 destination = Vector3.zero;
        // retract the connected anchor over time
        tetherJoint.connectedAnchor = Vector3.Lerp(tetherJoint.connectedAnchor, destination, Time.deltaTime * beam.RetractionSpeed);
    }
}
