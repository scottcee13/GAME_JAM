using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Runtime.Serialization.Formatters.Binary;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;

public class GameManager : MonoBehaviour
{
    // Singleton instance of the game manager
    public static GameManager Instance { get; private set; }

    public UnityEvent death;
    public UnityEvent pause;

    [SerializeField]
    private InputAction pauseAction;

    private bool gameFrozen = false;

    public enum GameState
    {
        MAIN_MENU,
        PLAYING,
        PAUSED,
        GAME_WIN,
        GAME_OVER,
        TRANSITION
    }

    public GameState gameState = GameState.MAIN_MENU;

    public bool GameFrozen
    {
        get { return gameFrozen; }
        set { 
            if (value)
            {
                gameFrozen = true;
                Time.timeScale = 0f;
            }
            else
            {
                gameFrozen = false;
                Time.timeScale = 1f;
            }         
        }
        
    }

    public bool IsPlaying
    {
        get { return gameState == GameState.PLAYING; }       
    }

    public bool IsTransitioning
    {
        get { return gameState == GameState.TRANSITION; }
    }

    private void Awake()
    {
        // Ensure only one instance of the GameManager exists
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void Start()
    {
        death.AddListener(GameOver);
    }

    void GameOver()
    {
        gameState = GameState.GAME_OVER;
        UIManager.Instance.hideTimer();
        GameFrozen = true;
        UIManager.Instance.gameOverMenu.SetActive(true);    
    }

    //toggle pause
    public void TogglePause()
    {
        if (gameState != GameState.PLAYING && gameState != GameState.PAUSED)
            return;

        if (!gameFrozen)
        {
            pause.Invoke();
            GameFrozen = true;
            gameState = GameState.PAUSED;
        }
        else
        {
            GameFrozen = false;
            gameState = GameState.PLAYING;
        }
        UIManager.Instance.pauseMenu.SetActive(gameFrozen);
    }

    public static void QuitGame()
    {
        Application.Quit();
    }

}
