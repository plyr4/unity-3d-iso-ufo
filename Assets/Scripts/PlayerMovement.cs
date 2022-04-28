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
    private float MinVerticalHeight = 4f;
    [SerializeField]
    private float MaxVerticalHeight = 20f;
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
        _mainCamera = _mainCameraPivot.GetComponentInChildren<Camera>();
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
            _verticalMovement = _rb.velocity * VelocitySlowRate;

            // apply rest when reaching the lower limit
            if (Mathf.Abs(_rb.velocity.y) <= ApproximationPrecision) _verticalMovement = new Vector3(_verticalMovement.x, 0f, _verticalMovement.z);
        }

        // apply the calculated vertical velocity
        _rb.velocity = new Vector3(_rb.velocity.x, _verticalMovement.y, _rb.velocity.z);
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

    private void ClampVerticalMovement()
    {
        // capture relevant input axis
        Vector3 _verticalInput = new Vector3(0f, _input.verticalMove.y, 0f);

        // clamp minimum height using vertical velocity
        if (transform.position.y <= MinVerticalHeight && _verticalInput.y <= 0f) _rb.velocity = new Vector3(_rb.velocity.x, 0f, _rb.velocity.z);

        // clamp maximum height using vertical velocity
        if (transform.position.y >= MaxVerticalHeight && _verticalInput.y >= 0f) _rb.velocity = new Vector3(_rb.velocity.x, 0f, _rb.velocity.z);
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
        // calculate camera size based on current height / max height
        return CameraSizeCoefficient * transform.position.y / MaxVerticalHeight;
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
}
