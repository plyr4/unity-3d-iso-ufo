using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(CharacterController))]
[RequireComponent(typeof(PlayerInput))]
[RequireComponent(typeof(PlayerInputs))]
public class PlayerController : MonoBehaviour
{
    [Header("Player")]
    [Tooltip("Move speed of the character in m/s")]
    [SerializeField]
    private float MoveSpeed = 2.0f;
    [Tooltip("Sprint speed of the character in m/s")]
    [SerializeField]
    private float SprintSpeed = 5.335f;
    [Tooltip("How fast the character turns to face movement direction")]
    [Range(0f, 0.3f)]
    [SerializeField]
    private float RotationSmoothTime = 0.12f;
    [Tooltip("Acceleration and deceleration")]
    [SerializeField]
    private float SpeedChangeRate = 10f;


    // player
    private float _horizontalSpeed;
    private float _verticalSpeed;
    private float _targetRotation;
    private float _rotationVelocity;
    private float _verticalVelocity;
    private Vector3 _verticalDirection;

    private CharacterController _controller;
    private PlayerInput _playerInput;
    private PlayerInputs _input;
    [SerializeField]
    private GameObject _mainCameraPivot;

    private bool IsCurrentDeviceMouse => _playerInput.currentControlScheme == "KeyboardMouse";
    [SerializeField]
    private float CameraSizeCoefficient = 20f;
    [SerializeField]
    private float MaxHeight = 20f;
    [SerializeField]
    private float MinHeight = 6f;
    private Vector3 _cameraOffset;

    [SerializeField]
    private GameObject _ufoBody;
    [SerializeField]
    private float TiltDegree = 15f;

    [SerializeField]
    private float TiltSprintModifier = 1.5f;

    private void Start()
    {
        _controller = GetComponent<CharacterController>();
        _input = GetComponent<PlayerInputs>();
        _playerInput = GetComponent<PlayerInput>();
        _cameraOffset = _mainCameraPivot.transform.position - transform.position;
    }

    private void Update()
    {
        MoveHorizontal();
        MoveVertical();
    }

    private void LateUpdate()
    {
        FollowCameraPosition();
        RotateCamera();
    }

    private void FollowCameraPosition() {
        Debug.Log("late update FollowCameraPosition");
		Vector3 targetCamPos = transform.position + _cameraOffset;
        _mainCameraPivot.transform.position = targetCamPos;
        _mainCameraPivot.GetComponentInChildren<Camera>().orthographicSize = CameraSizeCoefficient * transform.position.y / MaxHeight;
    }

    private void RotateCamera() {
        // only care about x axis
        Vector2 verticalMove = new Vector2(_input.verticalMove.x, 0f);
        if (verticalMove == Vector2.zero) return;












                float rotationX = _mainCameraPivot.transform.localEulerAngles.x;
                float newRotationY = _mainCameraPivot.transform.localEulerAngles.y + verticalMove.x;

                // Weird clamping code due to weird Euler angle mapping...
                // float newRotationX = (rotationX - inputRotateAxisY);
                float newRotationX = (rotationX);
                if (rotationX <= 90.0f && newRotationX >= 0.0f)
                    newRotationX = Mathf.Clamp(newRotationX, 0.0f, 90.0f);
                if (rotationX >= 270.0f)
                    newRotationX = Mathf.Clamp(newRotationX, 270.0f, 360.0f);

                _mainCameraPivot.transform.localRotation = Quaternion.Euler(_mainCameraPivot.transform.localEulerAngles.x, newRotationY, _mainCameraPivot.transform.localEulerAngles.z);

        // _rotationDirection = verticalMove.y > 0f ? Vector3.left : Vector3.right;

    }

    private void MoveVertical()
    {
        float targetSpeed = MoveSpeed;
        // only care about y axis
        Vector2 verticalMove = new Vector2(0f, _input.verticalMove.y);
        if (verticalMove == Vector2.zero) targetSpeed = 0f;
        else
        {
            _verticalDirection = verticalMove.y < 0f ? Vector3.up : Vector3.down;
        }
        float currentVerticalSpeed = new Vector3(0f, _controller.velocity.y, 0f).magnitude;
        float speedOffset = 0.1f;
        float inputMagnitude = IsCurrentDeviceMouse ? 1f : verticalMove.magnitude;

        // accelerate or decelerate to target speed
        if (currentVerticalSpeed < targetSpeed - speedOffset || currentVerticalSpeed > targetSpeed + speedOffset)
        {
            // creates curved result rather than a linear one giving a more organic speed change
            // note T in Lerp is clamped, so we don't need to clamp our speed
            _verticalSpeed = Mathf.Lerp(currentVerticalSpeed, targetSpeed * inputMagnitude, Time.deltaTime * SpeedChangeRate);

            // round speed to 3 decimal places
            _verticalSpeed = Mathf.Round(_verticalSpeed * 1000f) / 1000f;
        }
        else
        {
            _verticalSpeed = targetSpeed;
        }
    }

