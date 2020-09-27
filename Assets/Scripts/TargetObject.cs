using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TargetObject : UIObject
{
    [SerializeField] float _rotateSpeed = 5f;

    // Update is called once per frame
    void Update()
    {
        base.FollowObject();
        RotateImage();
    }

    private void RotateImage()
    {
        transform.Rotate(Vector3.forward * Time.deltaTime * _rotateSpeed);
    }
}
