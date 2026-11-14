using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerMovement : MonoBehaviour
{
    [SerializeField] private float _moveSpeed = 5.0f;
    [SerializeField] private float _strafeSpeed = 3.0f;
    [SerializeField] private float _gravity = -9.81f;

    [SerializeField] private float _lookSpeed = 0.15f;
    [SerializeField] private Transform _cameraTarget;

    [SerializeField] private Camera _playerCamera;
    [SerializeField] private Transform _gunTransform;
    [SerializeField] private float _maxShootRange = 100f;
    [SerializeField] private float _gunDamage = 1f;

    private CharacterController _controller;
    private Vector2 _moveVector;
    private Vector3 _playerVelocity;
    private float _xRotation = 0f;
    private Player_Controls _playerControls;

    void Awake()
    {
        _playerControls = new Player_Controls();
    }

    void OnEnable()
    {
        _playerControls.Player.Movement.performed += OnMove;
        _playerControls.Player.Movement.canceled += OnMove;

        _playerControls.Player.Attack.performed += OnShoot;

        _playerControls.Player.Enable();        
    }

    void OnDisable()
    {
        _playerControls.Player.Movement.performed -= OnMove;
        _playerControls.Player.Movement.canceled -= OnMove;

        _playerControls.Player.Attack.performed -= OnShoot;

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

    void Start()
    {
        _controller = GetComponent<CharacterController>();
    }

    private void Update()
    {
        MovePlayer();
    }

    void LateUpdate()
    {
        RotatePlayer();
    }

    private void MovePlayer()
    {
        Vector3 move = transform.right * _moveVector.x + transform.forward * _moveVector.y;
        _controller.Move(move * _moveSpeed * Time.deltaTime);

        // Player was jittering a bunch if I didn't apply some downward grav
        _playerVelocity.y += _gravity * Time.deltaTime;
        _controller.Move(_playerVelocity * Time.deltaTime);
    }

    private void RotatePlayer()
    {
        Vector2 lookInput = _playerControls.Player.Look.ReadValue<Vector2>();
        float mouseX = lookInput.x * _lookSpeed;
        float mouseY = lookInput.y * _lookSpeed;

        _xRotation -= mouseY;
        _xRotation = Mathf.Clamp(_xRotation, -90f, 90f);

        transform.Rotate(Vector3.up * mouseX);
        // This lets us aim a bit up and down without actually rotating player model
        _cameraTarget.localRotation = Quaternion.Euler(_xRotation, 0f, 0f);
    }
}
