using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class Enemy : MonoBehaviour
{
    [Header("General Enemy Stats")]
    [SerializeField, Tooltip("A reference to the player")] 
    protected Transform player;
    [SerializeField, Tooltip("A reference to this enemy's rigidbody, used for movements")]
    protected Rigidbody rb;
    [SerializeField, Tooltip("Move speed of this enemy")] 
    protected float moveSpeed;
    [SerializeField, Tooltip("The health of this enemy")]
    protected int health;
    [SerializeField, Tooltip("The damage this enemy deals with whatever it attacks with")]
    protected int attackDamage;
    [SerializeField, Tooltip("The range this enemy needs to be within to initiate its attack")]
    protected float attackRange;

    protected void Start()
    {
        Initialize();
    }
    
    /// <summary>
    /// This function is always called within Start(), override it if needed.
    /// </summary>
    protected virtual void Initialize()
    {
        player = GameObject.FindWithTag("Player").transform;
        rb = GetComponent<Rigidbody>();
    }
    
    // helper function
    protected static Vector3 ZeroY(Vector3 input)
    {
        input.y = 0;
        return input;
    }
}