    private void MoveHorizontal()
    {
        // set target speed based on move speed, sprint speed and if sprint is pressed
        float targetSpeed = _input.sprint ? SprintSpeed : MoveSpeed;

        // a simplistic acceleration and deceleration designed to be easy to remove, replace, or iterate upon

        // note: Vector2's == operator uses approximation so is not floating point error prone, and is cheaper than magnitude
        // if there is no input, set the target speed to 0
        Vector2 horizontalMove = new Vector2(_input.horizontalMove.x, _input.horizontalMove.y);
        if (horizontalMove == Vector2.zero) targetSpeed = 0f;

        // a reference to the players current horizontal velocity
        float currentHorizontalSpeed = new Vector3(_controller.velocity.x, 0f, _controller.velocity.z).magnitude;

        float speedOffset = 0.1f;
        float inputMagnitude = IsCurrentDeviceMouse ? 1f : horizontalMove.magnitude;

        // accelerate or decelerate to target speed
        if (currentHorizontalSpeed < targetSpeed - speedOffset || currentHorizontalSpeed > targetSpeed + speedOffset)
        {
            // creates curved result rather than a linear one giving a more organic speed change
            // note T in Lerp is clamped, so we don't need to clamp our speed
            _horizontalSpeed = Mathf.Lerp(currentHorizontalSpeed, targetSpeed * inputMagnitude, Time.deltaTime * SpeedChangeRate);

            // round speed to 3 decimal places
            _horizontalSpeed = Mathf.Round(_horizontalSpeed * 1000f) / 1000f;
        }
        else
        {
            _horizontalSpeed = targetSpeed;
        }

        // normalise input direction
        Vector3 inputDirection = new Vector3(horizontalMove.x, 0f, horizontalMove.y).normalized;

        // note: Vector2's != operator uses approximation so is not floating point error prone, and is cheaper than magnitude
        // if there is a move input rotate player when the player is moving
        if (horizontalMove != Vector2.zero)
        {
            _targetRotation = Mathf.Atan2(inputDirection.x, inputDirection.z) * Mathf.Rad2Deg + _mainCameraPivot.GetComponentInChildren<Camera>().gameObject.transform.eulerAngles.y;
            float rotation = Mathf.SmoothDampAngle(transform.eulerAngles.y, _targetRotation, ref _rotationVelocity, RotationSmoothTime);

            // rotate to face input direction relative to camera position
            transform.rotation = Quaternion.Euler(0f, rotation, 0f);

            // apply "body animation"
            float tilt = TiltDegree;
            if (_input.sprint) tilt *= TiltSprintModifier;
            _ufoBody.transform.rotation = Quaternion.Euler(tilt, rotation, 0f);
            // transform.rotation = Quaternion.Euler(tilt, rotation, 0f);
        }
        if (horizontalMove == Vector2.zero) {
            // revert "body animation"
            _ufoBody.transform.rotation = Quaternion.Euler(0f, transform.eulerAngles.y, 0f);
            // transform.rotation = Quaternion.Euler(0f, transform.eulerAngles.y, 0f);
        }


        Vector3 horizontalDirection = Quaternion.Euler(0f, _targetRotation, 0f) * Vector3.forward;

        // move the player
        _controller.Move(horizontalDirection.normalized * (_horizontalSpeed * Time.deltaTime) + (_verticalDirection * _verticalSpeed * Time.deltaTime));

        // clamp vertical movement
        if (_verticalDirection == Vector3.up && transform.position.y >= MaxHeight)
        {
            _verticalSpeed = 0;
            transform.position = new Vector3(transform.position.x, MaxHeight, transform.position.z);
        }


        if (_verticalDirection == Vector3.down && transform.position.y <= MinHeight)
        {
            _verticalSpeed = 0;
            transform.position = new Vector3(transform.position.x, MinHeight, transform.position.z);
        }
    }
}

