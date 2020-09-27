using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(RectTransform))]
public class UIObject : MonoBehaviour
{
    // components to modify behavior
    protected Camera _activeCamera = null;
    protected Transform _followTransform = null;
    protected Transform _playerTransform = null;
    protected RectTransform _rectTransform = null;


    public virtual void ActivateObject(Transform followTransform, Transform playerTransform = null, float lifeTime = 0)
    {
        _activeCamera = Camera.main;
        _followTransform = followTransform;
        _playerTransform = playerTransform;
        if (lifeTime > 0)
            Destroy(this, lifeTime);
        gameObject.SetActive(true);
    }


    protected virtual void FollowObject()
    {
        if (_followTransform != null && _activeCamera != null)
        {
            Vector3 newPosition = _activeCamera.WorldToScreenPoint(_followTransform.position);
            transform.position = new Vector3(newPosition.x, newPosition.y, 0);
        }
    }
}
