using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerMovement : MonoBehaviour
{
    [SerializeField]
    private Rigidbody _rb;
    [SerializeField]
    private Transform _mainCameraPivot;
    [SerializeField]
    private PlayerInput _playerInput;
    [SerializeField]
    private PlayerInputs _input;
    [Space]
    [SerializeField]
    private float Speed;
    [SerializeField]
    private float VerticalSpeed;
        [SerializeField]
    private float CameraRotationSpeed;
    [SerializeField]
    private float Sensitivity;
    [SerializeField]
    private float CameraSizeCoefficient = 20f;
    [SerializeField]
    private float MinHeight = 4f;
    [SerializeField]
    private float MaxHeight = 20f;
    [SerializeField]
    private float SpeedChangeRate = 1f;

    private Vector3 _horizontalInput, _verticalInput;
    private Vector3 _cameraOffset;
    private Vector3 _verticalDirection;
    private float _verticalSpeed;
    private bool IsCurrentDeviceMouse => _playerInput.currentControlScheme == "KeyboardMouse";

    void Start()
    {
        _cameraOffset = _mainCameraPivot.transform.position - transform.position;
    }
    void Update()
    {
        _horizontalInput = new Vector3(_input.horizontalMove.x, 0f, _input.horizontalMove.y);
        _horizontalInput.Normalize();

        _verticalInput = new Vector3(_input.verticalMove.x, _input.verticalMove.y, 0f);
        _verticalInput.Normalize();

    }

    void FixedUpdate()
    {
        MoveHorizontal();
        MoveVertical();
        FollowCameraPosition();
        RotateCamera();
    }


    void LateUpdate()
    {

    }

    private void MoveHorizontal()
    {
        // Vector3 moveVector = transform.TransformDirection(playerMovementInput)  * Speed;
        Vector3 moveVector = _mainCameraPivot.TransformDirection(_horizontalInput) * (_input.sprint ? Speed * 1.25f : Speed);
        moveVector = Quaternion.Euler(new Vector3(0f, -45f, 0f)) * moveVector;
        _rb.velocity = new Vector3(moveVector.x, _rb.velocity.y, moveVector.z);
    }

    private void MoveVertical()
    {
        Vector3 moveVector = _verticalInput * VerticalSpeed;
        float y = _rb.transform.position.y;
        if (y >= MaxHeight && moveVector.y > 0) {
            moveVector.y = 0;
        }
        if (y <= MinHeight && moveVector.y < 0) {
            moveVector.y = 0;
        }
        
        _rb.velocity = new Vector3(_rb.velocity.x, moveVector.y, _rb.velocity.z);
    }
    private void FollowCameraPosition()
    {
        Vector3 targetCamPos = transform.position + _cameraOffset;
        _mainCameraPivot.transform.position = targetCamPos;
        float cameraSize = CameraSizeCoefficient * transform.position.y / MaxHeight;
        // Debug.Log("assigning orthographic size to " + cameraSize);
        _mainCameraPivot.GetComponentInChildren<Camera>().orthographicSize = cameraSize;
    }

    private void RotateCamera()
    {
        // only care about x axis
        Vector2 verticalMove = new Vector2(_verticalInput.x, 0f);
        // verticalMove = new Vector2(1f, 0);
        if (verticalMove == Vector2.zero) return;
        float rotationX = _mainCameraPivot.transform.localEulerAngles.x;
        float newRotationY = _mainCameraPivot.transform.localEulerAngles.y + verticalMove.x * Time.deltaTime * CameraRotationSpeed;

        _mainCameraPivot.transform.localRotation = Quaternion.Euler(_mainCameraPivot.transform.localEulerAngles.x, newRotationY, _mainCameraPivot.transform.localEulerAngles.z);

        // _rotationDirection = verticalMove.y > 0f ? Vector3.left : Vector3.right;

    }
}
