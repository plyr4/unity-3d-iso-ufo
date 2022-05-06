using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(PlayerInput))]
[RequireComponent(typeof(PlayerInputs))]
[DisallowMultipleComponent]
public class PlayerMovement : MonoBehaviour
{
    [SerializeField]
    private Rigidbody _rb;

    [Header("Input Settings")]
    [Space]
    [Space]
    [Space]
    [SerializeField]
    private PlayerInput _playerInput;
    [SerializeField]
    private PlayerInputs _input;

    [Header("Camera Settings")]
    [Space]
    [Space]
    [Space]
    [SerializeField]
    private Transform _mainCameraPivot;
    private Camera _mainCamera;
    [SerializeField]
    private float CameraSizeCoefficient = 20f;

    [SerializeField]
    private float CameraRotationSpeed = 75f;
    private Vector3 _cameraOffset;

    [Header("Movement Settings")]
    [Space]
    [Space]
    [Space]
    [SerializeField]
    [Range(1f, 50f)]
    private float HorizontalMaxSpeed = 20f;
    [SerializeField]
    [Range(1f, 200f)]
    private float HorizontalAcceleration = 200f;
    [SerializeField]
    [Range(1f, 200f)]
    private float HorizontalMaxAccelerationForce = 150f;
    [SerializeField]
    private Vector3 HorizontalForceScale = new Vector3(1f, 0, 1f);
    [SerializeField]
    [Range(1f, 30f)]
    private float VerticalMaxSpeed = 12f;
    [SerializeField]
    [Range(1f, 200f)]
    private float VerticalAcceleration = 200f;
    [SerializeField]
    [Range(1f, 200f)]
    private float VerticalMaxAccelerationForce = 150f;
    [SerializeField]
    private Vector3 VerticalForceScale = Vector3.up;
    [SerializeField]
    [Range(0f, 2f)]
    private float SpeedFactor = 1f;
    [SerializeField]
    [Range(0f, 2f)]
    private float MaxAccelForceFactor = 1f;
    [SerializeField]
    private AnimationCurve AccelerationFactorFromDot;
    [SerializeField]
    private AnimationCurve MaxAccelerationForceFactorFromDot;
    private Vector3 _horizontalUnitGoal;
    private Vector3 _horizontalGoalVel;
    private Vector3 _verticalUnitGoal;
    private Vector3 _verticalGoalVel;

    [Header("Elevation Settings")]
    [Space]
    [Space]
    [Space]
    [SerializeField]
    [Range(4f, 10f)]
    private float MinVerticalElevation = 4f;
    [SerializeField]
    [Range(11f, 30f)]
    private float MaxVerticalElevation = 20f;
    [SerializeField]
    [Range(0f, 500f)]
    private float MaxElevationSpringForce = 100f;
    [SerializeField]
    [Range(0f, 200f)]
    private float ElevationSpringDamper = 0.2f;
    [Space]
    [SerializeField]
    private LayerMask GroundLayers;
    [Space]

    // cached elevation properties
    private float _cachedTime;
    public RaycastHit _currentGroundHit
    {
        get
        {
            if (_cachedTime != Time.time) UpdateElevationProperties();
            return _cachedGroundHit;
        }
    }
    private RaycastHit _cachedGroundHit;
    public bool _groundIsHit
    {
        get
        {
            if (_cachedTime != Time.time) UpdateElevationProperties();
            return _cachedGroundIsHit;
        }
    }
    private bool _cachedGroundIsHit = false;

    [Header("Animation Body Settings")]
    [Space]
    [Space]
    [Space]
    [SerializeField]
    private GameObject _ufoBody;
    [SerializeField]
    [Range(0f, 15f)]
    private float BodyTiltDegree = 4f;
    [SerializeField]
    [Range(1f, 2f)]
    private float BodyTiltBoostCoefficient = 2f;
    [SerializeField]
    [Range(0f, 1f)]
    private float BodyRotationRate = 0.12f;
    private float _targetRotation;
    private float _rotationVelocity;

    [Header("Misc Settings")]
    [Space]
    [Space]
    [Space]
    [SerializeField]
    [Range(0.00001f, 1f)]
    private float ApproximationPrecision = 0.00001f;
    private bool IsCurrentDeviceMouse => _playerInput.currentControlScheme == "KeyboardMouse";

    private void Start()
    {
        _cameraOffset = _mainCameraPivot.transform.position - transform.position;
        if (_mainCamera == null) _mainCamera = _mainCameraPivot.GetComponentInChildren<Camera>();
    }

