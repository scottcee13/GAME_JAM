using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class LevelBase : MonoBehaviour
{
    public static LevelBase Instance { get; private set; }

    [SerializeField] private string path, fileName;
    [SerializeField] private bool endOfArea = false;

    [Space]

    [Tooltip("How many seconds to beat the level")]
    [SerializeField] private float _allottedTime = 300f;

    [Space]

    [SerializeField] private GameObject _pastWorld;
    [SerializeField] private GameObject _presentWorld;
    [SerializeField] private bool _inThePast = false; // when do we start?

    [Space]

    [Tooltip("For player stuck checks")]
    [SerializeField] private List<CompositeCollider2D> _mapColliders;

    public float AllottedTime => _allottedTime;
    public List<CompositeCollider2D> MapColliders => _mapColliders;

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

    private void Start()
    {
        UIManager.Instance.HighScore = FFGJData.GetLevelData(path, fileName).highScore;

        //set starting time based on yes
        _pastWorld.SetActive(_inThePast);
        _presentWorld.SetActive(!_inThePast);
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
        UIManager.Instance.CompleteLevel(path, fileName, endOfArea);
    }
}
