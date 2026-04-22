using System.Collections;
using System.Collections.Generic;
using Assets.Scripts.Combat.Prototype;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;

public class PlayerInitializer : MonoBehaviour
{

    [SerializeField] GameObject existingLeftArm;
    [SerializeField] GameObject existingRightArm;
    [SerializeField] GameObject existingChassis;
    [SerializeField] GameObject existingLegs;
    [SerializeField] GameObject parentObject;
    [SerializeField] GameObject originalRig;
    [SerializeField] HitBoxManager hitBoxManager;
    [SerializeField] CombatPartManager partManager;
    
    
    [SerializeField] Image leftBasicIcon;
    [SerializeField] Image rightBasicIcon;
    [SerializeField] Image leftSpecialIcon;
    [SerializeField] Image rightSpecialIcon;
    [SerializeField] UIAbilityCooldown uIAbilityCooldown;
    [SerializeField] PartDebug partDebug;

    public static PlayerInput sharedPlayerInput;
    
    private Animator playerAnimator;
    private Rigidbody playerRb;
    
    protected void Start()
    {
        sharedPlayerInput = new PlayerInput();
        sharedPlayerInput.Player.Enable();
        
        if(partDebug!=null )
        {
            if(partDebug.isDebug)
            {
                Cursor.visible = false;
                Cursor.lockState = CursorLockMode.Locked;
            }
            
        }
        
        //sharedPlayerInput = new PlayerInput();
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

        //Add sticker mods
    }
    
    private void SetupArm(ArmType armType, LeftOrRightControls side)
    {

        var swapJoint = originalRig.GetComponent<ArmSwap>();
    
    Debug.Log($"[SetupArm] Setting up {side}");
    Debug.Log($"[SetupArm] ArmType: {armType?.name}");
    //Debug.Log($"[SetupArm] Normal Ability: {armType?.normalAbility?.name}");
    //Debug.Log($"[SetupArm] Special Ability: {armType?.specialAbility?.name}");
    GameObject arm = null;
    if(side == LeftOrRightControls.LEFT_ARM)
    {
        arm = Instantiate(armType.combatPrefab, parentObject.transform, false);
        Destroy(existingLeftArm);
         existingLeftArm = arm;
    }
    if(side == LeftOrRightControls.RIGHT_ARM)
    {
        arm = Instantiate(armType.combatPrefabRight, parentObject.transform, false);
        Destroy(existingRightArm);
         existingRightArm = arm;
    }
    

   
    arm.transform.localScale = side == LeftOrRightControls.LEFT_ARM ? Vector3.one : new Vector3(-1, 1, 1);


   var remoteComp = arm.transform.Find("Remote Transform")?.GetComponent<RemoteTransform>();
    if(partDebug!=null )
    {
         if (remoteComp != null && partDebug.isDebug)
        {
            remoteComp.remote = this.transform.Find("Smooth Rotation").Find("Tilt Pivot");
        }
    }   
    else
    {
        Debug.Log($"[SetupArm] NoRemote Transform");
    }



    
    // Check if this arm needs ComboArmBehavior (for multi-hitbox combo attacks like OniSamurai)
    bool needsComboBehavior = CheckIfNeedsComboBehavior(armType);
    
    ArmBehavior behavior = null;
    ComboArmBehavior comboBehavior = null;
    
    if (needsComboBehavior)

    {
        // Use ComboArmBehavior for arms that need multiple hitboxes
        // Remove any existing ArmBehavior first
        ArmBehavior existingArmBehavior = arm.GetComponent<ArmBehavior>();
        if (existingArmBehavior != null)
        {
            DestroyImmediate(existingArmBehavior);
        }
        
        comboBehavior = arm.GetComponent<ComboArmBehavior>();
        if (comboBehavior == null)
        {
            comboBehavior = arm.AddComponent<ComboArmBehavior>();
            Debug.Log($"[SetupArm] Added ComboArmBehavior component to {arm.name}");
        }
        else
        {
            Debug.Log($"[SetupArm] Found existing ComboArmBehavior on {arm.name}");
        }
    }
    else
    {
        // Use standard ArmBehavior
        // Remove any existing ComboArmBehavior first
        ComboArmBehavior existingComboBehavior = arm.GetComponent<ComboArmBehavior>();
        if (existingComboBehavior != null)
        {
            DestroyImmediate(existingComboBehavior);
        }
        
        behavior = arm.GetComponent<ArmBehavior>();
        if (behavior == null)
        {
            behavior = arm.AddComponent<ArmBehavior>();
           
        }
        else
        {
            Debug.Log($"[SetupArm] Found existing ArmBehavior on {arm.name}");
        }
    }
    
   
    
    if (needsComboBehavior && comboBehavior != null)
    {
      
        comboBehavior.Initialize(armType.normalAbility, armType.specialAbility, side, hitBoxManager, partManager, playerAnimator, playerRb);
    }
    else if (behavior != null)
    {
      
        behavior.Initialize(armType.normalAbility, armType.specialAbility, side, hitBoxManager, partManager, playerAnimator, playerRb);
    }

    SpriteRenderer basicSprite = arm.transform.Find("Basic Icon")?.GetComponent<SpriteRenderer>();
  
    
    SpriteRenderer specialSprite = arm.transform.Find("Special Icon")?.GetComponent<SpriteRenderer>();
    
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
        if(needsComboBehavior)
        {
            uIAbilityCooldown.leftArmNormal = comboBehavior.normalAbility;
            uIAbilityCooldown.leftArmSpecial = comboBehavior.specialAbility;
        }else
        {
            uIAbilityCooldown.leftArmNormal = behavior.normalAbility;
            uIAbilityCooldown.leftArmSpecial = behavior.specialAbility;
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

         if(needsComboBehavior)
        {
            uIAbilityCooldown.rightArmNormal = comboBehavior.normalAbility;
            uIAbilityCooldown.rightArmSpecial = comboBehavior.specialAbility;
        }else
        {
            uIAbilityCooldown.rightArmNormal = behavior.normalAbility;
            uIAbilityCooldown.rightArmSpecial = behavior.specialAbility;
        }
    }
    
    Debug.Log($"[SetupArm] Initialize complete for {side}");
    }
    
