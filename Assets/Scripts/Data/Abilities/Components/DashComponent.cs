using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "ScriptableObjects/Components/Dash")]
public class DashComponent : PartComponent
{
    [Header("Dash Settings")]
    public float dashSpeed = 15f;
    public float dashDuration = 0.3f;
    
    private float dashTimeRemaining;
    private Vector3 dashDirection;
    
    public override void Initialize(PartContext context) 
    {
        Debug.Log($"[DashComponent] Initialize called");
    }
    
    public override void OnExecute(PartContext context)
    {
        Debug.Log($"[DashComponent] OnExecute called!");
        
        
        // Determine dash dir
        Vector3 currentVelocity = context.Rigidbody.velocity;
        
        if (currentVelocity.magnitude > 0.1f)
        {
            dashDirection = currentVelocity.normalized;
        }
        else
        {
            dashDirection = context.Owner.forward;
        }
        
        // Zero out velocity pre dash
        context.Rigidbody.velocity = Vector3.zero;
        
        // Set up dash state
        dashTimeRemaining = dashDuration;
       
        ActivateHitbox(context);
    }
    
    public override void OnUpdate(PartContext context, float deltaTime)
    {
        if (dashTimeRemaining > 0)
        {
            if (context.Rigidbody == null || context.Owner == null)
            {
                dashTimeRemaining = 0;
                return;
            }
            
            Vector3 movement = dashDirection * dashSpeed * deltaTime;
            Vector3 newPosition = context.Owner.position + movement;
            
            context.Rigidbody.MovePosition(newPosition);
            dashTimeRemaining -= deltaTime;
        }
    
    }
}