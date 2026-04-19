using System.Collections;
using System.Collections.Generic;
using UnityEngine;
    
public class PostMovingEvent : MonoBehaviour {
    public AK.Wwise.Event Play_CoolCar_Moving;
    // Use this for initialization.
    public void CoolCar_Moving_SFX() {
        Play_CoolCar_Moving.Post(gameObject);
    }
}