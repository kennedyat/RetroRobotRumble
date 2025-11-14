using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AnimationEventReceiver : MonoBehaviour
{

    [SerializeField] List<AnimationEvent> events = new();

    public void OnAnimationEventTrigger(string eventName)
    {
        foreach (AnimationEvent e in events)
        {
            if (e.eventName == eventName)
            {
                e?.animEvent?.Invoke();
            }
        }
    }
}
