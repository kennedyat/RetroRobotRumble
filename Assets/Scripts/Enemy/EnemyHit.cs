using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyHit : MonoBehaviour
{
    public GameObject _player;
    private Animator _anim;

    private int _animIDHit;
    // Start is called before the first frame update
    void Start()
    {
        _anim = GetComponent<Animator>();
        _animIDHit = Animator.StringToHash("hit");
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    void OnCollisionEnter(Collision collision)
    {
       
    }
}
