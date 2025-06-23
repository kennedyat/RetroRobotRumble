using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting.Dependencies.Sqlite;
using UnityEngine;

public class EnemyHit : MonoBehaviour
{

    public GameObject _player;
    private Gameplay _gameplay;

    private Animator _anim;

    private int _animIDHit;
    // Start is called before the first frame update
    void Start()
    {
        _gameplay = _player.GetComponent<Gameplay>();
        _anim = GetComponent<Animator>();
        _animIDHit = Animator.StringToHash("hit");
    }

    // Update is called once per frame
    void Update()
    {
        if (_gameplay.hit)
        {
            Debug.Log("Enemy Hit");
            _anim.SetTrigger(_animIDHit);
            _gameplay.hit = false;
        }
    }



    void OnCollisionEnter(Collision collision)
    {
       
    }
}
