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
    [SerializeField]
    public float timer;
    [SerializeField]
    public float highScore = float.MaxValue;

    public GameObject pauseMenu;
    public GameObject gameOverMenu;  
    public GameObject gameWinMenu;
    public GameObject timerObject;
    public GameObject newBestObject;

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
        timer += Time.deltaTime;

        //only run in scenes with timer object
        if (timerObject)
            timerText.text = TGData.ConvertToTimeFormat(timer);
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

    public void resetTimer()
    {
        timer = 0f;
    }

    public void HideUI()
    {
        timerObject.SetActive(false);
        pauseMenu.SetActive(false);
        gameOverMenu.SetActive(false);
        gameWinMenu.SetActive(false);
        newBestObject.SetActive(false);
    }

    public void CompleteLevel()
    {       
        gameWinMenu.SetActive(true);
        if (timer < highScore)
        {           
            highScore = timer;
            newBestObject.SetActive(true);
        }
        gameClearTime.text = TGData.ConvertToTimeFormat(timer);
        bestClearTime.text = TGData.ConvertToTimeFormat(highScore);
    }

    public void CompleteGame(string path, string fileName, bool endOfArea)
    {
        gameWinMenu.SetActive(true);
        if (timer < highScore)
        {
            highScore = timer;
            TGData.SaveLevelData(highScore, path, fileName);
            newBestObject.SetActive(true);
        }
        gameClearTime.text = TGData.ConvertToTimeFormat(timer);
        bestClearTime.text = TGData.ConvertToTimeFormat(highScore);
    }
}

