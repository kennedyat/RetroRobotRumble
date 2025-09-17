using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnableCutscene : MonoBehaviour
{
    // Start is called before the first frame update
    [SerializeField] private GameObject cutscene;
    [SerializeField] private GameObject cutsceneManager;
    void Start()
    {
        bool isEnabled = PlayerPrefs.GetInt("CutsceneEnabled", 1) == 1;
        cutscene.SetActive(isEnabled);
        cutsceneManager.SetActive(isEnabled);

    }
 

}
