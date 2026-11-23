using UnityEngine;
using UnityEngine.Splines;

public class Wrymm : Enemy
{
   public enum WrymmState
    {
        Idle,
        Spawn,
        Waiting,
        Chasing,
        Dodging,
        AttackingPlayer
    }

    private WrymmState _state;

    // Treespline could have a parent class like "EnemySpawn" that, if it exists,
    // it means we wait to activate the nav mesh agent until the enemy finishes "emerging"
    private TreeSpline _treeSplineSpawn;

    override protected void Start()
    {
        base.Start();
        SetState(WrymmState.Idle);
    }

    public void Init(SplineContainer spline)
    {
        if (TryGetComponent(out _treeSplineSpawn))
        {
            _treeSplineSpawn.OnComplete += SpawnComplete;
            _treeSplineSpawn.Init(spline);
        }
    }

    private void SetState(WrymmState state)
    {
        _state = state;
    }

    private void SpawnComplete()
    {
        _agent.enabled = true;
        SetState(WrymmState.Chasing);
    }

    void ChasePlayer()
    {
        _agent.SetDestination(_player.transform.position);
        if (_agent.remainingDistance <= _attackDistance && !_agent.pathPending)
        {
            // Attack
            Debug.Log("Reached attacking distance");
        }
    }

    override protected void Update()
    {
        switch(_state)
        {
            case WrymmState.Spawn:
                break;
            case WrymmState.Waiting:
                break;
            case WrymmState.Chasing:
                ChasePlayer();
                break;
            case WrymmState.Dodging:
                break;
            case WrymmState.AttackingPlayer:
                break;
            default:
                break;
        }

        base.Update();
    }

}
