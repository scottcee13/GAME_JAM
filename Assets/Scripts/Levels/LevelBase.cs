using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LevelBase : MonoBehaviour
{
    public static LevelBase Instance { get; private set; }

    [SerializeField] private GameObject _pastWorld;
    [SerializeField] private GameObject _presentWorld;

    private bool _inThePast = false;

    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
        {
            Destroy(gameObject);
            return;
        }
        _pastWorld.SetActive(true);
        _presentWorld.SetActive(false);
    }

    public void OnTimeShift()
    {
        _inThePast = !_inThePast;
        _pastWorld.SetActive(_inThePast);
        _presentWorld.SetActive(!_inThePast);
    }

    public void CompleteLevel()
    {
        GameManager.Instance.gameState = GameManager.GameState.GAME_WIN;
        UIManager.Instance.hideTimer();
        GameManager.Instance.GameFrozen = true;
        UIManager.Instance.CompleteLevel();
    }
}
