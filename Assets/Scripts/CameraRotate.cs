using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;

public class CameraRotate : MonoBehaviour
{
    public bool ManualCamera { get; private set; }

    [SerializeField] float _cameraRotateSpeed = 5f;

    CinemachineVirtualCameraBase vcamManual;

    private void Awake()
    {
        vcamManual = GetComponent<CinemachineVirtualCameraBase>();
    }

    private void Update()
    {
        if(ManualCamera)
        {
            if(Input.mousePosition.x < Screen.width/2)
            {
                // adjust x axis value on manual camera positive
            }
            else
            {
                // adjust x axis value on manual camera negative
            }

            if(Input.mousePosition.y < Screen.height / 2)
            {
                // adjust y axis value on manual camera positive
            }
            else
            {
                // adjust y axis value on manual camera negative
            }

        }
    }

    // these two are meant to be hooked into events, but the event system can change cameras automatically?
    public void SwitchToManual(CinemachineVirtualCameraBase vcamFreelook)
    {
        // set manual as live camera

        // give manual the last transform of freelook -- this is automatically handled by inherit tranform on in the editor I think

        // set Manual bool true

    }

    public void SwitchToFree()
    {
        // set manual bool false

        // set freelook as live camera
    }
}
