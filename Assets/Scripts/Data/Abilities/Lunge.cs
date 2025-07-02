using System;
using UnityEngine;


[CreateAssetMenu(menuName = "ArmBehavior/Lunge")]
public class Lunge : ArmBehaviorData
{
    [Header("Lunge Parameters")]
    public float distance = 5f;
    public float speed = 10f;
    public float duration = 0.3f;
    public float cooldown = 1.2f;

    public override IArmBehavior MakeInstance()
    {
        return new Instance { data = this };
    }
}
    [Serializable]
    public class Instance : IArmBehavior
    {
        public Lunge data;
        public bool active;
        public float actionCooldown;
        public float actionDuration;

    public void Activate(GameObject owner, ArmInstance arm)
    {
        if (active|| actionCooldown>0) return;

        active = true;
            
        actionCooldown = data.cooldown;
        actionDuration = data.duration;
    }

    public void Deactivate(GameObject owner, ArmInstance arm)
    {
            active = false;
    }

   
    public void FixedUpdateFromArm(GameObject owner, ArmInstance arm)
    {
       actionCooldown = Mathf.Max(0, actionCooldown - Time.fixedDeltaTime);

            if (!active) return;

            actionDuration = Mathf.Max(0, actionDuration - Time.fixedDeltaTime);

            if (actionDuration <= 0)
            {
                active = false;
                return;
            }

            if (owner.TryGetComponent<Rigidbody>(out var rb))
            {
                Vector3 direction = owner.transform.forward;
                float moveDistance = data.speed * Time.fixedDeltaTime;

                // Check for collision before moving
                if (rb.SweepTest(direction, out RaycastHit hit, moveDistance))
                {
                    // Collision detected, cancel lunge
                    
                    active = false;
                    actionDuration = 0;
                   
                }

                // No collision, proceed with movement
                Vector3 newPosition = rb.position + direction * moveDistance;
                rb.MovePosition(newPosition);
            }

    }

   
        
}


