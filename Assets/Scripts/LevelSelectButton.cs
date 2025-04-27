using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Button))]
public class LevelSelectButton : MonoBehaviour
{
    [SerializeField]
    private Sprite levelSprite;
    [SerializeField]
    private string levelName;
    [SerializeField]
    private string levelPath, levelFile;

    private MenuScript menuScript;
    private float highScore;

    private void Awake()
    {
        menuScript = FindAnyObjectByType<MenuScript>();
        GetComponent<Button>().onClick.AddListener(ButtonPressed);
    }

    private void Start()
    {
        highScore = FFGJData.GetLevelData(levelPath, levelFile).highScore;
    }

    void ButtonPressed()
    {
        menuScript.SelectLevel(levelSprite, levelName, highScore);
    }
}
