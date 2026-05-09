using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UIElements;

/// <summary>
/// Arm behavior specifically designed for combo-based attacks that require multiple hitboxes.
/// Supports three different hitboxes for different combo stages (e.g., OniSamurai 5-hit combo).
/// </summary>
public class ComboArmBehavior : MonoBehaviour
{
    [Header("Combo Hitboxes")]
    [Tooltip("Hitbox for combo hits 1-2 (smaller angle attacks)")]
    public GameObject comboHitBox1;

    [Tooltip("Hitbox for combo hits 3-4 (wider angle attacks)")]
    public GameObject comboHitBox2;

    [Tooltip("Hitbox for combo hit 5 (spin/full circle attack)")]
    public GameObject comboHitBox3;

    [Header("Special Hitbox")]
    public GameObject specialHitBox;

    public PartInstance normalAbility;
    public PartInstance specialAbility;
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
        side = armSide;
        animator = anim;
        playerRb = rb;
        boxManager = hitBoxManager;
        manager = partManager;

        // Setup input
        SetupNewInput(armSide);

        // Create context with all three combo hitboxes stored in CustomData
        var normalContext = CreateComboContext();
        var specialContext = CreateContext(specialHitBox);

        normalContext.CustomData["InputAction"] = normalInput;
        specialContext.CustomData["InputAction"] = specialInput;

        // Store all three hitboxes in CustomData for the component to access
        HitBox hitbox1 = comboHitBox1 ? comboHitBox1.GetComponent<HitBox>() : null;
        HitBox hitbox2 = comboHitBox2 ? comboHitBox2.GetComponent<HitBox>() : null;
        HitBox hitbox3 = comboHitBox3 ? comboHitBox3.GetComponent<HitBox>() : null;

        normalContext.CustomData["ComboHitBox1"] = hitbox1;
        normalContext.CustomData["ComboHitBox2"] = hitbox2;
        normalContext.CustomData["ComboHitBox3"] = hitbox3;

        // Set default hitbox (for backwards compatibility)
        normalContext.HitBox = hitbox1;

        Debug.Log($"[ComboArmBehavior] Created combo context with 3 hitboxes for {armSide}");
        Debug.Log($"[ComboArmBehavior] ComboHitBox1: {(hitbox1 != null ? hitbox1.name : "NULL")}");
        Debug.Log($"[ComboArmBehavior] ComboHitBox2: {(hitbox2 != null ? hitbox2.name : "NULL")}");
        Debug.Log($"[ComboArmBehavior] ComboHitBox3: {(hitbox3 != null ? hitbox3.name : "NULL")}");

        // Create ability instances
        if (normalData != null)
        {
            normalAbility = new PartInstance(normalData, normalContext, manager, side, blocks: false, blocked: true);
        }
        else
        {
            Debug.LogWarning($"[ComboArmBehavior] Normal ability data is NULL for {side}");
        }

        if (specialData != null)
        {
            specialAbility = new PartInstance(specialData, specialContext, manager, side, blocks: true, blocked: false);
        }
        else
        {
            Debug.LogWarning($"[ComboArmBehavior] Special ability data is NULL for {side}");
        }

        animator.SetBool("Mirror", side == LeftOrRightControls.LEFT_ARM);
    }

    /// <summary>
    /// Creates a context for combo attacks with all three hitboxes available.
    /// </summary>
    private PartContext CreateComboContext()
    {
        // Use comboHitBox1 as the default HitBox (for backwards compatibility)
        HitBox defaultBox = comboHitBox1 ? comboHitBox1.GetComponent<HitBox>() : null;

        var context = new PartContext
        {
            Owner = transform,
            Animator = animator,
            Rigidbody = playerRb,
            HitBox = defaultBox,
            hitBoxManager = boxManager,
            partManager = manager
        };

        return context;
    }

    /// <summary>
    /// Creates a standard context for special attacks (single hitbox).
    /// </summary>
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

            // Get input actions based on arm side
            normalInput = armSide == LeftOrRightControls.LEFT_ARM
                ? inputMap.LeftArmNormal
                : inputMap.RightArmNormal;
            specialInput = armSide == LeftOrRightControls.LEFT_ARM
                ? inputMap.LeftArmSpecial
                : inputMap.RightArmSpecial;

            normalInput.started += OnNormalInputStarted;
            specialInput.started += OnSpecialInputStarted;

            inputMap.Enable();

            Debug.Log($"[ComboArmBehavior] New Input System setup complete for {armSide}");
        }
        catch (System.Exception e)
        {
            Debug.LogError($"[ComboArmBehavior] Failed to setup new input system: {e.Message}. Enable useFallbackInput in inspector.");
            useFallbackInput = true;
        }
    }

    private void OnNormalInputStarted(InputAction.CallbackContext context)
    {
        if (normalAbility != null && normalAbility.CanUse)
        {
            Debug.Log($"[ComboArmBehavior] Can Use?: {normalAbility.CanUse}");
            BarkManager.Instance?.PlayBarkForPart("Player Basic Attack", normalAbility.PartName, "Arm (Any)", gameObject.name);
            normalAbility.Execute(animator);
        }
        else if (normalAbility != null)
        {
            Debug.Log($"[ComboArmBehavior] Cannot use {side} normal. State: {normalAbility.CurrentState}, CD: {normalAbility.RemainingCooldown:F2}");
        }
    }

    private void OnSpecialInputStarted(InputAction.CallbackContext context)
    {
        if (specialAbility != null && specialAbility.CanUse)
        {
            BarkManager.Instance?.PlayBarkForPart("Player Special Attack", specialAbility.PartName, "Arm (Any)", gameObject.name);
            specialAbility.Execute(animator);
        }
        else if (specialAbility != null)
        {
            Debug.Log($"[ComboArmBehavior] Cannot use {side} special. State: {specialAbility.CurrentState}, CD: {specialAbility.RemainingCooldown:F2}");
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
