using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AnimationBehavior : StateMachineBehaviour
{
    // Start is called before the first frame update

    public EventType type;
    [Range(0, 1)] public float eventTime;
    bool isTrigger;

    override public void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        isTrigger = false;
    }

    override public void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        float currentTime = stateInfo.normalizedTime % 1f;
        if (!isTrigger && currentTime >= eventTime) //when the current time of the animation passes the set event time
        {
            CallReceiver(animator);
            isTrigger = true;
        }
    }
    override public void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {

    }

    public void CallReceiver(Animator animator)
    {
        AnimationEventReceiver receiver = animator.GetComponent<AnimationEventReceiver>();

        if (receiver != null)
        {
            receiver.OnAnimationEventTrigger(type);
        }
    }
}
