using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance;

    [SerializeField]
    private AudioClip[] _music;
    [SerializeField]
    private AudioClip[] _sfx;
    [SerializeField]
    private AudioSource _musicSource;
    [SerializeField]
    private AudioSource _sfxSource;

    private void Awake()
    {
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

    public void PlayMusic(int index)
    {
        if (_musicSource.clip == _music[index])
            return;

        _musicSource.clip = _music[index];
        _musicSource.Play();
    }

    public void StopMusic()
    {
        _musicSource.Stop();
        _musicSource.clip = null;
    }

    public void PlaySFX(int index)
    {
        _sfxSource.PlayOneShot(_sfx[index]);
    }
}