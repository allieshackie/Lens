using System;
using UnityEngine;

public class Reticle : MonoBehaviour
{
    [SerializeField] private Vector2 _normalSize = new Vector2(200f, 200f);
    [SerializeField] private Vector2 _adsSize = new Vector2(300f, 300f);
    [SerializeField] private float _lerpTime = 0.25f;

    private RectTransform _reticleImage;
    private bool _isAds = false;
    private float _progress;

    private void Awake()
    {
        _reticleImage = GetComponent<RectTransform>();
        _reticleImage.sizeDelta = _normalSize;
    }

    public void SetADS(bool isAds)
    {
        _isAds = isAds;
    }

    private void Update()
    {
        if ((_isAds && _reticleImage.sizeDelta == _adsSize) || (!_isAds && _reticleImage.sizeDelta == _normalSize))
        {
            return;
        }
        float targetProgress = _isAds ? 1f : 0f;
        float step = Time.deltaTime / _lerpTime;
        _progress = Mathf.MoveTowards(_progress, targetProgress, step);

        float easedT = Mathf.SmoothStep(0f, 1f, _progress);
        _reticleImage.sizeDelta = Vector2.Lerp(_normalSize, _adsSize, easedT);
    }
}
