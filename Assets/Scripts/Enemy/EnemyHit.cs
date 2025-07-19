using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyHit : MonoBehaviour
{

    public GameObject _player;
    private Gameplay _gameplay;

    private Animator _anim;

    private int _animIDHit;

    public LimbMetaData limbMetaData;
    // Start is called before the first frame update
    void Start()
    {
        _gameplay = _player.GetComponent<Gameplay>();
        _anim = GetComponent<Animator>();
        _animIDHit = Animator.StringToHash("hit");
    }

    // Update is called once per frame
    void OnCollisionStay(Collision collision)
    {
       
        Debug.Log($"Nope! This enemy got hit by {collision.collider.name}");
    }

    void OnTriggerStay(Collider other)
    {
         if (limbMetaData.LimbDetection(other))
        {
    
            Debug.Log($"This enemy got hit by {other.name}");
        }
    }




}
