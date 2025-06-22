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

    private bool hit;



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
        robot.leftArm.abilities[0].user = _player.gameObject;

        //Ability index
      

    }

    // Update is called once per frame
    void Update()
    {
        //If left-click

        float delta = Time.deltaTime;
        
        if (_input.basicAttack)
        {
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
        }
    }

    //Basic attack method
  /*  private void BasicAttack(ArmType arm)
    {
        Debug.Log("Start attack- Abilities: " +arm.abilities.Count + " index: "+ index );

        //If cooldowns haven't started, populate with zeros
        if (cooldown == null)
        {
            cooldown = new float[arm.abilities.Count];
        }
        //While index is less than amount of abilities called && it's cooldown is 0
        if (index < arm.abilities.Count)
        {
             Debug.Log("Index Cooldown: " + cooldown[index]);
            if (cooldown[index] <= 0)
            {
                Debug.Log("In the mainframe");
                //Set respective duration and cooldown       
                time = arm.abilities[index].duration;
                cooldown[index] = arm.abilities[index].cooldown;
            }

                //Start effect
                Debug.Log("Not Effect: "+arm.abilities[index].isTrigger + " Name :" + arm.abilities[index].name);

                if (arm.abilities[index].isTrigger == false && time>0)
                {

                    arm.abilities[index].Effect(this.gameObject);
                }
                    
            

            //Start duration
            Duration(arm.abilities);

        }
        //After all effects, left click is not held down (create seperate method and parameter later)
        
           

    }

    private void BasicSpecial(ArmType arm)
    {
        if (index > -1 && arm.abilities[index].isTrigger != false)
        {
            arm.abilities[index].Effect(this.gameObject);
        }
    }


    private void Duration(List<Abilities> ability)
    {
        if (time > 0)
        {
            time -= Time.deltaTime;
            Debug.Log("Time: " + time);
        }
        else //If duration time hits 0, start next effect
        {
            index++;
        }
    }

    private void Cooldown(List<Abilities> ability)
    {
        //For each ability
        for (int i = 0; i < ability.Count; i++)
        {
            if (cooldown[i] > 0)
                cooldown[i] -= Time.deltaTime;

            if (cooldown[i] < 0)
            {
                 //reset cooldown
                cooldown[i] = 0;
            }   
            if (index >= ability.Count && cooldown[ability.Count-1] <=0)
                {
                Debug.Log("Cooldowns over: ");
                    _input.basicAttack = false;
                    index = 0;
                }

            if (index < robot.leftArm.abilities.Count)
                Debug.Log("Cooldown: " + cooldown[index]);
        }
    }*/
}