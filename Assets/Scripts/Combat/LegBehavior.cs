using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LegBehavior : MonoBehaviour
{
     private PartInstance passiveAbility;
    private Animator animator;
    private Rigidbody playerRb;
    private HitBoxManager boxManager;
    private CombatPartManager manager;
    public void Initialize(PartComponentData passiveData,
        HitBoxManager hitBoxManager,
        CombatPartManager partManager,
        Animator anim,
        Rigidbody rb)
    {
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
        if (passiveAbility != null)
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
