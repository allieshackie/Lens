using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerMovement : MonoBehaviour
{
    [SerializeField] float _moveSpeed = 5.0f;
    [SerializeField] float _strafeSpeed = 3.0f;
    [SerializeField] float _gravity = -9.81f;

    [SerializeField] float _lookSpeed = 0.15f;

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

        _playerControls.Player.Enable();        
    }

    void OnDisable()
    {
        _playerControls.Player.Movement.performed -= OnMove;
        _playerControls.Player.Movement.canceled -= OnMove;

        _playerControls.Player.Disable();    
    }

    private void OnMove(InputAction.CallbackContext context)
    {
        _moveVector = context.action.ReadValue<Vector2>();
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
    }
}
