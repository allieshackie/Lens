using System;
using System.Collections;
using TMPro;
using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Animations.Rigging;
using static Unity.U2D.Physics.PhysicsBody;

public class PlayerMovement : MonoBehaviour
{
    [SerializeField] private float _moveSpeed = 5.0f;
    [SerializeField] private float _sprintSpeed = 5.0f;
    [SerializeField] private float _adsSpeed = 3.0f;
    [SerializeField] private float _gravity = -9.81f;

    [SerializeField] private float _lookSpeed = 0.15f;
    [SerializeField] private float _lookDampening = 0.08f;
    [SerializeField] private float _maxLookUp = -90;
    [SerializeField] private float _maxLookDown = 90;
    [SerializeField] private Transform _cameraTarget;

    [SerializeField] private Camera _playerCamera;
    [SerializeField] private CinemachineCamera _overShouldCamera;
    [SerializeField] private CinemachineCamera _adsCamera;
    [SerializeField] private Transform _gunTransform;
    [SerializeField] private float _maxShootRange = 100f;
    [SerializeField] private float _gunDamage = 1f;
    [SerializeField] private Transform _aimTarget;
    
    // Visuals
    [SerializeField] private Animator _animator;
    [SerializeField] private ParticleSystem _bulletCasing;
    [SerializeField] private Light _muzzleFlash;
    [SerializeField] private Rig _aimRig;

    private CharacterController _controller;
    private Vector2 _moveVector;
    private Vector3 _playerVelocity;
    private Player_Controls _playerControls;
    private Reticle _reticle;

    private bool _mouseCaptured = true;
    private bool _adsActive = false;
    private bool _isSprinting = false;

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
        _playerControls.Player.RClick.performed += OnADSEnter;
        _playerControls.Player.RClick.canceled += OnADSExit;

        _playerControls.Player.Sprint.performed += OnSprint;
        _playerControls.Player.Sprint.canceled += OnSprintCancelled;

        _playerControls.Player.DebugReleaseMouse.performed += OnToggleMouse;

        _playerControls.Player.Enable();        
    }

    void OnDisable()
    {
        _playerControls.Player.Movement.performed -= OnMove;
        _playerControls.Player.Movement.canceled -= OnMove;

        _playerControls.Player.Attack.performed -= OnShoot;
        _playerControls.Player.RClick.performed -= OnADSEnter;
        _playerControls.Player.RClick.canceled -= OnADSExit;

        _playerControls.Player.Sprint.performed -= OnSprint;
        _playerControls.Player.Sprint.canceled -= OnSprintCancelled;

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

        _animator.SetTrigger("Shoot");
        _bulletCasing.Emit(1);
        StartCoroutine(FlashSequence());
    }

    private void OnSprint(InputAction.CallbackContext context)
    {
        _isSprinting = true;
    }

    private void OnSprintCancelled(InputAction.CallbackContext context) 
    {
        _isSprinting = false;
    }

    private void OnADSEnter(InputAction.CallbackContext context)
    {

        _adsCamera.Priority = 20;
        _adsActive = true;
        _animator.SetBool("IsAiming", true);
        _aimRig.weight = 1;
        _adsCamera.Target.TrackingTarget = _cameraTarget;

        _reticle.SetADS(true);
    }

    private void OnADSExit(InputAction.CallbackContext context)
    {
        _adsCamera.Priority = 5;
        _adsActive = false;
        _animator.SetBool("IsAiming", false);
        _aimRig.weight = 0;
        _adsCamera.Target.TrackingTarget = _overShouldCamera.transform;

        _reticle.SetADS(false);
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

        _reticle = FindAnyObjectByType<Reticle>();

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
        if (_controller.isGrounded)
        {
            _playerVelocity.y = -0.2f;
        }

        // TODO: Add lerping when going from _moveSpeed, to _sprintSpeed.
        float speed = _adsActive ? _adsSpeed : _isSprinting ? _sprintSpeed : _moveSpeed ;
        _controller.Move((move * speed + _playerVelocity) * Time.deltaTime);

        if (!_adsActive)
        {
            transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(move), Time.deltaTime * 2f);
        }

        Vector3 velocity = _controller.velocity;
        float animVelocity = Mathf.Clamp(new Vector2(velocity.x, velocity.z).magnitude, 0f, speed);
        _animator.SetFloat("Velocity", animVelocity);
    }

    private void RotatePlayer()
    {
        if (!_mouseCaptured)
        {
            return;
        }

        Vector2 lookInput = _playerControls.Player.Look.ReadValue<Vector2>();

        if (_adsActive)
        {
            _targetPitch = Mathf.Clamp(_targetPitch - lookInput.y * _lookSpeed, _maxLookUp, _maxLookDown);
            _targetYaw = _overShouldCamera.transform.eulerAngles.y;
            float yaw = Mathf.SmoothDampAngle(transform.eulerAngles.y, _targetYaw, ref _yawVelocity, _lookDampening);
            float targetPitch = _overShouldCamera.transform.eulerAngles.x;
            _currentPitch = Mathf.SmoothDampAngle(_currentPitch, targetPitch, ref _pitchVelocity, _lookDampening);

            transform.rotation = Quaternion.Euler(0f, yaw, 0f);
        }
        else
        {
            /*
            _targetPitch = Mathf.Clamp(_targetPitch - lookInput.y * _lookSpeed, _maxLookUp, _maxLookDown);
            _targetYaw = _overShouldCamera.transform.eulerAngles.y;

            float yaw = Mathf.SmoothDampAngle(transform.eulerAngles.y, _targetYaw, ref _yawVelocity, _lookDampening);
            float targetPitch = _overShouldCamera.transform.eulerAngles.x;
            _currentPitch = Mathf.SmoothDampAngle(_currentPitch, targetPitch, ref _pitchVelocity, _lookDampening);

            transform.rotation = Quaternion.Euler(0f, yaw, 0f);
            */
        }

        _cameraTarget.localRotation = Quaternion.Euler(_currentPitch, 0f, 0f);
    }

    private IEnumerator FlashSequence()
    {
        _muzzleFlash.enabled = true;
        yield return new WaitForSeconds(0.125f);
        _muzzleFlash.enabled = false;

    }
}
