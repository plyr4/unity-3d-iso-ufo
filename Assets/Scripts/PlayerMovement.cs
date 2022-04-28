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
    [Range(4f, 10f)]
    private float MinVerticalElevation = 4f;
    [SerializeField]
    [Range(11f, 30f)]
    private float MaxVerticalElevation = 20f;
    [SerializeField]
    private float ElevationBoundsPrecisionCoefficient = 0.001f;
    [SerializeField]
    [Range(0f, 500f)]
    private float MaxElevationSpringForce = 100f;
    [SerializeField]
    [Range(0.01f, 500f)]
    private float ElevationSpringStrength = 100f;
    [SerializeField]
    [Range(0f, 200f)]
    private float ElevationSpringDamper = 0.2f;
    [SerializeField]
    private LayerMask GroundLayers;
    [Space]
    [SerializeField]
    private float HorizontalMaxSpeed = 10f;
    [SerializeField]
    private float HorizontalBoostMaxSpeed = 14f;
    [SerializeField]
    private float HorizontalAcceleration = 4f;
    [Space]
    [SerializeField]
    private float VerticalMaxSpeed = 10f;
    [SerializeField]
    private float VerticalBoostMaxSpeed = 14f;
    [SerializeField]
    private float VerticalAcceleration = 4f;

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

    [Space]
    [SerializeField]
    [Range(0.01f, 1f)]
    private float VelocitySlowRate = 0.9f;
    private float SpeedAccelerationOffset = 0.001f;

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
        MoveHorizontal();
        MoveVertical();
        ClampVerticalMovement();
        RotateAnimationBody();
        RotateCamera();
        FollowCameraPosition();
        UpdateCameraSize();
    }

    private void MoveHorizontal()
    {
        // capture relevant input axis
        Vector3 _horizontalInput = new Vector3(_input.horizontalMove.x, 0f, _input.horizontalMove.y);

        // capture current speed
        float _currentHorizontalSpeed = GetCurrentHorizontalSpeed();

        // calculated speed is relative to our current speed
        float speed = _currentHorizontalSpeed;

        // determine the target speed
        float _targetSpeed = GetTargetHorizontalSpeed();

        // +/- speed if necessary
        if (_currentHorizontalSpeed < _targetSpeed - SpeedAccelerationOffset || _currentHorizontalSpeed > _targetSpeed + SpeedAccelerationOffset)
        {
            // use appropriate magnitude
            float _inputMagnitude = IsCurrentDeviceMouse ? 1f : _horizontalInput.magnitude;

            // interpolate towards the target speed
            speed = Mathf.Lerp(speed, _targetSpeed * _inputMagnitude, Time.deltaTime * HorizontalAcceleration);

            // round 3 places
            speed = Mathf.Round(speed * 1000f) / 1000f;
        }

        // point the input vector in the direction the camera is facing
        Vector3 _cameraDirectedInput = _mainCameraPivot.TransformDirection(_horizontalInput.normalized);

        // angle the input and apply the calculated speed
        Vector3 _horizontalMovement = AngleLeft() * _cameraDirectedInput * speed;

        // when horizontal input is idle return horizontal velocity to rest
        if (_horizontalInput == Vector3.zero)
        {
            // slow the velocity
            _horizontalMovement = _rb.velocity * VelocitySlowRate;

            // apply rest when reaching the lower limit
            if (Mathf.Abs(_rb.velocity.x) <= ApproximationPrecision) _horizontalMovement = new Vector3(0f, _horizontalMovement.y, _horizontalMovement.z);
            if (Mathf.Abs(_rb.velocity.z) <= ApproximationPrecision) _horizontalMovement = new Vector3(_horizontalMovement.x, _horizontalMovement.y, 0f);
        }

        // apply the calculated horizontal velocity
        _rb.velocity = new Vector3(_horizontalMovement.x, _rb.velocity.y, _horizontalMovement.z);
    }

    private void MoveVertical()
    {
        // capture relevant input axis
        Vector3 _verticalInput = new Vector3(0f, _input.verticalMove.y, 0f);

        // value of 1 means velocity slow is left as-is
        float _boundsSlow = 1f;

        // player is moving out of bounds
        if ((!WithinMaximumVerticalBound(ElevationBoundsPrecisionCoefficient * 2f) && HasUpwardVerticalInput()))
        {
            // restrict movement input
            _verticalInput = Vector3.zero;
        }

        // player is moving out of bounds
        if ((!WithinMinimumVerticalBound(ElevationBoundsPrecisionCoefficient * 2f) && HasDownwardVerticalInput()))
        {
            // restrict movement input
            _verticalInput = Vector3.zero;
        }

        // capture current speed
        float _currentVerticalSpeed = GetCurrentVerticalSpeed();

        // calculated speed is relative to our current speed
        float speed = _currentVerticalSpeed;

        // determine the target speed
        float _targetSpeed = GetTargetVerticalSpeed();

        // +/- speed if necessary
        if (_currentVerticalSpeed < _targetSpeed - SpeedAccelerationOffset || _currentVerticalSpeed > _targetSpeed + SpeedAccelerationOffset)
        {
            // use appropriate magnitude
            float _inputMagnitude = IsCurrentDeviceMouse ? 1f : _verticalInput.magnitude;

            // interpolate towards the target speed
            speed = Mathf.Lerp(speed, _targetSpeed * _inputMagnitude, Time.deltaTime * VerticalAcceleration);

            // round 3 places
            speed = Mathf.Round(speed * 1000f) / 1000f;
        }

        // normalize the vertical input and apply the calculated speed
        Vector3 _verticalMovement = _verticalInput.normalized * speed;

        // when vertical input is idle return vertical velocity to rest
        if (_verticalInput == Vector3.zero)
        {
            // slow the velocity
            _verticalMovement = _rb.velocity * VelocitySlowRate * _boundsSlow;

            // apply rest when reaching the lower limit
            if (Mathf.Abs(_rb.velocity.y) <= ApproximationPrecision) _verticalMovement = new Vector3(_verticalMovement.x, 0f, _verticalMovement.z);
        }

        // apply the calculated vertical velocity
        _rb.velocity = new Vector3(_rb.velocity.x, _verticalMovement.y, _rb.velocity.z);
    }

    private void ClampVerticalMovement()
    {
        if (!WithinMinimumVerticalBound(ElevationBoundsPrecisionCoefficient * 0.5f)) MoveToElevation(MinVerticalElevation);
        if (!WithinMaximumVerticalBound(ElevationBoundsPrecisionCoefficient * 0.5f)) MoveToElevation(MaxVerticalElevation);
    }

    private void MoveToElevation(float targetElevation)
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
            float _springForce = (_distanceToElevation * ElevationSpringStrength) - (_relativeVelocity * ElevationSpringDamper);

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

    private bool WithinMinimumVerticalBound(float precision)
    {
        return (GetGroundDistance() > (MinVerticalElevation + precision));
    }
    private bool WithinMaximumVerticalBound(float precision)
    {
        return (GetGroundDistance() < (MaxVerticalElevation - precision));
    }

    private bool WithinVerticalBounds(float precision)
    {
        return WithinMinimumVerticalBound(ElevationBoundsPrecisionCoefficient) && WithinMaximumVerticalBound(ElevationBoundsPrecisionCoefficient);
    }

    private float GetGroundDistance()
    {
        return _groundIsHit ? _currentGroundHit.distance : transform.position.y;
    }

    private Vector3 GroundDirection()
    {
        return -transform.up;
    }

    private bool HasDownwardVerticalInput()
    {
        return _input.verticalMove.y < 0f;
    }

    private bool HasUpwardVerticalInput()
    {
        return _input.verticalMove.y > 0f;
    }

    private float GetCurrentHorizontalSpeed()
    {
        return new Vector3(_rb.velocity.x, 0f, _rb.velocity.z).magnitude;
    }

    private float GetCurrentVerticalSpeed()
    {
        return new Vector3(0f, _rb.velocity.y, 0f).magnitude;
    }

    private float GetTargetHorizontalSpeed()
    {
        return (_input.horizontalMove != Vector3.zero) ? (_input.boost ? HorizontalBoostMaxSpeed : HorizontalMaxSpeed) : 0f;
    }

    private float GetTargetVerticalSpeed()
    {
        return (Mathf.Abs(_input.verticalMove.y) > ApproximationPrecision) ? (_input.boost ? VerticalBoostMaxSpeed : VerticalMaxSpeed) : 0f;
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
