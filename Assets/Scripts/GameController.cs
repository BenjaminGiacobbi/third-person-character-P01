using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GameController : MonoBehaviour
{
    [SerializeField] ThirdPersonMovement player = null;
    [SerializeField] Canvas gameOverCanvas = null;

    private void OnEnable()
    {
        player.Death += OnGameOver;
    }

    private void OnDisable()
    {
        player.Death -= OnGameOver;
    }

    private void Start()
    {
        player.ActivePlayer(true);
        gameOverCanvas.gameObject.SetActive(false);
    }

    // Update is called once per frame
    void Update()
    {
        // restarts scene
        if (Input.GetKeyDown(KeyCode.Backspace))
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);

        if (Input.GetKeyDown(KeyCode.Escape))
            Application.Quit();

        if (Input.GetKeyDown(KeyCode.J))
            player.ActivePlayer(true);

        if (Input.GetKeyDown(KeyCode.K))
            player.ActivePlayer(false);
    }

    private void OnGameOver()
    {
        gameOverCanvas.gameObject.SetActive(true);
    }
}
