using System;
using UnityEngine;
using UnityEngine.InputSystem;

public class MoveControl : MonoBehaviour
{
    [SerializeField] private float _moveSpeed;
    [SerializeField] private float _rotateBodySpeed;
    [SerializeField] private bool _isVisibleCursore;

    private event Action OnPlayerUpdate;
    private CharacterController _controller;
    private UserInputActions _input;
    private bool _isSubscribeMove = false;
    private Vector3 _cameraForward;
    private Vector2 _moveDirection;


    private void Awake()
    {
        _input = new UserInputActions();
        _controller = GetComponent<CharacterController>();
    }

    private void OnEnable()
    {
        Cursor.visible = _isVisibleCursore;

        _input.Enable();

        _input.Player.Move.performed += MovementEnable;
        _input.Player.Move.canceled += MovementDisable;
    }

    private void OnDisable()
    {
        _input.Player.Move.performed -= MovementEnable;
        _input.Player.Move.canceled -= MovementDisable;

        _input.Disable();
    }

    private void LateUpdate()
    {
        OnPlayerUpdate?.Invoke();
    }

    private void Move()
    {
        Vector3 movementDirection = _cameraForward * _moveDirection.y + Camera.main.transform.right * _moveDirection.x;
        movementDirection.y = 0;

        _controller.Move(movementDirection.normalized * _moveSpeed * Time.deltaTime);
    }

    private void Rotate()
    {
        _cameraForward = Camera.main.transform.forward;
        _cameraForward.y = 0;
        _cameraForward.Normalize();

        Vector3 inputDirection = new Vector3(_moveDirection.x, 0, _moveDirection.y).normalized;

        if (inputDirection.magnitude >= 0.1f)
        {
            Vector3 combinedDirection = Quaternion.LookRotation(_cameraForward) * inputDirection;
            float targetAngle = Mathf.Atan2(combinedDirection.x, combinedDirection.z) * Mathf.Rad2Deg;
            Quaternion targetRotation = Quaternion.Euler(0, targetAngle, 0);
            _controller.transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * _rotateBodySpeed);
        }
    }

    private void MovementEnable(InputAction.CallbackContext context)
    {
        _moveDirection = context.action.ReadValue<Vector2>();
        _moveDirection *= _moveSpeed;

        if (_isSubscribeMove == false)
        {
            _isSubscribeMove = true;
            OnPlayerUpdate += Rotate;
            OnPlayerUpdate += Move;
        }
    }

    private void MovementDisable(InputAction.CallbackContext context)
    {
        _isSubscribeMove = false;
        OnPlayerUpdate -= Rotate;
        OnPlayerUpdate -= Move;
    }

}
