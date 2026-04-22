using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEngine.SocialPlatforms;
//using static UnityEditor.Progress;



public class HitStopManager : MonoBehaviour
{

    // sets up the manager
    [Header("Hit Stop Options")]
    [SerializeField, Tooltip("Hitstop flag.")]
    bool isHitStopActive = false;

    bool isInvincibleActive = false;

    bool isHit = false;

    //Hitstop should be called once per activation! This keeps track of that
    [Header("Combat Feel")]
    //[SerializeField] protected float GlobalHitstopTime = 0.02f;
    //[SerializeField] protected float DeathHitstopTime = 0.08f;
    public float uniqueHitStopTimer; //just to make sure we have a variable to store the unique (if we want) hit stop timers based on abilities or other triggers
    public AnimationCurve hitStopCurve;
    public float hitStopDuration;

    [Header("Combat visuals")]
    public float uniqueIFrameTimer;
    public AnimationCurve damageScreenDecay;
    public float damageScreenDecaytime = .1f;
    public GameObject globalVolumeref;
    private Vignette vignette;

    // setting up I-Frame Visuals
    private GameObject player;
    private PlayerInitializer playerInitializerRef;
    private GameObject LeftArm;
    private GameObject RightArm;
    private GameObject Chassis;
    private GameObject Legs;
    public AnimationCurve IFrameFlashingCurve;



    // Sets isHitStop to false at the start incase somehow it got messed up during initialization 
    protected void Start()
    {
        isHitStopActive = false;
        globalVolumeref = GameObject.Find("Global Volume");
        player= GameObject.Find("Player");
        playerInitializerRef= player.GetComponent<PlayerInitializer>();
        LeftArm = playerInitializerRef.RobotPartGetter("LeftArm");
        RightArm = playerInitializerRef.RobotPartGetter("RightArm");
        Chassis = playerInitializerRef.RobotPartGetter("Chassis");
        Legs = playerInitializerRef.RobotPartGetter("Legs");
        LeftArm.GetComponentInChildren<Renderer>().sharedMaterial.DisableKeyword("_EMISSION");
        RightArm.GetComponentInChildren<MeshRenderer>().sharedMaterial.DisableKeyword("_EMISSION");
        Chassis.GetComponentInChildren<MeshRenderer>().sharedMaterial.DisableKeyword("_EMISSION");
        Legs.GetComponentInChildren<MeshRenderer>().sharedMaterial.DisableKeyword("_EMISSION");



    }



    //returns isHitStop, True would mean there is HitStop Active, False would mean the game isn't using hitstop at the moment
    public bool isHitStopped()
    { return isHitStopActive; }

    //returns isInvincible, True would mean the I-Frames are on, False WOuld mean the Iframes Are off.

    public bool isInvincible()
    { return isInvincibleActive; }

