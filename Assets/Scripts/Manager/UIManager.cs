    using System;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    // Singleton instance of the ui manager
    public static UIManager Instance { get; private set; }

    [SerializeField]
    private TextMeshProUGUI gameClearTime;
    [SerializeField]
    private TextMeshProUGUI bestClearTime;
    [SerializeField]
    private TextMeshProUGUI timerText;
    public float _timer;
    public float HighScore = float.MinValue;

    public GameObject pauseMenu;
    public GameObject gameOverMenu;  
    public GameObject gameWinMenu;
    public GameObject timerObject;
    public GameObject newBestObject;

    public float Timer => _timer;


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

    private void Start()
    {
        HideUI();
    }

    private void Update()
    {
        _timer -= Time.deltaTime;

        //only run in scenes with Timer object
        if (timerObject)
            timerText.text = FFGJData.ConvertToTimeFormat(_timer);
    }

    public void Resume()
    {
        GameManager.Instance.TogglePause();
    }

    public void LoadNextLevel()
    {
        SceneLoadManager.Instance.LoadNextLevel();
    }

    public void Restart()
    {
        SceneLoadManager.Instance.LoadLevel(SceneManager.GetActiveScene().buildIndex);
    }

    public void ReturnToMenu()
    {
        SceneLoadManager.Instance.ReturnToMenu();
    }

    public void hideTimer()
    {
        timerObject.SetActive(false);
    }

    public void showTimer()
    {
        timerObject.SetActive(true);
    }

    public void ResetTimer(float timer)
    {
        _timer = timer;
    }

    public void HideUI()
    {
        timerObject.SetActive(false);
        pauseMenu.SetActive(false);
        gameOverMenu.SetActive(false);
        gameWinMenu.SetActive(false);
        newBestObject.SetActive(false);
    }

    public void CompleteLevel(string path, string fileName, bool endOfArea)
    {
        gameWinMenu.SetActive(true);
        if (_timer < HighScore)
        {
            HighScore = _timer;
            FFGJData.SaveLevelData(HighScore, path, fileName);
            newBestObject.SetActive(true);
        }
        gameClearTime.text = FFGJData.ConvertToTimeFormat(_timer);
        bestClearTime.text = FFGJData.ConvertToTimeFormat(HighScore);
    }
}

