using System;
using System.Collections;
using System.Collections.Generic;
using System.Net.NetworkInformation;
using Unity.VisualScripting;
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

    // Update is called once per frame
    void Update()
    {

    }
}