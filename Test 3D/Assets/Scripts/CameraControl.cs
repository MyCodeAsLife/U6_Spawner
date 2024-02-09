using System;
using UnityEngine;
using UnityEngine.InputSystem;

/*
 * ���� ���������� ���� ������?
 *
 * �������� ���������� �� ������ �� ������?
 *
 * ����������� � ������ SphereCollider � � ���� �������� isTrigger, ����� ��� �� ���������� ���������� ������.
 *
 * �����! ������ � ������� ��� ����� ����������, ������ ����� ��� MainCamera.
 * ��� �������� �������, ������ ������ �� ����� �� ��������� ����� ������ ���.
 * ��� ����� ��������, ������� ������ �� ����� ��� � Hierarchy, ����� Inspector -> Tag -> MainCamera
 * 
 * - ������ �� 3�� ����.
 * ��� ������������ ������ �� ������� �� 3�� ����, ���������� �� ��������� 
 * �������� ����� �� ������ ��� �������, � ����� � ������ ����� ���������� ������.
 * ����� � ���� _target, �������� ������ �����. � ����������� ������ ����� �������� ������ ������ �����.
 * 
 * - ������ �� 1�� ����.
 * ������ ���������� ������ � ���������, ����������� �� � ����������� �������6�� � � ���� _target ��������� ���� ������.
 * ����� ����� ������������� ���������� �� ���� X � Y ��������� �������:            ( �������� �������������� ����� �������)
 * 
*/

public class CameraControl : MonoBehaviour
{
    [SerializeField] private Transform _target;             // ����� ��������
    [SerializeField] private float _speedX = 10f;           // �������� ����������� ������ �� X  X � Y ���������� � 1?
    [SerializeField] private float _speedY = 10f;           // �������� ����������� ������ �� Y
    [SerializeField] private float _maxAngleY = 50f;        // ������������ ���� �� ��� Y
    [SerializeField] private float _minAngleY = -70f;       // ����������� ���� �� ��� Y
    [SerializeField] private float _hideDistance = 0.7f;    // ����������� ��������� ������(�� ������) ��� ��������� ������
                                                            // �������� ����������� ����� �� ��� X
    [SerializeField] private LayerMask _obstacles;          // ����� ���� �� ���������� ������� (������ �� �������� ����������� ���� � ������� ��� ����� ��� � ���������� ����� ��������� ������ � ������� ����)
    [SerializeField] private LayerMask _noPlayer;           // ��� ���� ����� ���� ��� �����
    private LayerMask _defaultVisibleLayers;                // ����� ��������� ������ �� ���������

    private UserInputActions _input;
    private event Action OnPlayerUpdate;
    private Quaternion _virtualRotation;
    private Vector2 _lookDirection;
    private float _horizontal = 0f;
    private float _vertical = 0f;
    private bool _isSubscribeLook = false;

    private Vector3 _tempPosition;
    private float _currentYRotation;
    [SerializeField] private float _maxDistance = 4f;
    //[SerializeField] private float _minDistance = 0.2f;
    // ����� �������� ����������� ��������� � ��������

    private void Start()
    {
        // ��� Raycast (��� ������ ��������������?)
        //_localPosition = transform.position;                                    // ��������� ������� ������
        //_maxDistance = Vector3.Distance(transform.position, _target.position);  // ����������� ��������� ����� ����� ��������� � ����������
        //_minDistance =
        _defaultVisibleLayers = Camera.main.cullingMask;
    }

    private void Awake()
    {
        _input = new UserInputActions();
    }

    private void OnEnable()
    {
        _virtualRotation = _target.rotation;            // ������� ����������� ����� ��������

        _input.Enable();
        _input.Player.Look.performed += OnRotation;     // ����������� ����� ������ �������� ��� ��������
        _input.Player.Look.canceled += OffRotation;     // ����������� ����� ������ �������� ��� ��������
    }

    private void OnDisable()
    {
        _input.Player.Look.performed -= OnRotation;     // ���������� ����� ������ �������� ��� ��������
        _input.Player.Look.canceled -= OffRotation;     // ����������� ����� ������ �������� ��� ��������
        _input.Disable();
    }

    private void OnRotation(InputAction.CallbackContext context)
    {
        _lookDirection = context.action.ReadValue<Vector2>();

        if (_isSubscribeLook == false)
        {
            _isSubscribeLook = true;
            OnPlayerUpdate += VirtualRotation;
        }
    }

    private void OffRotation(InputAction.CallbackContext context)
    {
        _isSubscribeLook = false;
        OnPlayerUpdate -= VirtualRotation;
    }

