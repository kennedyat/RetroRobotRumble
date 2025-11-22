using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UIElements;

public class ArmBehavior : MonoBehaviour
{
    public GameObject normalHitBox;
    public GameObject specialHitBox;
    private PartInstance normalAbility;
    private PartInstance specialAbility;
    private LeftOrRightControls side;
    
    private Animator animator;
    private Rigidbody playerRb;
    private HitBoxManager boxManager;
    private CombatPartManager manager;
    
    private static PlayerInput sharedPlayerInput;
    private PlayerInput.PlayerActions inputMap;
    
    private InputAction normalInput;
    private InputAction specialInput;
    
    [Header("Debug")]
    [SerializeField] private bool useFallbackInput = false;
    
    public void Initialize(
        PartComponentData normalData,
        PartComponentData specialData,
        LeftOrRightControls armSide,
        HitBoxManager hitBoxManager,
        CombatPartManager partManager,
        Animator anim,
        Rigidbody rb)
    {
        Debug.Log($"[ArmBehavior] Initialize START for {armSide}");
        Debug.Log($"[ArmBehavior] normalData: {normalData?.name ?? "NULL"}");
        Debug.Log($"[ArmBehavior] specialData: {specialData?.name ?? "NULL"}");
        Debug.Log($"[ArmBehavior] partManager: {(partManager == null ? "NULL" : "OK")}");
        Debug.Log($"[ArmBehavior] animator: {(anim == null ? "NULL" : "OK")}");
        Debug.Log($"[ArmBehavior] rb: {(rb == null ? "NULL" : "OK")}");
        
        side = armSide;
        animator = anim;
        playerRb = rb;
        boxManager = hitBoxManager;
        manager = partManager;

        // Setup input
        if (!useFallbackInput)
        {
            SetupNewInput(armSide);
        }
        
        // Create shared contexts
        var normalContext = CreateContext(normalHitBox);
        var specialContext = CreateContext(specialHitBox);

         normalContext.CustomData["InputAction"] = normalInput;
        specialContext.CustomData["InputAction"] = specialInput;
        
        Debug.Log($"[ArmBehavior] Created contexts");
        
        // Create ability instances
        if (normalData != null)
        {
           
            normalAbility = new PartInstance(normalData, normalContext, manager, blocks: false, blocked: true);
            Debug.Log($"[ArmBehavior] Created normal ability for {side}: {normalAbility.PartName}");
           
        }
        else
        {
            Debug.LogWarning($"[ArmBehavior] Normal ability data is NULL for {side}");
        }
        
        if (specialData != null)
        {
           
            specialAbility = new PartInstance(specialData, specialContext, manager, blocks: true, blocked: false);
            Debug.Log($"[ArmBehavior] Created special ability for {side}: {specialAbility.PartName}");
            
        }
        else
        {
            Debug.LogWarning($"[ArmBehavior] Special ability data is NULL for {side}");
        }
             
    }
    
    private PartContext CreateContext(GameObject hitBox)
    {

        HitBox box = hitBox ? hitBox.GetComponent<HitBox>() : null;
        var context = new PartContext
        {
            Owner = transform,
            Animator = animator,
            Rigidbody = playerRb,
            HitBox = box,
            hitBoxManager = boxManager,
            partManager = manager
        };
        
        return context;
    }
    
    private void SetupNewInput(LeftOrRightControls armSide)
    {
        try
        {
            // Create or reuse the shared PlayerInput instance
            if (sharedPlayerInput == null)
            {
                sharedPlayerInput = new PlayerInput();
               
            }
            
            inputMap = sharedPlayerInput.Player;
            
            // Get  input actions based on arm side
            normalInput = armSide == LeftOrRightControls.LEFT_ARM 
                ? inputMap.LeftArmNormal 
                : inputMap.RightArmNormal;
            specialInput = armSide == LeftOrRightControls.LEFT_ARM 
                ? inputMap.LeftArmSpecial 
                : inputMap.RightArmSpecial;
            
           
            normalInput.started += OnNormalInputStarted;
            specialInput.started += OnSpecialInputStarted;
            
            inputMap.Enable();
            
            Debug.Log($"[ArmBehavior] New Input System setup complete for {armSide}");
        }
        catch (System.Exception e)
        {
            Debug.LogError($"[ArmBehavior] Failed to setup new input system: {e.Message}. Enable useFallbackInput in inspector.");
            useFallbackInput = true;
        }
    }
    
    private void OnNormalInputStarted(InputAction.CallbackContext context)
    {
        
        if (normalAbility != null && normalAbility.CanUse)
        {
            normalAbility.Execute(animator);
        }
        else if (normalAbility != null)
        {
            Debug.Log($"[ArmBehavior] Cannot use {side} normal. State: {normalAbility.CurrentState}, CD: {normalAbility.RemainingCooldown:F2}");
        }
    }
    
    private void OnSpecialInputStarted(InputAction.CallbackContext context)
    {
        
        if (specialAbility != null && specialAbility.CanUse)
        {
            specialAbility.Execute(animator);
        }
        else if (specialAbility != null)
        {
            Debug.Log($"[ArmBehavior] Cannot use {side} special. State: {specialAbility.CurrentState}, CD: {specialAbility.RemainingCooldown:F2}");
        }
    }
    
    
    
    protected void FixedUpdate()
    {
        if (normalAbility != null)
        {
            normalAbility.UpdateAbility(Time.fixedDeltaTime);
        }
        
        if (specialAbility != null)
        {
            specialAbility.UpdateAbility(Time.fixedDeltaTime);
        }
    }
    
    protected void OnDestroy()
    {
        // Unsubscribe from input events
        if (normalInput != null)
        {
            normalInput.started -= OnNormalInputStarted;
        }
        if (specialInput != null)
        {
            specialInput.started -= OnSpecialInputStarted;
        }
        
        
        // Cleanup abilities
        normalAbility?.Cleanup();
        specialAbility?.Cleanup();
    }
}