using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using System;

public class PlayerInput : MonoBehaviour
{
    public event Action<Vector3> Move = delegate { };
    public event Action<float> Jump = delegate { };
    public event Action StartSprint = delegate { };
    public event Action StopSprint = delegate { };
    public event Action LeftClick = delegate { };
    public event Action RightClick = delegate { };

    private void Update()
    {
        MoveInput();
        JumpInput();
        SprintInput();
        Mouse0Input();
        Mouse1Input();
    }


    private void MoveInput()
    {
        float horizontal = Input.GetAxisRaw("Horizontal");
        float vertical = Input.GetAxisRaw("Vertical");


        // combines as Vector3 and sends
        Vector3 direction = new Vector3(horizontal, 0f, vertical).normalized;
        Move?.Invoke(direction);
    }


    private void JumpInput()
    {
        float jumpFloat = Input.GetAxisRaw("Jump");
        Jump?.Invoke(jumpFloat);
    }


    private void SprintInput()
    {
        if(Input.GetButtonDown("Sprint"))
            StartSprint?.Invoke();


        if (Input.GetButtonUp("Sprint"))
            StopSprint?.Invoke();
    }


    public void Mouse0Input()
    {
        if (Input.GetKeyDown(KeyCode.Mouse0))
            LeftClick?.Invoke();
    }


    public void Mouse1Input()
    {
        if (Input.GetKeyDown(KeyCode.Mouse1))
            RightClick?.Invoke();
    }
}