    private void VirtualRotation()
    {
        if (_lookDirection.sqrMagnitude < 0.1f)
            return;

        // �������� ������������� �������������� �� ����� ����
        _vertical -= _speedY * _lookDirection.y * Time.deltaTime;       // -= �������������� �� ��� Y
        _vertical = Math.Clamp(_vertical, _minAngleY, _maxAngleY);      // ������ �������� � ������ ����������� �� ��� Y

        // �������� ������ ����������� �� ��� X
        _horizontal += _speedX * _lookDirection.x * Time.deltaTime;

        _virtualRotation = Quaternion.Euler(_vertical, _horizontal, 0f);
    }

    private void ObstaclesReact()
    {
        const float DISTANCE = 0.3f;
        float distance = Vector3.Distance(transform.position, _target.position);  // ��������� ����� ������� � � ������ ��������. �����������?
        RaycastHit hit;
        //_tempPosition = transform.position;

        if (Physics.Raycast(_target.position, transform.position - _target.position, out hit, _maxDistance, _obstacles)/* && distance > _minDistance*/)
        {
            // ���� ����� ������� � ������ ��������� ���� ������, �� ������ ����������� ����� ��������
            //transform.position = hit.point;

            transform.position = hit.point;
            //float offset = Vector3.Distance(transform.position, hit.point) + Camera.main.GetComponent<SphereCollider>().radius;
            //transform.position += transform.forward * offset;
        }
        else if (distance < _maxDistance)
        {
            transform.position -= transform.forward * DISTANCE;
        }
        //else if (distance < _maxDistance && Physics.Raycast(transform.position, -transform.forward, DISTANCE, _obstacles) == false)   // ������� ����������� ������, ����� ������� �� �����������
        //{
        //    // ���� ����� ������ ��� �����������(_obstacles), � ���������(����� ������� � � ����������)
        //    // ������ ������������ �� �������� ������ ������� �����.
        //    transform.position -= transform.forward * DISTANCE;
        //}
        //else if (isOverlap)
        //{
        //    transform.position += transform.forward * 0.3f;
        //}
    }
    //----------------------------------------------------------------------------------------------------------------
    //private Action OnOverlap;               // ���������
    //private Collider _collider;
    //private bool isOverlap = false;

    //private void OnTriggerEnter(Collider other)
    //{
    //    if (isOverlap == false && (_obstacles.value & (1 << other.gameObject.layer)) != 0)
    //    {
    //        isOverlap = true;
    //        //_collider = other;
    //        //OnOverlap += CorrectingPosition;
    //    }
    //}

    //private void OnTriggerExit(Collider other)
    //{
    //    if ((_obstacles.value & (1 << other.gameObject.layer)) != 0)    // ���� ���� ��������������� ������� ������ � _obstacles
    //    {
    //        isOverlap = false;
    //        //OnOverlap -= CorrectingPosition;
    //        //_collider = null;
    //    }
    //}

    //private void CorrectingPosition()
    //{
    //    //if (_collider != null)
    //    transform.position += transform.forward * 0.01f/*Camera.main.GetComponent<SphereCollider>().radius*/;
    //    //else if (distance < _maxDistance && Physics.Raycast(transform.position, -transform.forward, DISTANCE, _obstacles) == false)   // ������� ����������� ������, ����� ������� �� �����������
    //    //{
    //    //    // ���� ����� ������ ��� �����������(_obstacles), � ���������(����� ������� � � ����������)
    //    //    // ������ ������������ �� �������� ������ ������� �����.
    //    //    transform.position -= transform.forward * DISTANCE;
    //    //}
    //}
    //----------------------------------------------------------------------------------------------------------------
    private void PlayerReact()
    {
        // �������������� � ObstaclesReact()
        float distance = Vector3.Distance(transform.position, _target.position);  // ��������� ����� ������� � � ������ ��������. �����������?

        if (distance < _hideDistance)
            Camera.main.cullingMask = _noPlayer;
        else
            Camera.main.cullingMask = _defaultVisibleLayers;
    }

    private void LateUpdate()
    {
        //transform.position = _localPosition;

        // �������� ������������� ��� ���������� ������ ������� (�������� ��� ����� ��� ���������������)
        //if (_target.rotation != _virtualRotation /*&& isOverlap*/)               // ����� ������� ������ �������� ������ ����������� �� �������� ���������.
            _target.transform.rotation = _virtualRotation;

        // new
        OnPlayerUpdate?.Invoke();

        //// � ������ ����� ����������� ��������� ������, ����������� � ������� �����.
        //transform.position = _target.TransformPoint(_localPosition);    // �������������� ��������� ������� � ���������� � �������� �������� �������(������)

        //// ���������� ��������� ������, ��� ��� ������������ � ��������� �����.
        //_localPosition = _target.InverseTransformPoint(transform.position); // ����� ���������� ������� � �������������� �� � ���������, � ����������

        ObstaclesReact();
        PlayerReact();

        //OnOverlap?.Invoke();
    }
}
