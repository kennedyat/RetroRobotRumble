using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance = null;

    public AK.Wwise.Event music;
    [SerializeField] string musicVolumeRtpcName = "Music_Volume";
    [SerializeField, Range(0f, 1f)] float defaultMusicVolume = 0.35f;
    [SerializeField, Range(0f, 1f)] float barkDuckedMusicVolume = 0f;
    [SerializeField, Range(0f, 1f)] float barkDuckedWwiseVolume = 0.15f;

    public const string MusicVolumePrefsKey = "MusicVolume_v2";
    private float musicVolume = 1f;
    private Coroutine musicDuckCoroutine;
    private Coroutine wwiseDuckCoroutine;

    void Awake()
    {
        if (Instance != null)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;

        music.Post(gameObject);
        SetMusicVolume(PlayerPrefs.GetFloat(MusicVolumePrefsKey, defaultMusicVolume));
        SceneManager.sceneLoaded += OnSceneLoaded;
        DontDestroyOnLoad(gameObject);
    }

    private void OnDestroy()
    {
        if (Instance == this)
        {
            SceneManager.sceneLoaded -= OnSceneLoaded;
        }
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        ApplyMusicVolume(musicVolume);
    }

    public void SetMusicVolume(float volume)
    {
        musicVolume = Mathf.Clamp01(volume);
        PlayerPrefs.SetFloat(MusicVolumePrefsKey, musicVolume);
        ApplyMusicVolume(musicVolume);
    }

    public void DuckMusicForSeconds(float duration)
    {
        // BarkManager calls this whenever a voice line starts.
        if (musicDuckCoroutine != null)
        {
            StopCoroutine(musicDuckCoroutine);
        }

        musicDuckCoroutine = StartCoroutine(MusicDuck(duration));

        if (wwiseDuckCoroutine != null)
        {
            StopCoroutine(wwiseDuckCoroutine);
        }

        wwiseDuckCoroutine = StartCoroutine(WwiseDuck(duration));
    }

    private IEnumerator MusicDuck(float duration)
    {
        ApplyMusicVolume(Mathf.Min(musicVolume, barkDuckedMusicVolume));
        yield return new WaitForSeconds(duration);
        ApplyMusicVolume(musicVolume);
        musicDuckCoroutine = null;
    }

    private IEnumerator WwiseDuck(float duration)
    {
        float endTime = Time.time + duration;
        while (Time.time < endTime)
        {
            ApplyWwiseEmitterVolume(barkDuckedWwiseVolume);
            AkSoundEngine.SetOutputVolume(0, barkDuckedWwiseVolume);
            yield return null;
        }

        ApplyWwiseEmitterVolume(1f);
        AkSoundEngine.SetOutputVolume(0, 1f);
        wwiseDuckCoroutine = null;
    }

    private void ApplyMusicVolume(float volume)
    {
        /* This only works if Wwise is actually listening to the Music_Volume RTPC
           or if the music event's bus routing accepts this object volume. */
        if (!string.IsNullOrWhiteSpace(musicVolumeRtpcName))
        {
            AkSoundEngine.SetRTPCValue(musicVolumeRtpcName, volume * 100f);
            AkSoundEngine.SetRTPCValue(musicVolumeRtpcName, volume * 100f, gameObject);
        }

        AkAudioListener[] listeners = FindObjectsOfType<AkAudioListener>();
        if (listeners.Length == 0)
        {
            AkSoundEngine.SetGameObjectOutputBusVolume(gameObject, gameObject, volume);
            return;
        }

        foreach (AkAudioListener listener in listeners)
        {
            if (listener == null || !listener.enabled || !listener.gameObject.activeInHierarchy)
            {
                continue;
            }

            AkSoundEngine.SetGameObjectOutputBusVolume(gameObject, listener.gameObject, volume);
        }
    }

    public float GetMusicVolume()
    {
        return musicVolume;
    }

    private void ApplyWwiseEmitterVolume(float volume)
    {
        AkAudioListener[] listeners = FindObjectsOfType<AkAudioListener>();
        AkGameObj[] emitters = FindObjectsOfType<AkGameObj>();

        foreach (AkGameObj emitter in emitters)
        {
            if (emitter == null || emitter.gameObject == gameObject || !emitter.gameObject.activeInHierarchy)
            {
                continue;
            }

            if (listeners.Length == 0)
            {
                AkSoundEngine.SetGameObjectOutputBusVolume(emitter.gameObject, emitter.gameObject, volume);
                continue;
            }

            foreach (AkAudioListener listener in listeners)
            {
                if (listener == null || !listener.enabled || !listener.gameObject.activeInHierarchy)
                {
                    continue;
                }

                AkSoundEngine.SetGameObjectOutputBusVolume(emitter.gameObject, listener.gameObject, volume);
            }
        }
    }

}
