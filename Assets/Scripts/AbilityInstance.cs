using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AbilityInstance
{
    // Start is called before the first frame update
    public Abilities ability;

    private Melee melee;
    public float cooldownTimer;
    public float durationTimer;
    public int _animIDAbility;
    private GameObject _player;
    private Animator _anim;

    public AbilityInstance(Abilities ability)
    {
        this.ability = ability;
        
        if (!string.IsNullOrEmpty(ability.animationTrigger))
            _animIDAbility = Animator.StringToHash(ability.animationTrigger);

        cooldownTimer = 0;
        durationTimer = 0;
    }

    public bool IsReady => cooldownTimer <= 0;

    public void Activate(GameObject player)
    {
        _player = player;

        _anim = _player.GetComponent<Animator>();

        if (_anim)
        {
            PlayAnimation();
        }

        if (!IsReady) return;

        if (ability.isAction)
            ability.Effect(_player);

        cooldownTimer = ability.cooldown;
        durationTimer = ability.duration;
    }

    public void TickTimers(float deltaTime)
    {
        if (cooldownTimer > 0)
            cooldownTimer -= deltaTime;

        if (durationTimer > 0)
        {
            durationTimer -= deltaTime;
            ability.Effect(_player);
        }

    }

    public void TryTriggerEffect(GameObject other)
    {
        if (!ability.isAction && durationTimer > 0)
        {
            ability.Effect(other);
        }
    }

    public void PlayAnimation()
    {
        if(cooldownTimer <= 0)
            _anim.SetTrigger(_animIDAbility);
    }

    public void PlayVFX(GameObject vfx)
    {
        if (ability.vfx != null)
        {
            GameObject vfxEffect = GameObject.Instantiate(ability.vfx, _player.transform.position, _player.transform.rotation);
            GameObject.Destroy(vfx, 5f); // Clean up after delay
        }
    }
}