    private void FixedUpdate()
    {
        MoveHorizontalWithForce();
        MoveVerticalWithForce();
        ClampVerticalMovement();
        RotateAnimationBody();
        RotateCamera();
        FollowCameraPosition();
        UpdateCameraSize();
    }

    private void MoveHorizontalWithForce()
    {
        // capture relevant input axis
        Vector3 move = new Vector3(_input.horizontalMove.x, 0f, _input.horizontalMove.y);
        float inputMagnitude = IsCurrentDeviceMouse ? 1f : move.magnitude;

        // normalize if necessary
        if (move.magnitude > 0)
        {
            move.Normalize();
        }

        _horizontalUnitGoal = AngleLeft() * _mainCameraPivot.TransformDirection(move) * inputMagnitude;

        Vector3 unitVel = _horizontalGoalVel.normalized;

        // groundVel is just going to be zero for now
        Vector3 groundVel = Vector3.zero;

        float velDot = Vector3.Dot(_horizontalUnitGoal, unitVel);

        float accel = HorizontalAcceleration * AccelerationFactorFromDot.Evaluate(velDot);

        Vector3 goalVel = _horizontalUnitGoal * HorizontalMaxSpeed * SpeedFactor;

        _horizontalGoalVel = Vector3.MoveTowards(_horizontalGoalVel, goalVel + groundVel, accel * Time.deltaTime);

        Vector3 neededAccel = (_horizontalGoalVel - _rb.velocity) / Time.deltaTime;

        float maxAccel = HorizontalMaxAccelerationForce * MaxAccelerationForceFactorFromDot.Evaluate(velDot) * MaxAccelForceFactor;

        neededAccel = Vector3.ClampMagnitude(neededAccel, maxAccel);

        _rb.AddForce(Vector3.Scale(neededAccel * _rb.mass, HorizontalForceScale));
    }

    private void MoveVerticalWithForce()
    {
        Vector3 move = new Vector3(0f, _input.verticalMove.y, 0f);
        float inputMagnitude = IsCurrentDeviceMouse ? 1f : move.magnitude;

        // normalize if necessary
        if (move.magnitude > 0)
        {
            move.Normalize();
        }

        _verticalUnitGoal = move * inputMagnitude;

        Vector3 unitVel = _verticalGoalVel.normalized;

        // groundVel is just going to be zero for now
        Vector3 groundVel = Vector3.zero;

        float velDot = Vector3.Dot(_verticalUnitGoal, unitVel);

        float accel = VerticalAcceleration * AccelerationFactorFromDot.Evaluate(velDot);

        Vector3 goalVel = _verticalUnitGoal * VerticalMaxSpeed * SpeedFactor;

        _verticalGoalVel = Vector3.MoveTowards(_verticalGoalVel, goalVel + groundVel, accel * Time.deltaTime);

        Vector3 neededAccel = (_verticalGoalVel - _rb.velocity) / Time.deltaTime;

        float maxAccel = VerticalMaxAccelerationForce * MaxAccelerationForceFactorFromDot.Evaluate(velDot) * MaxAccelForceFactor;

        neededAccel = Vector3.ClampMagnitude(neededAccel, maxAccel);

        _rb.AddForce(Vector3.Scale(neededAccel * _rb.mass, Vector3.up));
    }
    private void ClampVerticalMovement()
    {
        Vector3 positionAtEndOfStep = _rb.position + _rb.velocity * Time.deltaTime;
        positionAtEndOfStep.y = Mathf.Clamp(positionAtEndOfStep.y, MinVerticalElevation, MaxVerticalElevation);
        Vector3 neededVelocity = (positionAtEndOfStep - _rb.position) / Time.deltaTime;
        _rb.velocity = neededVelocity;
    }

