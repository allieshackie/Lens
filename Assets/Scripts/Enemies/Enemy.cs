using UnityEngine;
using System;
using UnityEngine.AI;

public class Enemy : MonoBehaviour
{   
    [SerializeField] private int _health;
    [SerializeField] private bool _immortal = false;
    [SerializeField] protected float _attackDistance = 2f;
    [SerializeField] protected Animator _animator;

    public event Action OnHit;
    public event Action OnDeath;

    private bool _dying = false;

    protected NavMeshAgent _agent;
    protected GameObject _player;

    virtual protected void Start()
    {
        _agent = GetComponent<NavMeshAgent>();
        _player = GameObject.FindWithTag("Player");
    }

    virtual protected void Update()
    {
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
