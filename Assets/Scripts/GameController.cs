using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameController : MonoBehaviour
{

    // Update is called once per frame
    void Update()
    {
        // restarts scene
        if (Input.GetKeyDown(KeyCode.Backspace))
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);

        if (Input.GetKeyDown(KeyCode.Escape))
            Application.Quit();
    }
}
