using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class Angler : Enemy
{
    public enum AnglerState
    {
        FindTrees,
        Approach,
        Summoning,
        Idle,
    };

    [SerializeField] private float _summoningTime;
    [SerializeField] private float _summonRange;

    private AnglerState _state;
    private float _timer;

    private Transform _targetDestination;

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, _summonRange);
    }

    override protected void Start()
    {
        base.Start();
        SetState(AnglerState.FindTrees);
    }

    private void SetState(AnglerState state)
    {
        _state = state;
        if (_state == AnglerState.FindTrees)
        {
            FindTree();
        }
    }

    private void FindTree()
    {
        TreeSpawner[] items = FindObjectsByType<TreeSpawner>();
        float distance = float.MaxValue;
        foreach(TreeSpawner item in items)
        {
            float newDistance = Vector3.Distance(transform.position, item.transform.position);
            if (newDistance < distance)
            {
                distance = newDistance;
                _targetDestination = item.transform;
            }
        }

        _agent.SetDestination(_targetDestination.position);
        SetState(AnglerState.Approach);
    }

    void GoToTree()
    {
        if (_agent.remainingDistance <= _attackDistance && !_agent.pathPending && _agent.pathStatus == NavMeshPathStatus.PathComplete)
        {
            // turn to face player? 
            SetState(AnglerState.Summoning);
            _timer = _summoningTime;
            _animator.SetBool("IsLuring", true);   
        }
    }

    void Summoning()
    {
        _timer -= Time.deltaTime;
        if (_timer <= 0.0f)
        {
            _animator.SetBool("IsLuring", false);
            Collider[] hitColliders = Physics.OverlapSphere(transform.position, _summonRange);
            foreach (Collider col in hitColliders)
            {
                if (col.TryGetComponent(out TreeSpawner treeSpawner))
                {
                    treeSpawner.SpawnEnemy(UnityEngine.Random.Range(.5f, 2f));
                }
            }
            SetState(AnglerState.Idle);
        }
    }

    override protected void Update()
    {
        switch(_state)
        {
            case AnglerState.Approach:
                GoToTree();
                break;
            case AnglerState.Summoning:
                Summoning();
                break;
            default:
                break;
        }

        base.Update();
    }
    
}
