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


    [SerializeField]
    private ArmInstance leftArm;
    private ArmInstance rightArm;
    private ArmInstance chassis;
    private ArmInstance legs;

    public bool hit;

    void Awake()
    {

    }

    void Start()
    {
        _input = GetComponent<InputClass>();
        _player = GetComponent<PlayerController>();

        leftArm = new ArmInstance(robot.leftArm);
    }

    private bool _temp_clicked_last_frame = false;

    void FixedUpdate()
    {
        if (_input.basicAttack)
        {
            leftArm.Activate(this.gameObject, leftArm);
        }
        if (!_input.basicAttack)
        {
            leftArm.Deactivate(this.gameObject, leftArm);
        }

        leftArm.FixedUpdateFromArm(this.gameObject, leftArm);
        _temp_clicked_last_frame = _input.basicAttack;
    }

     private void OnTriggerEnter(Collider other)
    {
        var context = new EffectContext
        {
            source = this.gameObject,
            target = other.gameObject,
            direction = this.gameObject.transform.forward
        };

        Debug.Log("hitting");

        leftArm.ApplyEffect(context); // or pass data from constructor
    

       
    }


}