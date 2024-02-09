using UnityEngine;

public class EnemyMover : MonoBehaviour
{
    private const string Speed = "Speed";

    [SerializeField] private Vector3 _movementDirrection;
    [SerializeField] private float _movementSpeed;
    private Animator _animator;

    private void Start()
    {
        _animator = GetComponent<Animator>();
        transform.rotation = Quaternion.LookRotation(_movementDirrection);
    }

    private void Update()
    {
        Move();
    }

    public void SetDirection(Vector3 direction)
    {
        _movementDirrection = direction;
    }

    private void Move()
    {
        transform.Translate(Vector3.forward * _movementSpeed * Time.deltaTime);
        _animator.SetFloat(Speed, _movementSpeed);
    }
}