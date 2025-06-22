using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class Abilities : ScriptableObject
{
    public GameObject user;
    public float amount;
    public string name;
    public bool isAction;
    public float duration = 0.4f;
    public float cooldown=1f;

    public GameObject vfx;
    public string animationTrigger;
    



    public virtual void Initialize(GameObject user)
    {
        this.user = user;
    }

    public abstract void Effect(GameObject effected);
}