    private void MoveToElevation(float targetElevation, float power)
    {
        // use the cached ground hit
        if (_groundIsHit)
        {
            // current velocity of the moving object
            Vector3 _currentVelocity = _rb.velocity;

            // initialize the hit velocity
            Vector3 _hitVelocity = Vector3.zero;

            // the rigidbody hit by the raycast
            Rigidbody _hitRigidbody = _currentGroundHit.rigidbody;

            // use the directional velocity of the hit object
            //    this might be useful for levitating off entities other than the ground
            if (_hitRigidbody != null) _hitVelocity = _hitRigidbody.velocity;

            // calculate the velocity towards the ground
            float _directionalVelocity = Vector3.Dot(GroundDirection(), _currentVelocity);
            float _hitDirectionVelocity = Vector3.Dot(GroundDirection(), _hitVelocity);

            // the velocity difference between the two interacting objects
            float _relativeVelocity = _directionalVelocity - _hitDirectionVelocity;

            // calculate the distance we need to move
            float _distanceToElevation = _currentGroundHit.distance - targetElevation;

            // calculate the force to spring the object towards the desired elevation
            //    adjust strength and damper to apply different spring
            float _springForce = (_distanceToElevation * power) - (_relativeVelocity * ElevationSpringDamper);

            // clamp maximum spring force
            if (MaxElevationSpringForce != 0f) _springForce = Mathf.Clamp(_springForce, -MaxElevationSpringForce, MaxElevationSpringForce);

            // draw a line to debug the calculations
            Debug.DrawLine(transform.position, transform.position + (GroundDirection() * _springForce), Color.green);

            // apply force to move the elevation towards the desired elevation in the direction towards the ground
            //    moving up is actually applying negative force in the direction of the ground
            _rb.AddForce(GroundDirection() * _springForce);

            // apply changes to the hit entity
            if (_hitRigidbody != null)
            {
                // apply the opposite force to the object
                _hitRigidbody.AddForceAtPosition(GroundDirection() * -_springForce, _currentGroundHit.point);
            }
        }
    }

    private void RotateAnimationBody()
    {
        // capture relevant input axis
        Vector3 _horizontalInput = new Vector3(_input.horizontalMove.x, _input.horizontalMove.y, 0f);

        // capture the body's current y rotation
        float _bodyYRotation = _ufoBody.transform.eulerAngles.y;

        // reset any horizontal tilt
        _ufoBody.transform.rotation = Quaternion.Euler(0f, _bodyYRotation, 0f);

        // apply rotation when input is received
        if (_horizontalInput != Vector3.zero)
        {
            // calculate angle from input in degrees
            float _inputAngle = Mathf.Atan2(_horizontalInput.normalized.x, _horizontalInput.normalized.y) * Mathf.Rad2Deg;

            // apply the input angle to the camera's current euler angle
            _targetRotation = _mainCamera.gameObject.transform.eulerAngles.y + _inputAngle;

            // smooth between the body's current angle and the target angle
            float _rotation = Mathf.SmoothDampAngle(_bodyYRotation, _targetRotation, ref _rotationVelocity, BodyRotationRate);

            // calculate the tilt based on speed
            float tilt = _input.boost ? BodyTiltDegree * BodyTiltBoostCoefficient : BodyTiltDegree;

            // apply body rotations
            _ufoBody.transform.rotation = Quaternion.Euler(tilt, _rotation, 0f);
        }
    }

    private void UpdateElevationProperties()
    {
        _cachedTime = Time.time;

        // variable to store the hit
        RaycastHit _groundHit;

        // attempt to hit the ground using raycast
        _cachedGroundIsHit = Physics.Raycast(transform.position, GroundDirection(), out _groundHit, Mathf.Infinity, GroundLayers);
        _cachedGroundHit = _groundHit;
    }

    private float GetGroundDistance()
    {
        return _groundIsHit ? _currentGroundHit.distance : transform.position.y;
    }

    private Vector3 GroundDirection()
    {
        return -transform.up;
    }

    private Quaternion AngleLeft()
    {
        return Quaternion.Euler(new Vector3(0f, -45f, 0f));
    }

    private void FollowCameraPosition()
    {
        // calculate camera destination using the original player/camera position offset
        Vector3 targetCamPos = transform.position + _cameraOffset;

        // move the camera to the offset destination
        _mainCameraPivot.transform.position = targetCamPos;

    }

    private void RotateCamera()
    {
        // capture relevant input axis
        Vector3 _verticalInput = new Vector3(_input.verticalMove.x, 0f, 0f);

        // skip camera rotation when no input is provided
        if (_verticalInput == Vector3.zero) return;

        // use appropriate magnitude
        float inputMagnitude = IsCurrentDeviceMouse ? 1f : _verticalInput.magnitude;

        // calculate target rotation based on current rotation, input and time
        float _targetRotation = _mainCameraPivot.transform.localEulerAngles.y + _verticalInput.normalized.x * inputMagnitude * Time.deltaTime * CameraRotationSpeed;

        // apply the calculated rotation
        _mainCameraPivot.transform.localRotation =
            Quaternion.Euler(_mainCameraPivot.transform.localEulerAngles.x, _targetRotation, _mainCameraPivot.transform.localEulerAngles.z);
    }

    private void UpdateCameraSize()
    {
        // fetches camera and updates the orthographic size
        _mainCamera.orthographicSize = CalculateOrthographicCameraSize();
    }

    private float CalculateOrthographicCameraSize()
    {
        // calculate camera size based on current elevation / max elevation
        return CameraSizeCoefficient * GetGroundDistance() / MaxVerticalElevation;
    }
}
