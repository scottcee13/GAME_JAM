using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class MenuScript : MonoBehaviour
{
    [SerializeField]
    private GameObject mainMenuScreen;
    [SerializeField] 
    private GameObject levelSelectScreen;

    [SerializeField]
    private TextMeshProUGUI bestTime;
    [SerializeField]
    private Image levelPreview;

    [SerializeField]
    private Button startGame;
    [SerializeField]
    private Button quitGame;

    private string levelToLoad;

    void Start()
    {
        startGame.onClick.AddListener(OpenLevelSelect);
        quitGame.onClick.AddListener(GameManager.QuitGame);
    }

    public void OpenLevelSelect()
    {
        levelSelectScreen.SetActive(true);
        mainMenuScreen.SetActive(false);
    }

    public void ReturnToMainMenu()
    {
        mainMenuScreen.SetActive(true);
        levelSelectScreen.SetActive(false);
    }

    public void SelectLevel(Sprite levelImage, string levelName, float highScore)
    {
        levelPreview.sprite = levelImage;
        levelToLoad = levelName;
        bestTime.text = (highScore < float.MaxValue) ? "Best Time: " + TGData.ConvertToTimeFormat(highScore) : "Level Not Beaten";
    }

    public void ResetData()
    {
        TGData.DeleteAllLevelData();
    }

    public void LoadLevel()
    {
        if (levelToLoad != null)
            SceneLoadManager.Instance.LoadLevel(levelToLoad);
    }
}