    public bool isScreenRed()
    { return isHit; }


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
            //shouldn't be adding more invinciblity timer if we're already [title card]
        }
        else
        {

            uniqueIFrameTimer = uniqueIFrameTime;
            isInvincibleActive = true;
            StartCoroutine(nameof(UniqueIFrames));


        }
    }

    public void onHitScreenAdjustment(int Damage)
    {
        isHit = true;
        float ScreenRedTime;
        ScreenRedTime = 3 * ((float)Damage / 25);
        damageScreenDecaytime = ScreenRedTime;
        globalVolumeref.GetComponent<Volume>().profile.TryGet(out vignette);
        StartCoroutine(nameof(damageScreenChanger));
        vignette.intensity.value = 0f;

    }



 

    public IEnumerator UniqueHitstop()
    {
        //yield return null;
        float timepassed = 0f;
        hitStopDuration = uniqueHitStopTimer;


        //Debug.Log("we entered UniqueHitstop");
        while (timepassed < hitStopDuration)
        {
            timepassed = timepassed + Time.unscaledDeltaTime;

            float percent = Mathf.Clamp01(timepassed / hitStopDuration);

            //Debug.Log("this is percentage " + percent);
            //Debug.Log("this is the curve output "+ Mathf.Clamp01(hitStopCurve.Evaluate(percent)));
            float TimeScaleAxis = Mathf.Clamp01(hitStopCurve.Evaluate(percent));
            Time.timeScale = TimeScaleAxis;
            yield return null;
        }
        //Time.timeScale = 1.0f;
        isHitStopActive = false;
        yield return null;

        


    }
    public IEnumerator UniqueIFrames()
    {
        //Debug.Log("we entered UniqueIFrames");
        float timepassed = 0f;

        while (timepassed < uniqueIFrameTimer)
        {
            timepassed = timepassed + Time.unscaledDeltaTime;

            float percent = Mathf.Clamp01(timepassed / uniqueIFrameTimer);

            //Debug.Log("this is percentage " + percent);
            //Debug.Log("this is the curve output "+ Mathf.Clamp01(IFrameFlashingCurve.Evaluate(percent)));
            float TimeScaleAxis = Mathf.Clamp01(IFrameFlashingCurve.Evaluate(percent));

            if (TimeScaleAxis >=.5f)
            {
                
                LeftArm.GetComponentInChildren<Renderer>().sharedMaterial.EnableKeyword("_EMISSION");
                RightArm.GetComponentInChildren<MeshRenderer>().sharedMaterial.EnableKeyword("_EMISSION");
                Chassis.GetComponentInChildren<MeshRenderer>().sharedMaterial.EnableKeyword("_EMISSION");
                Legs.GetComponentInChildren<MeshRenderer>().sharedMaterial.EnableKeyword("_EMISSION");
            }
            else
            {
                
                LeftArm.GetComponentInChildren<Renderer>().sharedMaterial.DisableKeyword("_EMISSION");
                RightArm.GetComponentInChildren<MeshRenderer>().sharedMaterial.DisableKeyword("_EMISSION");
                Chassis.GetComponentInChildren<MeshRenderer>().sharedMaterial.DisableKeyword("_EMISSION");
                Legs.GetComponentInChildren<MeshRenderer>().sharedMaterial.DisableKeyword("_EMISSION");
            }
            yield return null;
        }
        LeftArm.GetComponentInChildren<Renderer>().sharedMaterial.DisableKeyword("_EMISSION");
        RightArm.GetComponentInChildren<MeshRenderer>().sharedMaterial.DisableKeyword("_EMISSION");
        Chassis.GetComponentInChildren<MeshRenderer>().sharedMaterial.DisableKeyword("_EMISSION");
        Legs.GetComponentInChildren<MeshRenderer>().sharedMaterial.DisableKeyword("_EMISSION");

        isInvincibleActive = false;


}

    public IEnumerator damageScreenChanger()
    {
        //yield return null;
        float timepassed = 0f;
     
        if (damageScreenDecaytime <=1f)
        {
            damageScreenDecaytime = 1f;
        }
        float intensityScalePercentage = .4f + (damageScreenDecaytime / 3f);

        //Debug.Log("we entered UniqueHitstop");
        while (timepassed < damageScreenDecaytime)
        {
            timepassed = timepassed + Time.unscaledDeltaTime;

            float percent = Mathf.Clamp01(timepassed / damageScreenDecaytime);
            //Debug.Log("intensityScalePercentage value is: " + intensityScalePercentage);

            //Debug.Log("this is percentage " + percent);
            //Debug.Log("this is the curve output "+ Mathf.Clamp01(hitStopCurve.Evaluate(percent)));
            float TimeScaleAxis = Mathf.Clamp01(damageScreenDecay.Evaluate(percent));
            vignette.intensity.value = TimeScaleAxis * intensityScalePercentage;
            //Time.timeScale = TimeScaleAxis;
            //adjusting the number on the vignette


            yield return null;
        }
        isHit = false;
        vignette.intensity.value = 0f;
    }
}

