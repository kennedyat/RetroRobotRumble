using System;
using System.Collections;
using System.Collections.Generic;
using System.Net.NetworkInformation;
using Unity.VisualScripting;
using UnityEditor.UI;
using UnityEngine;

//
public class Gameplay2 : MonoBehaviour
{
    private InputClass _input;
    public Robot robot;
    PlayerController _player;

    ArmInstance leftArm;

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
        if (_input.basicAttack && !_temp_clicked_last_frame)
        {
            leftArm.Activate(this.gameObject, leftArm);
        }
        if (!_input.basicAttack && _temp_clicked_last_frame)
        {
            leftArm.Deactivate(this.gameObject, leftArm);
        }

        leftArm.FixedUpdate(this.gameObject, leftArm);
        _temp_clicked_last_frame = _input.basicAttack;
    }
}