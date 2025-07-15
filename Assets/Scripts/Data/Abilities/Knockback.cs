using System;
using UnityEngine;


[CreateAssetMenu(menuName = "Effects/Knockback")]
public class Knockback : StatusEffectData
{
    // Start is called before the first frame update

    [Header("Knockback Parameters")]
    public float knockbackDistance = 5f;
    public float knockbackSpeed = 5f;

    public override IEffect MakeInstance() => new Instance { knockback = this };


    [Serializable]
    public class Instance : IEffect
    {
        public Knockback knockback;

        public void ApplyEffect(EffectContext ctx)
        {
            
            if (ctx.target.tag == "Enemy")
            {
               
                if (ctx.target.TryGetComponent<Rigidbody>(out var rb))
                {
                   
                    rb.AddForce(ctx.source.transform.forward * knockback.knockbackDistance * knockback.knockbackSpeed, ForceMode.Impulse);
                }

            }
        }

        public void RemoveEffect()
        {

        }

        public void PerTick()
        {
            
        }

    }


}



    