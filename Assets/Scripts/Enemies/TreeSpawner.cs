using UnityEngine;
using UnityEngine.Splines;

public class TreeSpawner : MonoBehaviour
{
    [SerializeField] private Transform _spawnPoint;
    [SerializeField] private SplineContainer _spline;
    [SerializeField] private GameObject _enemyPrefab;

    public void SpawnEnemy(float randomTime)
    {
       Invoke(nameof(Spawn), randomTime);
    }

    private void Spawn()
    {
        GameObject obj = Instantiate(_enemyPrefab, _spawnPoint.position, Quaternion.identity);
        Wrymm wrymm = obj.GetComponent<Wrymm>();
        if (wrymm)
        {
            wrymm.Init(_spline);
        }
    }
}
