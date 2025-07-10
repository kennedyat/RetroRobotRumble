using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
/// <summary>
///Monobehavior for each arm behavior
/// Designers can adjust public variables here via script or inpector.
/// This partial class focus's on modifiable variables and Unity lifecycle methods.
/// </summary>

public sealed partial class ExampleClass : MonoBehaviour
{

    public float variable;

    void Start()
    {
        
    }
    void FixedUpdate()
    {
       
    }

}

/// <summary>
/// Partial class implementing the IArmBase, which defines shared
/// input behavior (OnClick, OnHold, OnRelease) used across all arm skill scripts.
/// This part of the class handles logic, private variables, and internal dependencies.
/// </summary>

public sealed partial class ExampleClass : IArmBase
{
  

    public void OnClick()
    {
      
    }

    public void OnHold()
    {

    }

    public void OnRelease()
    {

    }
}


