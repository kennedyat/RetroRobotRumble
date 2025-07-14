using System;
using System.Collections;
using System.Collections.Generic;
using System.Net.NetworkInformation;
using Unity.VisualScripting;
using UnityEngine;

public class Gameplay : MonoBehaviour
{
    // Start is called before the first frame update

    private InputClass _input;
    private PlayerStats _stats;
    public Robot robot;
    PlayerController _player;

    private List<AbilityInstance> _leftAbility = new List<AbilityInstance>();
    private List<AbilityInstance> rightAbility = new List<AbilityInstance>();
    private List<AbilityInstance> _chassisAbility = new List<AbilityInstance>();
    private List<AbilityInstance> _legAbility = new List<AbilityInstance>();

    public bool hit;



    void Awake()
    {
        foreach (var ability in robot.leftArm.abilities)
        {
            Debug.Log("Checking ability: " + (ability != null ? ability.name : "NULL"));


            var instance = new AbilityInstance(ability);

            _leftAbility.Add(instance);
        }

    }
    void Start()
    {

        _input = GetComponent<InputClass>();
        _stats = GetComponent<PlayerStats>();
        _player = GetComponent<PlayerController>();
      



    }

    // Update is called once per frame
    void Update()
    {
        //If left-click

        float delta = Time.deltaTime;

        if (_input.basicAttack)
        {
            Debug.Log("Clicked");
            foreach (var ability in _leftAbility)
            {

                ability.Activate(this.gameObject);
            }

        }

        foreach (var ability in _leftAbility)
            ability.TickTimers(delta);

       

        //Other keybinds
    }

    void OnTriggerStay(Collider other)
    {
        //During duration of ability
        foreach (var ability in _leftAbility)
        {
            ability.TryTriggerEffect(other.gameObject);

            hit = ability.inEffect;
        }
    }
       
}