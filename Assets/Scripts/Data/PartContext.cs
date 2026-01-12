using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PartContext 
{
    public Transform Owner;
    public Animator Animator;
    public Rigidbody Rigidbody;
    public HitBox HitBox;
    public HitBoxManager hitBoxManager;
    public CombatPartManager partManager;
    public PartInstance partInstance;
    public Dictionary<string, object> CustomData = new();
}
