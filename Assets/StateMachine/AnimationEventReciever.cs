using UnityEngine;
using UnityEngine;
using System.Collections.Generic;

public class AnimationEventReciever : MonoBehaviour
{
    [SerializeField] private List<global::AnimationEvent> events = new();

    public void OnAnimationEventTriggered(string eventName)
    {
        events.Find(evt => evt.eventName == eventName)?.eventResponse?.Invoke();
    }
}