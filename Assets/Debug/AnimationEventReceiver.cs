using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AnimationEventReceiver : MonoBehaviour
{

    [SerializeField] List<AnimationEvent> events = new();

    public void OnAnimationEventTrigger(EventType eventType)
    {
        foreach (AnimationEvent e in events) {
            if (e.type == eventType)
            {
                e?.animEvent?.Invoke();
            }
        }
    }
}
