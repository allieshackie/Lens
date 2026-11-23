using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Splines;

public class TreeSpline : MonoBehaviour
{
    [SerializeField] private SplineAnimate _splineAnimate;
    [SerializeField] private SplineContainer _spline;
    [SerializeField] private float _waitAtBottom = 2f;

    public event Action OnComplete;

    private bool _hasReachedBottom;

    void Awake()
    {
        _splineAnimate.PlayOnAwake = false;
        _splineAnimate.Completed += OnDescentComplete;
    }

    public void Init(SplineContainer spline)
    {
        _spline = spline;
        _splineAnimate.Container = spline;
        StartDescent();
    }

    private void StartDescent()
    {
        if (_hasReachedBottom) 
        {
            return;
        }

        _hasReachedBottom = true;
        _splineAnimate.Restart(true);
    }

    void OnDescentComplete()
    {
        StartCoroutine(FinishDescent());
    }

    IEnumerator FinishDescent()
    {
        _splineAnimate.enabled = false;

        yield return new WaitForSeconds(_waitAtBottom);

        OnComplete?.Invoke();
    }

    void OnDestroy()
    {
        if (_splineAnimate != null)
        {   
            _splineAnimate.Completed -= OnDescentComplete;
        }
    }

}
