using System;
using UnityEngine;
using UnityEngine.InputSystem;

/*
 * Куда прикрепить этот скрипт?
 *
 * Добавить информацию по работе со слоями?
 *
 * Прикрепляем к камере SphereCollider и у него включаем isTrigger, чтобы его не обсчитывал физический движок.
 *
 * ВАЖНО! Камера к которой это будет применятся, должна иметь тэг MainCamera.
 * При создании проекта, первая камера на сцене по умолчанию имеет данный тэг.
 * Тэг можно поменять, выделив камеру на сцене или в Hierarchy, затем Inspector -> Tag -> MainCamera
 * 
 * - Камера от 3го лица.
 * Для прикрепления камеры на позицию от 3го лица, необходимо на персонажа 
 * добавить точку на нужный вам уровень, а затем к данной точке прикрепить камеру.
 * Затем в поле _target, передать данную точку. В последствии камера будет вращатся вокруг данной точки.
 * 
 * - Камера от 1го лица.
 * Просто прикрепите камеру к персонажу, переместите ее в необходимое положен6ие и в поле _target передайте саму камеру.
 * Затем нужно инвертировать управление по осям X и Y следующим образом:            ( Добавить инвертирование через делегат)
 * 
*/

public class CameraControl : MonoBehaviour
{
    [SerializeField] private Transform _target;             // Центр вращения
    [SerializeField] private float _speedX = 10f;           // Скорость перемещения камеры по X  X и Y переделать в 1?
    [SerializeField] private float _speedY = 10f;           // Скорость перемещения камеры по Y
    [SerializeField] private float _maxAngleY = 50f;        // Максимальный угол по оси Y
    [SerializeField] private float _minAngleY = -70f;       // Минимальный угол по оси Y
    [SerializeField] private float _hideDistance = 0.7f;    // Минимальная дистанция камеры(от игрока) для скрывания игрока
                                                            // Добавить ограничение углов по оси X
    [SerializeField] private LayerMask _obstacles;          // Какой слой не пересекать камерой (камера не позволит переместить себя в позицию где между ней и персонажем будет находится объект с данного слоя)
    [SerializeField] private LayerMask _noPlayer;           // Все слои кроме того где игрок
    private LayerMask _defaultVisibleLayers;                // Маски видимости камеры по умолчанию

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
    // Также добавить минимальную дистанцию и скролинг

    private void Start()
    {
        // Для Raycast (это вообще задействованно?)
        //_localPosition = transform.position;                                    // Сохраняем позицию камеры
        //_maxDistance = Vector3.Distance(transform.position, _target.position);  // Высчитываем дистанцию между двумя объектами и записываем
        //_minDistance =
        _defaultVisibleLayers = Camera.main.cullingMask;
    }

    private void Awake()
    {
        _input = new UserInputActions();
    }

    private void OnEnable()
    {
        _virtualRotation = _target.rotation;            // Создаем виртуальную точку вращения

        _input.Enable();
        _input.Player.Look.performed += OnRotation;     // Подписываем метод взятия значений для вращения
        _input.Player.Look.canceled += OffRotation;     // Подписываем метод взятия значений для вращения
    }

    private void OnDisable()
    {
        _input.Player.Look.performed -= OnRotation;     // Отписываем метод взятия значений для вращения
        _input.Player.Look.canceled -= OffRotation;     // Подписываем метод взятия значений для вращения
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

        // Добавить переключатель инвертирования по обоим осям
        _vertical -= _speedY * _lookDirection.y * Time.deltaTime;       // -= Инвертирование по оси Y
        _vertical = Math.Clamp(_vertical, _minAngleY, _maxAngleY);      // Расчет поворота с учетом ограничений по оси Y

        // Добавить расчет ограничения по оси X
        _horizontal += _speedX * _lookDirection.x * Time.deltaTime;

        _virtualRotation = Quaternion.Euler(_vertical, _horizontal, 0f);
    }

    private void ObstaclesReact()
    {
        const float DISTANCE = 0.3f;
        float distance = Vector3.Distance(transform.position, _target.position);  // Дистанция между камерой и и точкой вращения. Дублируется?
        RaycastHit hit;
        //_tempPosition = transform.position;

        if (Physics.Raycast(_target.position, transform.position - _target.position, out hit, _maxDistance, _obstacles)/* && distance > _minDistance*/)
        {
            // Если между камерой и точкой крепления есть объект, то камеру переместить перед объектом
            //transform.position = hit.point;

            transform.position = hit.point;
            //float offset = Vector3.Distance(transform.position, hit.point) + Camera.main.GetComponent<SphereCollider>().radius;
            //transform.position += transform.forward * offset;
        }
        else if (distance < _maxDistance)
        {
            transform.position -= transform.forward * DISTANCE;
        }
        //else if (distance < _maxDistance && Physics.Raycast(transform.position, -transform.forward, DISTANCE, _obstacles) == false)   // Плавное отодвигание камеры, когда отходим от препятствия
        //{
        //    // Если сзади камеры нет припятствия(_obstacles), и дистанция(между камерой и её креплением)
        //    // меньше максимальной то сдвигаем камеру немного назад.
        //    transform.position -= transform.forward * DISTANCE;
        //}
        //else if (isOverlap)
        //{
        //    transform.position += transform.forward * 0.3f;
        //}
    }
    //----------------------------------------------------------------------------------------------------------------
    //private Action OnOverlap;               // Перенести
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
    //    if ((_obstacles.value & (1 << other.gameObject.layer)) != 0)    // Если слой контактируемого объекта входит в _obstacles
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
    //    //else if (distance < _maxDistance && Physics.Raycast(transform.position, -transform.forward, DISTANCE, _obstacles) == false)   // Плавное отодвигание камеры, когда отходим от препятствия
    //    //{
    //    //    // Если сзади камеры нет припятствия(_obstacles), и дистанция(между камерой и её креплением)
    //    //    // меньше максимальной то сдвигаем камеру немного назад.
    //    //    transform.position -= transform.forward * DISTANCE;
    //    //}
    //}
    //----------------------------------------------------------------------------------------------------------------
    private void PlayerReact()
    {
        // Продублирована в ObstaclesReact()
        float distance = Vector3.Distance(transform.position, _target.position);  // Дистанция между камерой и и точкой вращения. Дублируется?

        if (distance < _hideDistance)
            Camera.main.cullingMask = _noPlayer;
        else
            Camera.main.cullingMask = _defaultVisibleLayers;
    }

    private void LateUpdate()
    {
        //transform.position = _localPosition;

        // Добавить переключатель для отключения данной функции (подойдет для гонок или авиасимуляторов)
        //if (_target.rotation != _virtualRotation /*&& isOverlap*/)               // Таким образом делаем вращение камеры независимым от вращения персонажа.
            _target.transform.rotation = _virtualRotation;

        // new
        OnPlayerUpdate?.Invoke();

        //// В начале кадра присваиваем положение камере, расчитанное в прошлом кадре.
        //transform.position = _target.TransformPoint(_localPosition);    // Трансформируем локальную позицию в глобальную и передаем текущему объекту(камера)

        //// Запоминаем положение камеры, для его присваивания в следующем кадре.
        //_localPosition = _target.InverseTransformPoint(transform.position); // Берем глобальную позицию и трансформируем ее в локальную, и записываем

        ObstaclesReact();
        PlayerReact();

        //OnOverlap?.Invoke();
    }
}
