using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class Abilities : ScriptableObject
{
    public float amount;
    public string name;
    public bool isAction;
    public float duration = 0.4f;
    public float cooldown = 1f;

    public GameObject vfx;
    public string animationTrigger;

    public List<string> canHit;

    [HideInInspector]
    public bool onHit = false;

    public abstract void Effect(GameObject effected, GameObject player);
    
    public abstract void OnCollision(GameObject effected, GameObject player);
}
