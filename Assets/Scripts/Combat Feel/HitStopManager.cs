using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class HitStopManager : MonoBehaviour
{

    // sets up the manager
    [Header("Hit Stop Options")]
    [SerializeField, Tooltip("Hitstop flag.")]
    bool isHitStopActive = false;

    bool isInvincibleActive = false;

    //Hitstop should be called once per activation! This keeps track of that
    [Header("Combat Feel")]
    [SerializeField] protected float GlobalHitstopTime = 0.02f;
    [SerializeField] protected float DeathHitstopTime = 0.08f;
    public float uniqueHitStopTimer; //just to make sure we have a variable to store the unique (if we want) hit stop timers based on abilities or other triggers.
    public float uniqueIFrameTime;
    public AnimationCurve hitStopCurve;
    public float hitStopDuration;

    // Sets isHitStop to false at the start incase somehow it got messed up during initialization 
    protected void Start()
    {
        isHitStopActive = false;
    }

    //private void Update()
    //{

    //}

    //returns isHitStop, True would mean there is HitStop Active, False would mean the game isn't using hitstop at the moment
    public bool isHitStopped()
    { return isHitStopActive; }

    //returns isInvincible, True would mean the I-Frames are on, False WOuld mean the Iframes Are off.

    public bool isInvincible()
    { return isInvincibleActive; }


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
            uniqueHitStopTimer = uniqueHitStopTime;
            Debug.Log("death Hit Stop started");
            StartCoroutine(nameof(UniqueHitstop));
 

        }
    }

    public void IFrameinitiator(float uniqueIFrameTime)
    {
        if (isInvincibleActive)
        {
            return;
            // maybe set it to false here so we can fit in more hit stop later?
        }
        else
        {

            uniqueHitStopTimer = uniqueIFrameTime;
            isInvincibleActive = true;
            StartCoroutine(nameof(UniqueIFrames));
            isInvincibleActive = false;

        }
    }




    public IEnumerator GlobalHitstop()
    {
        Time.timeScale = .80f;
        yield return new WaitForSecondsRealtime(GlobalHitstopTime/10);
        Time.timeScale = 0.5f;
        yield return new WaitForSecondsRealtime(GlobalHitstopTime / 10);
        Time.timeScale = 0.0f;
        yield return new WaitForSecondsRealtime(GlobalHitstopTime);
        Time.timeScale = 0.5f;
        yield return new WaitForSecondsRealtime(GlobalHitstopTime / 10);
        Time.timeScale = 1.0f;
        isHitStopActive = false;
    }

    public IEnumerator UniqueHitstop()
    {
        float timepassed = 0f;
        hitStopDuration = uniqueHitStopTimer * 2;
        //Debug.Log("we entered UniqueHitstop");
        while (timepassed <= hitStopDuration)
        {
            timepassed = timepassed + Time.deltaTime;
 
            float percent = Mathf.Clamp01(timepassed / hitStopDuration);

            Debug.Log("this is percentage " + percent);
            Debug.Log("this is the curve output "+ Mathf.Clamp01(hitStopCurve.Evaluate(percent)));
            float TimeScaleAxis = Mathf.Clamp01(hitStopCurve.Evaluate(percent));
            Time.timeScale = TimeScaleAxis;
        }
        Time.timeScale = 1.0f;
        isHitStopActive = false;
        yield return null;

        //Time.timeScale = 0.8f;
        //yield return new WaitForSecondsRealtime(uniqueHitStopTimer / 10);
        //Time.timeScale = 0.5f;
        //yield return new WaitForSecondsRealtime(uniqueHitStopTimer / 10);
        //Time.timeScale = 0.0f;
        //yield return new WaitForSecondsRealtime(uniqueHitStopTimer);
        //Time.timeScale = 0.5f;
        //yield return new WaitForSecondsRealtime(uniqueHitStopTimer / 10);
        //Time.timeScale = 1.0f;
        //isHitStopActive = false;

  
    }
    public IEnumerator UniqueIFrames()
    {
        //Debug.Log("we entered UniqueHitstop");
        Time.timeScale = 0.0f;

        yield return new WaitForSecondsRealtime(uniqueHitStopTimer);
        Time.timeScale = 1.0f;

    }



}

