using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.UI;

[CreateAssetMenu(menuName = "ArmBehavior/Overheat")]
public partial class Overheat : ArmBehaviorData
{
    public float currentOverheat = 0;
    public float maxOverheat = 100;
    public float overheatIncrement = 3; //Amount of overheat stacked per bullet 


    public float bufferTime = 0.5f;
    private float currentBufferTime = 0f;

    public float overheatDecrement = 0.33f; //Decrement calculated as overheat per second 
    public bool overheating = false;
    public float overheatPenalty = 5.0f;

    public Slider overheatSlider;
    public override IArmBehavior MakeInstance()
    {
        return Instantiate(this);
    }
}

public partial class Overheat : IArmBehavior
{
    public bool active;

    public void Activate(GameObject owner, ArmInstance arm)
    {
        active = true;
    }

    public void Deactivate(GameObject owner, ArmInstance arm)
    {
        active = false;
    }

    public void FixedUpdateFromArm(GameObject owner, ArmInstance arm)
    {
        if (!overheating && !active) //It not in overheat & mouse is not being pressed, begin overheat decrement
        {
            if (currentOverheat >= 0)
            {
                currentBufferTime += Time.fixedDeltaTime;
                if (currentBufferTime >= bufferTime) //if buffertime has passed, begin actual decrement
                {
                    currentOverheat -= overheatDecrement;
                    if (currentOverheat < 0)
                    {
                        currentOverheat = 0; //just in case it becomes minus 
                    }
                }
            }

        }
        else if (overheating) //During overheat, decrement over overheatpenalty time
        {
            currentOverheat -= currentOverheat / overheatPenalty * Time.fixedDeltaTime;
        }
        overheatSlider.value = currentOverheat;
    }

    public void IncrementOverheat()
    {
        if (currentOverheat <= maxOverheat)
        {
            currentOverheat += overheatIncrement;
            Debug.Log(currentOverheat);
        }
        else
        {
            Debug.Log("Overheating!");
            overheating = true;
        }
    }

}
