using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;

public class CameraRotate : MonoBehaviour
{
    public bool ManualCamera { get; private set; }

    [SerializeField] float _cameraRotateSpeed = 5f;
    [SerializeField] CinemachineVirtualCameraBase _vcam;
    [SerializeField] PlayerInput _inputScript;

    Transform _camTransform;
    

    // caching
    private void Awake()
    {
        _camTransform = Camera.main.transform;
    }

    private void Update()
    {
        if(ManualCamera)
        {
            if(Input.mousePosition.x < Screen.width/2)
            {
                Debug.Log(Input.mousePosition.x);
                _camTransform.eulerAngles += new Vector3(_cameraRotateSpeed * Time.deltaTime, 0, 0);
                Debug.Log(_camTransform.eulerAngles);
            }
            else
            {
                _camTransform.eulerAngles -= new Vector3(_cameraRotateSpeed * Time.deltaTime, 0, 0);
            }

            if(Input.mousePosition.y < Screen.height / 2)
            {
                _camTransform.eulerAngles += new Vector3(0, _cameraRotateSpeed * Time.deltaTime, 0);
            }
            else
            {
                _camTransform.eulerAngles -= new Vector3(0, _cameraRotateSpeed * Time.deltaTime, 0);
            }

        }
    }

    // these two are meant to be hooked into events, but the event system can change cameras automatically?
    public void CameraSwitch(float switchAxis)
    {
        if (switchAxis > 0)
        {
            _vcam.enabled = false;
            ManualCamera = true;
        }
            
        else if (switchAxis == 0)
        {
            _vcam.enabled = true;
            ManualCamera = false;
        }
            
    }

    public void SwitchToFree()
    {
        // set manual bool false

        // set freelook as live camera
    }
}
