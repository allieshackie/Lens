using UnityEngine;
using System;
using UnityEngine.AI;

public class Enemy : MonoBehaviour
{
    public enum EnemyState
    {
        Spawn,
        Waiting,
        Chasing,
        Dodging,
        AttackingPlayer
    }
    [SerializeField] private int _health;
    [SerializeField] private bool _immortal = false;
    [SerializeField] private float _attackDistance = 2f;
    [SerializeField] private Animator _animator;

    public event Action OnHit;
    public event Action OnDeath;

    private bool _dying = false;

    private NavMeshAgent _agent;
    private EnemyState _state;
    private GameObject _player;

    // Treespline could have a parent class like "EnemySpawn" that, if it exists,
    // it means we wait to activate the nav mesh agent until the enemy finishes "emerging"
    private TreeSpline _treeSplineSpawn; 

    private void Start()
    {
        _agent = GetComponent<NavMeshAgent>();
        _player = GameObject.FindWithTag("Player");

        SetState(EnemyState.Spawn);

        if (TryGetComponent(out _treeSplineSpawn))
        {
            _agent.enabled = false;
            _treeSplineSpawn.OnComplete += SpawnComplete;
        }
    }

    private void SetState(EnemyState state)
    {
        _state = state;
    }

    private void SpawnComplete()
    {
        _agent.enabled = true;
        SetState(EnemyState.Chasing);
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

    private void Update()
    {
        switch(_state)
        {
            case EnemyState.Spawn:
                break;
            case EnemyState.Waiting:
                break;
            case EnemyState.Chasing:
                ChasePlayer();
                break;
            case EnemyState.Dodging:
                break;
            case EnemyState.AttackingPlayer:
                break;
            default:
                break;
        }

        _animator.SetFloat("Velocity", _agent.velocity.magnitude);
    }

    public void TakeHit()
    {
        if (_dying)
        {
            return; 
        }

        if (!_immortal)
        {
            _health--;
        }

        _animator.SetTrigger("IsAttacked");

        OnHit?.Invoke();

        if (_health <= 0)
        {
            Die();
        }
    }

    private void Die()
    {
        _dying = true;
        OnDeath?.Invoke();

        Destroy(gameObject);
    }
}
