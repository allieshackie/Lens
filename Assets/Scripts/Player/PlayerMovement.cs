using System;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerMovement : MonoBehaviour
{
    [SerializeField] private float _moveSpeed = 5.0f;
    [SerializeField] private float _strafeSpeed = 3.0f;
    [SerializeField] private float _gravity = -9.81f;

    [SerializeField] private float _lookSpeed = 0.15f;
    [SerializeField] private float _lookDampening = 0.08f;
    [SerializeField] private float _maxLookUp = -90;
    [SerializeField] private float _maxLookDown = 90;
    [SerializeField] private Transform _cameraTarget;

    [SerializeField] private Camera _playerCamera;
    [SerializeField] private Transform _gunTransform;
    [SerializeField] private float _maxShootRange = 100f;
    [SerializeField] private float _gunDamage = 1f;
    
    [SerializeField] private Animator _animator;

    // Testing 
    [SerializeField] private TextMeshProUGUI _velocityText;

    private CharacterController _controller;
    private Vector2 _moveVector;
    private Vector3 _playerVelocity;
    private Player_Controls _playerControls;

    private bool _mouseCaptured = true;
    private float _targetYaw;
    private float _targetPitch;
    private float _currentPitch;
    private float _yawVelocity;
    private float _pitchVelocity;

    void Awake()
    {
        _playerControls = new Player_Controls();

    }

    void OnEnable()
    {
        _playerControls.Player.Movement.performed += OnMove;
        _playerControls.Player.Movement.canceled += OnMove;

        _playerControls.Player.Attack.performed += OnShoot;

        _playerControls.Player.DebugReleaseMouse.performed += OnToggleMouse;

        _playerControls.Player.Enable();        
    }

    void OnDisable()
    {
        _playerControls.Player.Movement.performed -= OnMove;
        _playerControls.Player.Movement.canceled -= OnMove;

        _playerControls.Player.Attack.performed -= OnShoot;

        _playerControls.Player.DebugReleaseMouse.performed -= OnToggleMouse;

        _playerControls.Player.Disable();    
    }

    private void OnMove(InputAction.CallbackContext context)
    {
        _moveVector = context.action.ReadValue<Vector2>();
    }

    private void OnShoot(InputAction.CallbackContext context)
    {
        Ray cameraRay = _playerCamera.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0f));

        if (Physics.Raycast(cameraRay, out RaycastHit cameraHit, _maxShootRange))
        {
            Enemy enemy = cameraHit.collider.GetComponent<Enemy>();
            if (enemy)
            {
                Debug.Log("Enemy hit");
                enemy.TakeHit(); // TODO: Should change on hit to take in gun dmg
            }

            Debug.DrawLine(_gunTransform.position, cameraHit.point, Color.red, 1.5f);
        }
        else
        {
            Debug.DrawLine(_gunTransform.position, cameraRay.GetPoint(_maxShootRange), Color.yellow, 1.5f);
        }
    }

    private void OnToggleMouse(InputAction.CallbackContext context)
    {
        _mouseCaptured = !_mouseCaptured;
        if (_mouseCaptured)
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }
        else
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
    }

    void Start()
    {
        _controller = GetComponent<CharacterController>();
        _targetYaw = transform.eulerAngles.y;
        _targetPitch = Mathf.DeltaAngle(0f, _cameraTarget.localEulerAngles.x);
        _currentPitch = _targetPitch;

        // Hack for now to hide cursor
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    private void Update()
    {
        RotatePlayer();
        MovePlayer();
    }

    private void MovePlayer()
    {
        Vector3 move = transform.right * _moveVector.x + transform.forward * _moveVector.y;
        _playerVelocity.y += _gravity * Time.deltaTime;
        _controller.Move((move * _moveSpeed + _playerVelocity) * Time.deltaTime);

        float animVelocity = Mathf.Clamp(_controller.velocity.magnitude, 0f, _moveSpeed);
        _animator.SetFloat("Velocity", animVelocity);

        _velocityText.text = "Velocity: " + Mathf.Round(animVelocity);
    }

    private void RotatePlayer()
    {
        if (!_mouseCaptured)
        {
            return;
        }
        Vector2 lookInput = _playerControls.Player.Look.ReadValue<Vector2>();
        _targetYaw += lookInput.x * _lookSpeed;
        _targetPitch = Mathf.Clamp(_targetPitch - lookInput.y * _lookSpeed, _maxLookUp, _maxLookDown);
        
        float yaw = Mathf.SmoothDampAngle(transform.eulerAngles.y, _targetYaw, ref _yawVelocity, _lookDampening);
        _currentPitch = Mathf.SmoothDampAngle(_currentPitch, _targetPitch, ref _pitchVelocity, _lookDampening);

        transform.rotation = Quaternion.Euler(0f, yaw, 0f);
        _cameraTarget.localRotation = Quaternion.Euler(_currentPitch, 0f, 0f);
    }
}
