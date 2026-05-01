using UnityEngine;
using UnityEngine.UI;

public class AudioVolumeSettings : MonoBehaviour
{
    [SerializeField] Slider musicVolumeSlider;
    [SerializeField] Slider barkVolumeSlider;

    private void Start()
    {
        if (musicVolumeSlider != null)
        {
            musicVolumeSlider.minValue = 0f;
            musicVolumeSlider.maxValue = 1f;
            musicVolumeSlider.value = AudioManager.Instance != null ? AudioManager.Instance.GetMusicVolume() : PlayerPrefs.GetFloat(AudioManager.MusicVolumePrefsKey, 0.35f);
            musicVolumeSlider.onValueChanged.AddListener(SetMusicVolume);
        }

        if (barkVolumeSlider != null)
        {
            barkVolumeSlider.minValue = 0f;
            barkVolumeSlider.maxValue = 5f;
            barkVolumeSlider.value = BarkManager.Instance != null ? BarkManager.Instance.GetBarkVolume() : PlayerPrefs.GetFloat(BarkManager.BarkVolumePrefsKey, 4f);
            barkVolumeSlider.onValueChanged.AddListener(SetBarkVolume);
        }
    }

    public void SetMusicVolume(float volume)
    {
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.SetMusicVolume(volume);
        }
        else
        {
            PlayerPrefs.SetFloat(AudioManager.MusicVolumePrefsKey, Mathf.Clamp01(volume));
        }
    }

    public void SetBarkVolume(float volume)
    {
        if (BarkManager.Instance != null)
        {
            BarkManager.Instance.SetBarkVolume(volume);
        }
        else
        {
            PlayerPrefs.SetFloat(BarkManager.BarkVolumePrefsKey, Mathf.Clamp(volume, 0f, 5f));
        }
    }
}