    /// <summary>
    /// Checks if an arm type needs ComboArmBehavior (for multi-hitbox combo attacks).
    /// Checks if the normal ability uses OniSamuraiComboComponent, SnakeArmSweepComponent, or ShinkansenNormalComponent.
    /// </summary>
    private bool CheckIfNeedsComboBehavior(ArmType armType)
    {
        if (armType?.normalAbility?.components == null)
            return false;
        
        // Check if any component requires ComboArmBehavior
        foreach (var component in armType.normalAbility.components)
        {
            if (component is OniSamuraiComboComponent || 
                component is SnakeArmSweepComponent || 
                component is ShinkansenNormalComponent)
            {
                return true;
            }
        }
        
        return false;
    }
    

     private void SetupChassis(ChassisType chassisType)
    {

        var swapJoint = originalRig.GetComponent<ArmSwap>();
    

        Debug.Log($"[SetupChassis] ChassisType: {chassisType?.name}");

        //Temp solution for chassis placement
        GameObject chassis = Instantiate(chassisType.combatPrefab,existingChassis.transform.parent.gameObject.transform, false);
        //chassis.transform.position = new Vector3(existingChassis.transform.position.x, existingChassis., existingChassis.transform.position.z);
        existingChassis.SetActive(false);
        Destroy(existingChassis);
        existingChassis = chassis;

        chassis.transform.Find("Remote Transform").GetComponent<RemoteTransform>().remote =
                this.transform.Find("Smooth Rotation").Find("Tilt Pivot");
        
        ChassisBehavior behavior = chassis.GetComponent<ChassisBehavior>();
        if (behavior == null)
        {
            behavior = chassis.AddComponent<ChassisBehavior>();
        }
        else
        {
            Debug.Log($"[SetupArm] Found existing ChassisBehavior on {chassis.name}");
        }
        
      
        swapJoint.SwapJoint("Chassis", chassis);
      
        
        
        behavior.Initialize(chassisType.ultimateAbility, chassisType.passiveAbility, hitBoxManager, partManager, playerAnimator, playerRb);
        

    }

     private void SetupLegs(LegType legType)
    {

       var swapJoint = originalRig.GetComponent<ArmSwap>();
    

        Debug.Log($"[SetupLegs] LegType: {legType?.name}");

         //Temp solution for legs placement
        GameObject leg = Instantiate(legType.combatPrefab,existingLegs.transform.parent.gameObject.transform, false);
        leg.transform.position = existingLegs.transform.position;

        existingLegs.SetActive(false);
        Destroy(existingLegs);
        existingLegs = leg;

        //GameObject leg = Instantiate(legType.combatPrefab, parentObject.transform, false);

        leg.transform.Find("Remote Transform").GetComponent<RemoteTransform>().remote =
                this.transform.Find("Smooth Rotation").Find("Tilt Pivot");
        
        LegBehavior behavior = leg.GetComponent<LegBehavior>();
        if (behavior == null)
        {
            behavior = leg.AddComponent<LegBehavior>();
        }
        else
        {
            Debug.Log($"[SetupArm] Found existing LegsBehavior on {leg.name}");
        }
        
      
       //swapJoint.SwapJoint("Legs", leg);
      
        
        
       behavior.Initialize(legType.passiveAbility, hitBoxManager, partManager, playerAnimator, playerRb);
        

    }



    public GameObject RobotPartGetter(string RobotPartLocation)
    {
        GameObject nulled;

        if (RobotPartLocation == "LeftArm")
        {
            return existingLeftArm;
        }
        if (RobotPartLocation == "RightArm")
        {
            return existingRightArm;
        }
        if (RobotPartLocation == "Chassis")
        {
            return existingChassis;
        }
        if (RobotPartLocation == "Legs")
        {
            return existingLegs;
        }
        else
        {
            return nulled = null;
        }

    }
}
