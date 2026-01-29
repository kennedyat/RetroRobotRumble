using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "ScriptableObjects/Components/BasicMelee")]

public class BasicMeleeComponent : PartComponent
{
   [Header("Melee Settings")]
    public float delay;
    
    
    public override void Initialize(PartContext context)
    {
        
        context.CustomData["BasicMelee_Timer"] = 0f;
        context.CustomData["BasicMelee_HitboxActivated"] = false;
    }

    public override void OnExecute(PartContext context)
    {
        context.CustomData["BasicMelee_Timer"] = delay;
        context.CustomData["BasicMelee_HitboxActivated"] = false;
    }

    public override void OnUpdate(PartContext context, float deltaTime)
    {
        float timer = (float)context.CustomData["BasicMelee_Timer"];
        bool hitboxActivated = (bool)context.CustomData["BasicMelee_HitboxActivated"];
        
        if (timer > 0)
        {
            timer -= deltaTime;
            
            // Check if delay finished
            if (timer <= 0 && !hitboxActivated)
            {
                ActivateHitbox(context);
                hitboxActivated = true;
            }
        }
        
     
        context.CustomData["BasicMelee_Timer"] = timer;
        context.CustomData["BasicMelee_HitboxActivated"] = hitboxActivated;
    }
}
