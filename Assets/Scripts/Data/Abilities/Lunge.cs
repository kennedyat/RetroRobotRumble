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

        actionDuration = Mathf.Max(0, actionDuration - Time.fixedDeltaTime);

        if (owner.gameObject.TryGetComponent<Rigidbody>(out var rb))
        {
            if (actionDuration > 0 && actionCooldown > 0)
            {
                Vector3 pos = Vector3.Lerp(rb.position, rb.position + (owner.transform.forward * data.speed), Time.fixedDeltaTime);
                rb.MovePosition(pos);

            }
        
        }

    }

   
        
}


