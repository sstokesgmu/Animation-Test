using System;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEditor.Animations;
using UnityEngine;

[CreateAssetMenu(menuName = "ScriptableObjects/Animation/AnimatorParameters")]
public class AnimatorParameters : ScriptableObject
{
    public AnimatorController anim;

    [SerializeField]
    [Tooltip("Pick the exact name of the Animator Parameter, Be Careful of Spaces!!")]
    //TODO: Make an editor scritp to make this more straight forward Access Animator Attached -> find parameters -> add to list 
    private List<string> entries = new List<string>();

    //Avoid Rebuilds store a reference 
    private Dictionary<string, int> _lookupRef;
    public IReadOnlyDictionary<string, int> Parameters => _lookupRef;
    public void OnEnable()
    {
        Debug.Log("Enabled");
        //Build or rebuild the dictionary when the object is loaded
        _lookupRef = new Dictionary<string, int>();
        foreach (var elements in entries)
        {
            if (string.IsNullOrEmpty(elements))
                continue;
            if (!_lookupRef.ContainsKey(elements))
                _lookupRef[elements] = Animator.StringToHash(elements);
            else
                Debug.LogWarning($"Duplicate paramerter key '{elements}' in {name}", this);
        }
    }
}