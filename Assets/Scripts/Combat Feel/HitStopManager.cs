using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class HitStopManager : MonoBehaviour
{

    // sets up the manager
    [Header("Hit Stop Options")]
    [SerializeField, Tooltip("Hitstop flag.")]
    bool isHitStopActive = false;


    //Hitstop should be called once per activation! This keeps track of that
    [Header("Combat Feel")]
    [SerializeField] protected float GlobalHitstopTime = 0.02f;
    [SerializeField] protected float DeathHitstopTime = 0.08f;
    public float uniqueHitStopTimer; //just to make sure we have a variable to store the unique (if we want) hit stop timers based on abilities or other triggers.


    // Sets isHitStop to false at the start incase somehow it got messed up during initialization 
    protected void Start()
    {
        isHitStopActive = false;
    }

    //returns isHitStop, True would mean there is HitStop Active, False would mean the game isn't using hitstop at the moment
    public bool isHitStopped()
    { return isHitStopActive; }

    //StartCoroutine(nameof(GlobalHitstop));



    public void hitStopinitiator(float uniqueHitStopTime)
    {
        if (isHitStopActive)
        {
            return;
            // maybe set it to false here so we can fit in more hit stop later?
        }
        else
        {
            
            uniqueHitStopTimer = uniqueHitStopTime;
            isHitStopActive = true;
            StartCoroutine(nameof(UniqueHitstop));
            isHitStopActive = false;

        }
    }
    public void DeathhitStopinitiator(float uniqueHitStopTime)
    {
        if (isHitStopActive)
        {
            return;
            // maybe set it to false here so we can fit in more hit stop later?
        }
        else
        {
            Debug.Log("death Hit Stop started");
            StartCoroutine(nameof(UniqueHitstop));
 

        }
    }






    public IEnumerator GlobalHitstop()
    {
        Time.timeScale = 0.0f;
        yield return new WaitForSecondsRealtime(GlobalHitstopTime);
        Time.timeScale = 1.0f;
        isHitStopActive = false;
    }

    public IEnumerator UniqueHitstop()
    {
        //Debug.Log("we entered UniqueHitstop");
        Time.timeScale = 0.0f;

        yield return new WaitForSecondsRealtime(uniqueHitStopTimer);
        Time.timeScale = 1.0f;
  
    }


}

