using System.Collections;
using System.Collections.Generic;
using UnityEngine;
    
public class PostMarchSoundEvent : MonoBehaviour {
    public AK.Wwise.Event PlayMarchSound;
    // Use this for initialization.
    public void Play_March_Sound() {
        PlayMarchSound.Post(gameObject);
    }
}