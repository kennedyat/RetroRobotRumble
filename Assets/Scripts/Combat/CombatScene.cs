using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class CombatScene : MonoBehaviour
{
    public AK.Wwise.Event GameRoundStartEvent;
    private uint musicPlayingId = AkSoundEngine.AK_INVALID_PLAYING_ID;

    void Start()
    {
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;

        if (BarkManager.Instance != null)
         BarkManager.Instance.StopCurrentBark();

        StartCoroutine(PostMusicWhenReady());
        StartCoroutine(PlayRoundStartBark());
    }

    IEnumerator PostMusicWhenReady()
    {
        yield return null;
        yield return null;
        musicPlayingId = GameRoundStartEvent.Post(gameObject);
        Debug.Log($"Music posted with ID: {musicPlayingId}");
    }

    void OnDestroy()
    {
        if (musicPlayingId != AkSoundEngine.AK_INVALID_PLAYING_ID)
        {
            AkSoundEngine.StopPlayingID(musicPlayingId);
            Debug.Log($"Music stopped: {musicPlayingId}");
        }
    }

    IEnumerator PlayRoundStartBark()
    {
        string sceneName = UnityEngine.SceneManagement.SceneManager.GetActiveScene().name;
        if (sceneName == "MainCutscene")
        {
            PlayRoundStartBarkAudioOnly("Round Manager");
            yield break;
        }

        while (BarkManager.Instance == null)
        {
            yield return null;
        }

        string source = sceneName == "MainFinalBoss"
            ? "Final Boss"
            : "Round Manager";

        BarkManager.Instance.PlayRoundStartAnnouncerBark(source);

    }

    private void PlayRoundStartBarkAudioOnly(string source)
    {
        TextAsset barkDatabaseJson = Resources.Load<TextAsset>("Barks/BarkDatabase");
        if (barkDatabaseJson == null) return;

        BarkManager.BarkDatabase barkDatabase = JsonUtility.FromJson<BarkManager.BarkDatabase>(barkDatabaseJson.text);
        if (barkDatabase == null || barkDatabase.lines == null) return;

        List<BarkManager.BarkLine> roundStartLines = barkDatabase.lines
            .Where(line =>
                !line.tutorial &&
                line.triggerEvent == "Round Start" &&
                line.source == source)
            .ToList();

        if (roundStartLines.Count == 0) return;

        BarkManager.BarkLine line = roundStartLines[Random.Range(0, roundStartLines.Count)];
        AudioClip clip = Resources.Load<AudioClip>(line.audioResourcePath);
        if (clip == null) return;

        AudioSource audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
            audioSource = gameObject.AddComponent<AudioSource>();

        audioSource.playOnAwake = false;
        audioSource.spatialBlend = 0f;

        if (FindObjectOfType<AudioListener>() == null)
            gameObject.AddComponent<AudioListener>();

        audioSource.PlayOneShot(clip, PlayerPrefs.GetFloat(BarkManager.BarkVolumePrefsKey, 4f));

        if (AudioManager.Instance != null)
            AudioManager.Instance.DuckMusicForSeconds(clip.length);
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            //TemporaryEndCombatPressed();
        }
    }

    public void TemporaryEndCombatPressed()
    {
        RRRSceneManager.LoadBuildABot();
    }
}