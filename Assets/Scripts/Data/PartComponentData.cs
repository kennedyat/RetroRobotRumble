using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.VFX;
[CreateAssetMenu(menuName = "ScriptableObjects/Part Data")]
public class PartComponentData : ScriptableObject
{
    
    [Header("Basic Data")]
    public PartCommonData commonData;

     public Sprite icon;
    public float cooldown;
      
    [Header("Audio & Visuals")]
    public AudioClip[] audioClips;
    public VisualEffect[] visualEffects;
    public string animationTriggerName;

    [Header("Components - Add behaviors here!")]
    [Tooltip("Drag component SOs here to build your ability")]
    public PartComponent[] components;
}
