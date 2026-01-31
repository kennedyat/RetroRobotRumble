using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.VFX;
//[CreateAssetMenu(menuName = "ScriptableObjects/Part Data")]
[System.Serializable]
public class PartComponentData
{
    
    [Header("BASIC DATA")]
    public PartCommonData commonData;
    public float cooldown;
    [Space(20)]
    [Header("AUDIO & VISUALS")]
    public AudioClip[] audioClips;
    public VisualEffect[] visualEffects;
    public string animationTriggerName;

    [Space(20)]
    [Header("COMPONENTS")]
    [Tooltip("Drag component SOs here to build your ability")]
    public PartComponent[] components;
}
