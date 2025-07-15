using System;
using UnityEngine;


[CreateAssetMenu(menuName = "ArmBehavior/Lunge")]
public class Lunge : ArmBehaviorData
{
    [Header("Lunge Parameters")]
    public float speed = 10f;
    public float duration = 0.3f;
    public float cooldown = 1.2f;

     public float knockbackDistance = 5f;
    public float knockbackSpeed = 5f;

    public override IArmBehavior MakeInstance()
    {
        return new LungeInstance { data = this };
    }
}
    [Serializable]
    public class LungeInstance : IArmBehavior
    {
        public Lunge data;
        public bool active;
        public float actionCooldown;
        public float actionDuration;
        

       

    public void Activate(GameObject owner, ArmInstance arm)
    {
        if (active || actionCooldown > 0) return;

        active = true;

        actionCooldown = data.cooldown;
        actionDuration = data.duration;
    }

    public void Deactivate(GameObject owner, ArmInstance arm)
    {
           
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
                //float moveDistance = 

            // Check for collision before moving
                if (rb.SweepTest(direction, out RaycastHit hit, 1f))
                {
                    // Collision detected, cancel lunge

                    if (hit.transform.tag == "Enemy" &&
                        hit.transform.TryGetComponent<Rigidbody>(out var enemyrb))
                    {

                        enemyrb.AddForce(rb.transform.forward * data.knockbackDistance * data.knockbackSpeed, ForceMode.Impulse);


                    }

                    active = false;
                    actionDuration = 0;
                
                    
                    }
                    
                    // No collision, proceed with movement
                    Vector3 newPosition = rb.position + direction * data.speed * Time.fixedDeltaTime;
                    rb.MovePosition(newPosition);
                }

    }

   
        
}


