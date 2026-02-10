using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class HitStopManager : MonoBehaviour
{

    // sets the default number of hitStops to 0
    public enum HitStop { numberOfStops = 0 }
    [Header("Hit Stop Options")]
    [SerializeField, Tooltip("Hitstop flag.")]
    bool isHitStopActive = false;
    [SerializeField, Tooltip("Hitstop script, it causes the actual hit stop")]
    GameObject hitStopScript;
    [SerializeField, Tooltip("Where you check which type of hit stop you are applying.")]
    GameObject hitStopObject;


    // Sets isHitStop to false at the start incase somehow it got messed up during initialization 
    protected void Start()
    {
        isHitStopActive = false;
    }

    //returns isHitStop, True would mean there is HitStop Active, False would mean the game isn't using hitstop at the moment
    public bool isHitStopped()
    { return isHitStopActive; }
    }

    //public void hitStopType()
    //{

    //}
