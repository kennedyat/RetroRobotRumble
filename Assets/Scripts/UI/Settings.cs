using UnityEngine;
using UnityEngine.UI;

public class Settings : MonoBehaviour
{
    public Toggle toggle;

    public void Start()
    {
        PlayerPrefs.SetInt("CutsceneEnabled", toggle.isOn ? 1 : 0);
        PlayerPrefs.Save();
    }
    
       public void SaveToggleState(bool value)
    {
        PlayerPrefs.SetInt("CutsceneEnabled", value ? 1 : 0);
        PlayerPrefs.Save();
    }
}