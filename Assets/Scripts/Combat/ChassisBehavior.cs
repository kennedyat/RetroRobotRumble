using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChassisBehavior : MonoBehaviour
{
    private PartInstance ultimateAbility;
    private Animator animator;
    private Rigidbody playerRb;
    private HitBoxManager boxManager;
    private CombatPartManager manager;
    
    public void Initialize(PartComponentData ultimateData,
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
            Rigidbody = playerRb,
            HitBox = transform.Find("HitBox")?.GetComponent<HitBox>()
        };
        
        //SetupNewInput();
        ultimateAbility = new PartInstance(ultimateData, context, manager, blocks: true, blocked: false);
        
        // TODO: Setup proper ultimate input
        // For now, use R key
    }
    
    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.R))
        {
            if (ultimateAbility != null)
                ultimateAbility.Execute(animator);
        }
    }
    
    private void FixedUpdate()
    {
        if (ultimateAbility != null)
            ultimateAbility.UpdateAbility(Time.fixedDeltaTime);
    }
    
    private void OnDestroy()
    {
        if (ultimateAbility != null)
            ultimateAbility.Cleanup();
    }
}
