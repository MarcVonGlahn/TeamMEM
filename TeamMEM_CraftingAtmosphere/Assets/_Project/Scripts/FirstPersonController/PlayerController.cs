using Cinemachine;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    [Header("Input Action Asset")]
    [SerializeField] InputActionAsset inputActions;

    [Header("General Parameters")]
    [SerializeField] bool hideCursor = true;

    [Header("Movement Parameters")]
    [SerializeField] float walkSpeed = 1.0f;
    [SerializeField] float walkStepIntervall = 0.5f;
    [SerializeField] float sprintModifier = 2.0f;
    [SerializeField] float sprintStepIntervall = 0.3f;
    [SerializeField] float velocityThreshold = 2.0f;
    [SerializeField] float jumpForce = 2.0f;
    [SerializeField] float gravity = 9.81f;

    [Header("Look Parameters")]
    [SerializeField] CinemachineFreeLook freeLookCam;
    [SerializeField] float mouseSensitivity = 2.0f;
    [SerializeField] float upDownRange = 80;

    private bool _lockMovement = false;

    private bool _isMoving = false;

    private CharacterController _characterController;

    private InputAction _moveAction;
    private InputAction _lookAction;
    private InputAction _jumpAction;
    private InputAction _sprintAction;

    private InputAction _quitGameAction;

    private float _verticalRotation;
    private float _nextStepTime;

    private Vector2 _moveInput;
    private Vector2 _lookInput;

    private Vector3 _currentMovement;

    // Only done at the intro
    public bool LockMovement { get => _lockMovement; set => _lockMovement = value; }

    private void Awake()
    {
        _characterController = GetComponent<CharacterController>();

        _moveAction = inputActions.FindActionMap("Player").FindAction("Move");
        _lookAction = inputActions.FindActionMap("Player").FindAction("Look");
        _jumpAction = inputActions.FindActionMap("Player").FindAction("Jump");
        _sprintAction = inputActions.FindActionMap("Player").FindAction("Sprint");

        _quitGameAction = inputActions.FindActionMap("Player").FindAction("QuitGame");

        _moveAction.performed += context => _moveInput = context.ReadValue<Vector2>();
        _moveAction.canceled += context => _moveInput = Vector2.zero;

        _lookAction.performed += context => _lookInput = context.ReadValue<Vector2>();
        _lookAction.canceled += context => _lookInput = Vector2.zero;

        _quitGameAction.performed += QuitGameAction_performed;

        Cursor.visible = hideCursor ? false : true;
        Cursor.lockState = CursorLockMode.Locked;
    }


    private void QuitGameAction_performed(InputAction.CallbackContext obj)
    {
        Application.Quit();
    }

    private void OnEnable()
    {
        _moveAction.Enable();
        _lookAction.Enable();
        _jumpAction.Enable();
        _sprintAction.Enable();

        _quitGameAction.Enable();
    }


    private void OnDisable()
    {
        _moveAction.Disable();
        _lookAction.Disable();
        _jumpAction.Disable();
        _sprintAction.Disable();

        _quitGameAction.Disable();
    }


    private void Update()
    {
        if (_lockMovement)
            return;

        HandleMovement();
        HandleRotation();
        HandleFootSteps();
    }

    private void HandleMovement()
    {
        float speedModifier = _sprintAction.ReadValue<float>() > 0 ? sprintModifier : 1f;

        float verticalSpeed = _moveInput.y * walkSpeed * speedModifier;
        float horizontalSpeed = _moveInput.x * walkSpeed * speedModifier;

        Vector3 horizontalMovement = new Vector3(horizontalSpeed, 0, verticalSpeed);
        horizontalMovement = transform.rotation * horizontalMovement;

        HandleGravityAndJumping();

        _currentMovement.x = horizontalMovement.x;
        _currentMovement.z = horizontalMovement.z;

        _characterController.Move(_currentMovement * Time.deltaTime);

        _isMoving = _moveInput.x != 0 || _moveInput.y != 0;
    }


    private void HandleGravityAndJumping()
    {
        if (_characterController.isGrounded)
        {
            _currentMovement.y = -0.5f;

            if (_jumpAction.triggered)
            {
                _currentMovement.y = jumpForce;
            }
        }
        else
        {
            _currentMovement.y -= gravity * Time.deltaTime;
        }
    }


    private void HandleRotation()
    {
        float mouseRotationX = _lookInput.x * mouseSensitivity;

        transform.Rotate(0, mouseRotationX, 0);

        _verticalRotation -= _lookInput.y * mouseSensitivity;
        _verticalRotation = Mathf.Clamp(_verticalRotation, -upDownRange, upDownRange);

        freeLookCam.transform.localRotation = Quaternion.Euler(_verticalRotation, 0, 0);
    }


    private void HandleFootSteps()
    {
        float currentStepIntervall = _sprintAction.ReadValue<float>() > 0 ? sprintStepIntervall : walkStepIntervall;

        if(_characterController.isGrounded
            && _isMoving
            && Time.time > _nextStepTime
            && _characterController.velocity.magnitude > velocityThreshold)
        {
            _nextStepTime = Time.time + currentStepIntervall;
        }
    }
}
