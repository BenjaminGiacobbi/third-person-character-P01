using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(CanvasGroup))]
[RequireComponent(typeof(RectTransform))]
public class RadarObject : UIObject
{
    [SerializeField] float _startingOpacity = 0.4f;
    [SerializeField] float _fadeSpeed = 5f;
    [SerializeField] float _distanceFloat = 30f;
    [SerializeField] float _flashTime = 0.2f;
    [SerializeField] Image _flashImage = null;

    // components to modify image
    CanvasGroup _canvasGroup;
    CanvasRenderer _childRenderer;

    Coroutine _fadeRoutine = null;
    Coroutine _flashRoutine = null;


    // caching
    private void Awake()
    {
        _canvasGroup = GetComponent<CanvasGroup>();
        _childRenderer = _flashImage.gameObject.GetComponent<CanvasRenderer>(); 
        _rectTransform = GetComponent<RectTransform>();
    }


    // used to set the object's life and behavior
    public override void ActivateObject(Transform objectTransform, Transform playerTransform, float lifetime)
    {
        base.ActivateObject(objectTransform, playerTransform);
        _canvasGroup.alpha = _startingOpacity;


        if (_fadeRoutine == null)
            _fadeRoutine = StartCoroutine(FadeOutCycle(lifetime));
        if (_flashRoutine == null)
            _flashRoutine = StartCoroutine(FlashCycle());
    }


    // updates position every frame to follow correct object
    void Update()
    {
        base.FollowObject();
    }

    // I based this off of one of Chandler's dev tools. Writing this so he doesn't think I'm trying to be sneaky
    IEnumerator FadeOutCycle(float fadeTime)
    {
        float newOpacity;

        for (float t = 0; t <= _fadeSpeed; t += Time.deltaTime)
        {
            // use lerp to interpolate the scale of the object relative to its distance from the player
            float lerp = Mathf.Lerp(1.5f, 0.5f,
                Mathf.Clamp(Vector3.Distance(_playerTransform.position, _followTransform.position), 0, _distanceFloat) / _distanceFloat);
            _rectTransform.localScale = new Vector3(lerp, lerp, _rectTransform.localScale.z);

            // set opacity
            newOpacity = Mathf.Lerp(_startingOpacity, 0, t / _fadeSpeed);
            _canvasGroup.alpha = newOpacity;

            yield return null;
        }

        _canvasGroup.alpha = _startingOpacity;
        gameObject.SetActive(false);
        _fadeRoutine = null;
    }


    // flashes the object for a short time
    IEnumerator FlashCycle()
    {
        float timer = 0;
        // animates towards full opacity
        while (timer < _flashTime)
        {
            timer = BasicCounter.TowardsTarget(timer, _flashTime, 1f);

            _childRenderer.SetAlpha(timer / _flashTime);

            yield return null;
        }

        // animates towards full transparency
        while (timer > 0)
        {
            timer = BasicCounter.TowardsTarget(timer, 0f, 1f);

            _childRenderer.SetAlpha(timer / _flashTime);

            yield return null;
        }

        _flashRoutine = null;
    }
}
