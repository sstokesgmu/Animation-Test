using System.Net.Sockets;
using UnityEngine;
using UnityEngine.Events;

public class AnimationStateEvents : StateMachineBehaviour
{
    public string eventName;
    [Range(0, 1)] public float triggerTime;

    bool hasTriggerd = false;

    AnimationEventReciever reciever;

    public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        hasTriggerd = false;
        reciever = animator.GetComponent<AnimationEventReciever>();
    }

    public override void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        float currentTime = stateInfo.normalizedTime % 1f;

        if(!hasTriggerd && currentTime >= triggerTime)
        {
                NotifyReciever(animator);
                hasTriggerd = true;
        }
    }

    void NotifyReciever(Animator animator)
    {
        if(reciever != null)
            reciever.OnAnimationEventTriggered(eventName);
    }
}
