using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance = null;

    #if !UNITY_STANDALONE_LINUX
    public AK.Wwise.Event music;
    #endif  

    void Awake() {
        if (Instance != null) {
            Destroy(gameObject);
            return;
        }

        Instance = this;

        #if !UNITY_STANDALONE_LINUX
        music.Post(gameObject);
        #endif
        DontDestroyOnLoad(gameObject);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
