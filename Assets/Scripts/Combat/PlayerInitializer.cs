using System.Collections;
using System.Collections.Generic;
using Assets.Scripts.Combat.Prototype;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class PlayerInitializer : MonoBehaviour
{

    [SerializeField] GameObject existingLeftArm;
    [SerializeField] GameObject existingRightArm;
    [SerializeField] GameObject parentObject;
    [SerializeField] GameObject originalRig;
    [SerializeField] HitBoxManager hitBoxManager;
    [SerializeField] CombatPartManager partManager;
    
    
    [SerializeField] Image leftBasicIcon;
    [SerializeField] Image rightBasicIcon;
    [SerializeField] Image leftSpecialIcon;
    [SerializeField] Image rightSpecialIcon;
    
    private Animator playerAnimator;
    private Rigidbody playerRb;
    
    protected void Start()
    {
        Robot robot = RunData.currentRun.Robot;
        GameObject player = GameObject.Find("Player");
        playerAnimator = player.GetComponent<Animator>();
        playerRb = player.GetComponent<Rigidbody>();
        
        if (partManager == null)
        {
            partManager = player.GetComponent<CombatPartManager>();
            if (partManager == null)
                partManager = player.AddComponent<CombatPartManager>();
        }

        if (existingLeftArm != null)
        {
            existingLeftArm.SetActive(false);
            Destroy(existingLeftArm);
            existingLeftArm = null;
        }
             if (existingRightArm != null)
            {
                existingRightArm.SetActive(false);
                Destroy(existingRightArm);
                existingRightArm = null;
            }


        if (robot.leftArm != null)
            SetupArm(robot.leftArm, LeftOrRightControls.LEFT_ARM);
        if (robot.rightArm != null)
            SetupArm(robot.rightArm, LeftOrRightControls.RIGHT_ARM);
        if (robot.chassis != null)
            SetupChassis(robot.chassis);
        if (robot.legs != null)
            SetupLegs(robot.legs);
    }
    
    private void SetupArm(ArmType armType, LeftOrRightControls side)
    {

        var swapJoint = originalRig.GetComponent<ArmSwap>();
    
    Debug.Log($"[SetupArm] Setting up {side}");
    Debug.Log($"[SetupArm] ArmType: {armType?.name}");
    Debug.Log($"[SetupArm] Normal Ability: {armType?.normalAbility?.name}");
    Debug.Log($"[SetupArm] Special Ability: {armType?.specialAbility?.name}");
       
    GameObject arm = Instantiate(armType.combatPrefab, parentObject.transform, false);
    arm.transform.localScale = side == LeftOrRightControls.LEFT_ARM ? Vector3.one : new Vector3(-1, 1, 1);

    arm.transform.Find("Remote Transform").GetComponent<RemoteTransform>().remote =
               this.transform.Find("Smooth Rotation").Find("Tilt Pivot");
    
    ArmBehavior behavior = arm.GetComponent<ArmBehavior>();
    if (behavior == null)
    {
        behavior = arm.AddComponent<ArmBehavior>();
        Debug.Log($"[SetupArm] Added ArmBehavior component to {arm.name}");
    }
    else
    {
        Debug.Log($"[SetupArm] Found existing ArmBehavior on {arm.name}");
    }
    
     SpriteRenderer basicSprite = arm.transform.Find("Basic Icon").GetComponent<SpriteRenderer>();
  
    
    SpriteRenderer specialSprite = arm.transform.Find("Special Icon").GetComponent<SpriteRenderer>();
    
    if(side == LeftOrRightControls.LEFT_ARM)
    {
        swapJoint.SwapJoint("LeftArm", arm);
           
        if (basicSprite != null)
        {
            leftBasicIcon.sprite = basicSprite.sprite;
        }
        if (specialSprite != null)
        {
            leftSpecialIcon.sprite = specialSprite.sprite;
        }
    }
    if(side == LeftOrRightControls.RIGHT_ARM)
    {
        swapJoint.SwapJoint("RightArm", arm);
       if (basicSprite != null)
        {
            rightBasicIcon.sprite = basicSprite.sprite;
        }
        if (specialSprite != null)
        {
            rightSpecialIcon.sprite = specialSprite.sprite;
        }
    }
    
    Debug.Log($"[SetupArm] About to call Initialize on {behavior.name}");
    Debug.Log($"[SetupArm] partManager null? {partManager == null}");
    Debug.Log($"[SetupArm] playerAnimator null? {playerAnimator == null}");
    Debug.Log($"[SetupArm] playerRb null? {playerRb == null}");
    
    behavior.Initialize(armType.normalAbility, armType.specialAbility, side, hitBoxManager, partManager, playerAnimator, playerRb);
    
    Debug.Log($"[SetupArm] Initialize complete for {side}");
    }
    

     private void SetupChassis(ChassisType chassisType)
    {

        //var swapJoint = originalRig.GetComponent<ArmSwap>();
    

        Debug.Log($"[SetupArm] ChassisType: {chassisType?.name}");

        
        GameObject chassis = Instantiate(chassisType.combatPrefab, parentObject.transform, false);

        chassis.transform.Find("Remote Transform").GetComponent<RemoteTransform>().remote =
                this.transform.Find("Smooth Rotation").Find("Tilt Pivot");
        
        ChassisBehavior behavior = chassis.GetComponent<ChassisBehavior>();
        if (behavior == null)
        {
            behavior = chassis.AddComponent<ChassisBehavior>();
            Debug.Log($"[SetupArm] Added ChassisBehavior component to {chassis.name}");
        }
        else
        {
            Debug.Log($"[SetupArm] Found existing ChassisBehavior on {chassis.name}");
        }
        
      
        //swapJoint.SwapJoint("Chassis", chassis);
      
        
        
        behavior.Initialize(chassisType.ultimateAbility, hitBoxManager, partManager, playerAnimator, playerRb);
        

    }

     private void SetupLegs(LegType legType)
    {

       // var swapJoint = originalRig.GetComponent<ArmSwap>();
    

        Debug.Log($"[SetupArm] ChassisType: {legType?.name}");

        
        GameObject leg = Instantiate(legType.combatPrefab, parentObject.transform, false);

        leg.transform.Find("Remote Transform").GetComponent<RemoteTransform>().remote =
                this.transform.Find("Smooth Rotation").Find("Tilt Pivot");
        
        LegBehavior behavior = leg.GetComponent<LegBehavior>();
        if (behavior == null)
        {
            behavior = leg.AddComponent<LegBehavior>();
            Debug.Log($"[SetupArm] Added ChassisBehavior component to {leg.name}");
        }
        else
        {
            Debug.Log($"[SetupArm] Found existing ChassisBehavior on {leg.name}");
        }
        
      
       // swapJoint.SwapJoint("Legs", leg);
      
        
        
        behavior.Initialize(legType.passiveAbility, hitBoxManager, partManager, playerAnimator, playerRb);
        

    }
   
    /*[SerializeField] GameObject existingLeftArm;
    [SerializeField] GameObject existingRightArm;
    [SerializeField] GameObject parentObject;
    [SerializeField] GameObject originalRig;

    [SerializeField] Image leftBasicIcon;
    [SerializeField] Image rightBasicIcon;
    [SerializeField] Image leftSpecialIcon;
    [SerializeField] Image rightSpecialIcon;
    protected void Start()
    {
        Robot robot = RunData.currentRun.Robot;
        var swapJoint = originalRig.GetComponent<ArmSwap>();
        if ((robot.leftArm != null ? robot.leftArm.combatPrefab : null) is GameObject leftArmPrefab)
        {
            if (existingLeftArm != null)
            {
                existingLeftArm.SetActive(false);
                Destroy(existingLeftArm);
                existingLeftArm = null;
            }

            if (robot.leftArm != null)
            {
                Debug.Log(robot.leftArm.partCommonData.name);
            }

            GameObject instance = Instantiate(leftArmPrefab);
            instance.transform.SetParent(parentObject.transform, false);
            instance.transform.localPosition = Vector3.zero;
            instance.transform.localScale = Vector3.one;
            instance.transform.localRotation = Quaternion.identity;

            // HACK: Arms should set their own remote transforms. Maybe.
            instance.transform.Find("Remote Transform").GetComponent<RemoteTransform>().remote =
                    this.transform.Find("Smooth Rotation").Find("Tilt Pivot");

            HackForInputs(instance, false);
            swapJoint.SwapJoint("RightArm", instance);
            Debug.Log(instance.name);

            SpriteRenderer basicSprite = instance.transform.Find("Basic Icon").GetComponent<SpriteRenderer>();
            if (basicSprite != null)
            {
                leftBasicIcon.sprite = basicSprite.sprite;
            }
            
            SpriteRenderer specialSprite = instance.transform.Find("Special Icon").GetComponent<SpriteRenderer>();
            if (specialSprite != null)
            {
                leftSpecialIcon.sprite = specialSprite.sprite;
            }

        }

        if ((robot.rightArm != null ? robot.rightArm.combatPrefab : null) is GameObject rightArmPrefab)
        {
            if (existingRightArm != null)
            {
                existingRightArm.SetActive(false);
                Destroy(existingRightArm);
                existingRightArm = null;
            }

            if (robot.rightArm != null)
            {
                Debug.Log(robot.rightArm.partCommonData.name);
            }

            GameObject instance = Instantiate(rightArmPrefab);
            instance.transform.SetParent(parentObject.transform, false);
            instance.transform.localPosition = Vector3.zero;
            instance.transform.localScale = new Vector3(-1, 1, 1);
            instance.transform.localRotation = Quaternion.identity;

            // HACK: Arms should set their own remote transforms. Maybe.
            instance.transform.Find("Remote Transform").GetComponent<RemoteTransform>().remote =
                    this.transform.Find("Smooth Rotation").Find("Tilt Pivot");

            HackForInputs(instance, true);
            swapJoint.SwapJoint("LeftArm", instance);
            Debug.Log(instance.name);

            SpriteRenderer basicSprite = instance.transform.Find("Basic Icon").GetComponent<SpriteRenderer>();
            if (basicSprite != null)
            {
                rightBasicIcon.sprite = basicSprite.sprite;
            }

            SpriteRenderer specialSprite = instance.transform.Find("Special Icon").GetComponent<SpriteRenderer>();
            if (specialSprite != null)
            {
                rightSpecialIcon.sprite = specialSprite.sprite;
            }
        }
    }

    private void HackForInputs(GameObject arm, bool right)
    {
        if (arm.GetComponent<SharkLaserCannon>() is SharkLaserCannon yay)
        {
            yay.leftOrRightControls = right ? SharkLaserCannon.LeftOrRightControls.RIGHT_ARM : SharkLaserCannon.LeftOrRightControls.LEFT_ARM;
        }
        if (arm.GetComponent<OverheatMinigun>() is OverheatMinigun yay2)
        {
            yay2.leftOrRightControls = right ? OverheatMinigun.LeftOrRightControls.RIGHT_ARM : OverheatMinigun.LeftOrRightControls.LEFT_ARM;
        }
        if (arm.GetComponent<Shinkansen_Revised>() is Shinkansen_Revised yay3)
        {
            yay3.leftOrRightControls = right ? LeftOrRightControls.RIGHT_ARM : LeftOrRightControls.LEFT_ARM;
        }
        if (arm.GetComponent<Locomotive_Revised>() is Locomotive_Revised yay4)
        {
            yay4.leftOrRightControls = right ? LeftOrRightControls.RIGHT_ARM : LeftOrRightControls.LEFT_ARM;
        }
    }*/
}
