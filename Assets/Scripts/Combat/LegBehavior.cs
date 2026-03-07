using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class LegBehavior : MonoBehaviour
{
     private PartInstance passiveAbility;
    private Animator animator;
    private Rigidbody playerRb;
    private HitBoxManager boxManager;
    private CombatPartManager manager;

    private static PlayerInput sharedPlayerInput;
    private InputAction dashInput;
    public void Initialize(PartComponentData passiveData,
        HitBoxManager hitBoxManager,
        CombatPartManager partManager,
        Animator anim,
        Rigidbody rb)
    {
        SetupInput();
        
        animator = anim;
        playerRb = rb;
        boxManager = hitBoxManager;
        manager = partManager;

        var context = new PartContext
        {
            Owner = transform,
            Animator = animator,
            Rigidbody = playerRb
        };
        
        passiveAbility = new PartInstance(passiveData, context, manager,blocks: false, blocked: false);
        
        // Auto-activate passive
        //if (passiveAbility != null)
           // passiveAbility.Execute(animator);
    }

    private void SetupInput()
    {
        var inputMap = PlayerInitializer.sharedPlayerInput.Player;
        dashInput = inputMap.Dash;
        dashInput.started += OnDashInput;
    }

    private void OnDashInput(InputAction.CallbackContext context)
    {
            Debug.Log($"[LegBehavior] Dash input received. passiveAbility null: {passiveAbility == null}");

        if (passiveAbility != null && passiveAbility.CanUse)
            passiveAbility.Execute(animator);
    }
    private void FixedUpdate()
    {
        if (passiveAbility != null)
            passiveAbility.UpdateAbility(Time.fixedDeltaTime);
    }
    
    private void OnDestroy()
    {
        if (passiveAbility != null)
            passiveAbility.Cleanup();
    }
}
