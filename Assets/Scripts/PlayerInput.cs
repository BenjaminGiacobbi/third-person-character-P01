using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class PlayerInput : MonoBehaviour
{
    public event Action<Vector3> Move = delegate { };
    public event Action<float> Jump = delegate { };
    public event Action<float> Sprint = delegate { };

    private void Update()
    {
        MoveInput();
        JumpInput();
        SprintInput();
    }

    // gets keyboard axes movement and sends event
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
        float sprintFloat = Input.GetAxisRaw("Sprint");
        Sprint?.Invoke(sprintFloat);
    }
}
