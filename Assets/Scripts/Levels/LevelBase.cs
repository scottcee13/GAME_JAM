using System.Collections;
using System.Collections.Generic;
using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.Tilemaps;
using UnityEngine.UI;
using static GameManager;

public class LevelBase : MonoBehaviour
{
    public static LevelBase Instance { get; private set; }

    [SerializeField] private string path, fileName;
    [SerializeField] private bool endOfArea = false;

    [Space]

    [Tooltip("How many seconds to beat the level")]
    [SerializeField] private float _allottedTime = 300f;

    [Space]

    [Header("Timeshifting")]
    [SerializeField] private GameObject _pastWorld;
    [SerializeField] private GameObject _presentWorld;
    [SerializeField] private CinemachineCamera _levelCinemachineCam;
    [SerializeField] private float _timeShiftZoom = 0.9f;
    [SerializeField] private bool _inThePast = false; // when do we start?
    [SerializeField] private float flashDuration = 0.25f;
    [SerializeField] private Image flashImage;
    [SerializeField] private Color presentColor;
    [SerializeField] private Color pastColor;

    private Coroutine _timeShiftCoroutine;

    public float AllottedTime => _allottedTime;

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

    private void Update()
    {
        if (UIManager.Instance.Timer <= 0f && GameManager.Instance.gameState == GameState.PLAYING)
        {
            GameManager.Instance.death.Invoke("You ran out of time");
        }
    }

    public void OnTimeShift()
    {
        _inThePast = !_inThePast;
        _pastWorld.SetActive(_inThePast);
        _presentWorld.SetActive(!_inThePast);
        if (_timeShiftCoroutine != null)
            StopCoroutine(_timeShiftCoroutine);
        _timeShiftCoroutine = StartCoroutine(TimeShiftFlash(_inThePast));
    }

    public void CompleteLevel()
    {
        GameManager.Instance.gameState = GameManager.GameState.GAME_WIN;
        UIManager.Instance.hideTimer();
        GameManager.Instance.GameFrozen = true;
        UIManager.Instance.CompleteLevel(path, fileName, endOfArea);
    }

    private IEnumerator TimeShiftFlash(bool toThePast)
    {
        float t = 0f;
        float r = (toThePast) ? pastColor.r : presentColor.r;
        float g = (toThePast) ? pastColor.g : presentColor.g;
        float b = (toThePast) ? pastColor.b : presentColor.b;
        float baseCamSize = _levelCinemachineCam.Lens.OrthographicSize;

        while (t < flashDuration / 4f)
        {
            t += Time.deltaTime;
            float alpha = Mathf.Lerp(0f, 0.75f, t / (flashDuration / 4f));
            flashImage.color = new Color(r, g, b, alpha);
            _levelCinemachineCam.Lens.OrthographicSize = Mathf.Lerp(baseCamSize, baseCamSize * _timeShiftZoom, t / (flashDuration / 4f));
            yield return null;
        }

        //isInPast = !isInPast;
        //pastEnvironment.SetActive(isInPast);
        //presentEnvironment.SetActive(!isInPast);

        t = 0f;
        while (t < (flashDuration * 3 / 4f))
        {
            t += Time.deltaTime;
            float alpha = Mathf.Lerp(0.75f, 0f, t / (flashDuration * 3 / 4f));
            flashImage.color = new Color(r, g, b, alpha);
            _levelCinemachineCam.Lens.OrthographicSize = Mathf.Lerp(baseCamSize * _timeShiftZoom, baseCamSize, t / (flashDuration * 3 / 4f));
            yield return null;
        }
    }
}
