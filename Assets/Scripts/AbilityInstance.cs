using UnityEngine;

public class AbilityInstance
{
    public Abilities ability;

    private float cooldownTimer;
    private float durationTimer;
    private int _animIDAbility;

    private GameObject _player;
    private Animator _anim;

    public bool inEffect;

    public AbilityInstance(Abilities ability)
    {
        this.ability = ability;

        if (!string.IsNullOrEmpty(ability.animationTrigger))
            _animIDAbility = Animator.StringToHash(ability.animationTrigger);
    }

    public bool IsReady => cooldownTimer <= 0;

    public void Activate(GameObject player)
    {
        if (!IsReady) return;

        _player = player;
        _anim = _player.GetComponent<Animator>();

        ability.onHit = false;
        inEffect = false;



        cooldownTimer = ability.cooldown;
        durationTimer = ability.duration;
        PlayAnimation();
        PlayVFX();
        Debug.Log("How many");
    }

    public void TickTimers(float deltaTime)
    {
        cooldownTimer = Mathf.Max(0, cooldownTimer - deltaTime);

        if (durationTimer > 0)
        {
            durationTimer -= deltaTime;

            if (ability.isAction && _player != null)
            {
                ability.Effect(_player, _player); // Call ability's effect

            }

        }
        else
        {
            inEffect = false;
        }
    }

    public void TryTriggerEffect(GameObject target)
    {
        if (durationTimer <= 0 || _player == null) return;

        if (!ability.isAction)
            ability.Effect(target, _player);

        if (target.CompareTag("Enemy")) // Optional: make tags configurable via ScriptableObject
        {
            ability.onHit = true;
            inEffect = true;
        }
    }

    private void PlayAnimation()
    {
        if (_anim && ability.animationTrigger != "")
        {
            _anim.SetTrigger(_animIDAbility);
        }
    }

    private void PlayVFX()
    {
    

         if (ability.vfx == null || _player == null) return;

        Transform hand = GetLeftHand();

        Vector3 spawnPos = hand != null ? hand.position : _player.transform.position;

        GameObject vfxEffect = GameObject.Instantiate(ability.vfx, spawnPos, _player.transform.rotation, hand);
        GameObject.Destroy(vfxEffect, 1f);
    }
    
    private Transform GetLeftHand() //Reokace w/ something more modular
{
    if (_player.TryGetComponent<Animator>(out Animator anim))
    {
        return anim.GetBoneTransform(HumanBodyBones.LeftHand);
    }

    return null;
}
}
