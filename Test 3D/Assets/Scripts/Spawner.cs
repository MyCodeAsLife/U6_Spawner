using UnityEngine;

public class Spawner : MonoBehaviour
{
    [SerializeField] private Transform[] _spawnPonts;
    [SerializeField] private Enemy _enemy;
    [SerializeField] private float _timeBetweenSpawn = 2;
    [SerializeField] private float _lifetime = 20;

    private float _minValue = -1;
    private float _maxValue = 1;

    private void Start()
    {
        InvokeRepeating(nameof(EnemySpawn), _timeBetweenSpawn, _timeBetweenSpawn);
    }

    private void EnemySpawn()
    {
        int randomIndex = Random.Range(0, _spawnPonts.Length);
        Vector3 direction = new Vector3(Random.Range(_minValue, _maxValue), 0, Random.Range(_minValue, _maxValue));
        Enemy enemy = Instantiate(_enemy, _spawnPonts[randomIndex].position, Quaternion.identity);

        if (enemy.TryGetComponent(out EnemyMover enemyMover))
            enemyMover.SetDirection(direction);

        Destroy(enemy, _lifetime);
    }
}