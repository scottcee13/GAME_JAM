using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneLoadManager : MonoBehaviour
{
    public static SceneLoadManager Instance { get; private set; }

    public Animator transition;

    public float transitionTime = 0.5f;

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

    public void LoadLevel(int levelIndex)
    {
        if (!GameManager.Instance.IsTransitioning)
            StartCoroutine(LoadLevel(levelIndex, GameManager.GameState.PLAYING));
    }

    public void LoadLevel(string levelIndex)
    {
        if (!GameManager.Instance.IsTransitioning)
            StartCoroutine(LoadLevel(levelIndex, GameManager.GameState.PLAYING));
    }

    public void LoadNextLevel()
    {
        if (!GameManager.Instance.IsTransitioning)
            StartCoroutine(LoadLevel(SceneManager.GetActiveScene().buildIndex + 1, GameManager.GameState.PLAYING));
    }

    public void ReturnToMenu()
    {
        if (!GameManager.Instance.IsTransitioning)
            StartCoroutine(LoadLevel("MainMenu", GameManager.GameState.MAIN_MENU));
    }        

    IEnumerator LoadLevel(int levelIndex, GameManager.GameState nextState)
    {
        transition.SetTrigger("Next Scene");
        GameManager.Instance.gameState = GameManager.GameState.TRANSITION;

        bool sceneLoaded = false;
        void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            if (scene.buildIndex == levelIndex)
            {
                sceneLoaded = true;
            }
        }
        SceneManager.sceneLoaded += OnSceneLoaded;

        yield return new WaitForSecondsRealtime(transitionTime);

        UIManager.Instance.HideUI();
        GameManager.Instance.GameFrozen = false;
        transition.SetTrigger("Load Scene");
        SceneManager.LoadScene(levelIndex);

        while (!sceneLoaded)
        {
            yield return null;
        }

        SceneManager.sceneLoaded -= OnSceneLoaded;

        AudioManager.Instance.PlayMusic(0);
        UIManager.Instance.showTimer();
        if (nextState != GameManager.GameState.MAIN_MENU)
            UIManager.Instance.ResetTimer(LevelBase.Instance.AllottedTime);


        GameManager.Instance.gameState = nextState;
    }

    IEnumerator LoadLevel(string levelName, GameManager.GameState nextState)
    {
        transition.SetTrigger("Next Scene");
        GameManager.Instance.gameState = GameManager.GameState.TRANSITION;

        bool sceneLoaded = false;
        void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            if (scene.name == levelName)
            {
                sceneLoaded = true;
            }
        }
        SceneManager.sceneLoaded += OnSceneLoaded;

        yield return new WaitForSecondsRealtime(transitionTime);

        AudioManager.Instance.PlayMusic(0);
        UIManager.Instance.HideUI();
        GameManager.Instance.GameFrozen = false;
        transition.SetTrigger("Load Scene");
        SceneManager.LoadScene(levelName);

        while (!sceneLoaded)
        {
            yield return null;
        }

        SceneManager.sceneLoaded -= OnSceneLoaded;

        
        UIManager.Instance.ResetTimer(LevelBase.Instance.AllottedTime);
        if (nextState != GameManager.GameState.MAIN_MENU)
            UIManager.Instance.showTimer();

        GameManager.Instance.gameState = nextState;
    }
}
