using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class MenuScript : MonoBehaviour
{
    [SerializeField] private string levelPath, levelFile, levelName;

    [SerializeField]
    private TextMeshProUGUI bestTime;
    [SerializeField]
    private Button resetData;
    [SerializeField]
    private Button startGame;
    [SerializeField]
    private Button quitGame;

    private float _highScore;

    void Start()
    {
        startGame.onClick.AddListener(LoadLevel);
        quitGame.onClick.AddListener(GameManager.QuitGame);
        HandleScoreUI();
    }

    public void ResetData()
    {
        FFGJData.DeleteAllLevelData();
        HandleScoreUI();
    }

    public void HandleScoreUI()
    {
        _highScore = FFGJData.GetLevelData(levelPath, levelFile).highScore;
        resetData.onClick.RemoveAllListeners();
        if (_highScore > float.MinValue)
            resetData.onClick.AddListener(ResetData);
        else
            resetData.gameObject.SetActive(false);
        bestTime.text = (_highScore > float.MinValue) ? "Personal Best\n" + FFGJData.ConvertToTimeFormat(_highScore) : "";
    }

    public void LoadLevel()
    {
        Debug.Log("Loading level: " + levelName);
        SceneLoadManager.Instance.LoadLevel(levelName);
    }
}
