using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.VFX;

public class PartInstance : ICombatPart
{
    public PartComponentData data;
    private PartContext context;
    private CombatPartManager manager;

    public string PartName => data != null ? data.commonData.name : "Unknown";
    public PartState CurrentState { get; private set; } = PartState.Ready;
    public float RemainingCooldown { get; private set; }
    public float MaxCooldown { get; private set; }
    public float InternalCooldown { get; set; }

    private bool _isHeld = false;
    private float _heldDuration = 0f;

    [SerializeField] private bool blocksOtherAbilities = false;
    [SerializeField] private bool canBeBlocked = true;

    public bool CanUse
    {
        get
        {
            Debug.Log($"[PartInstance] Current State: {CurrentState} IsBlocking:{manager.IsAnyAbilityBlocking()} ");
            //if (manager != null && manager.IsAnyAbilityBlocking())
            //  return false;
            if (CurrentState != PartState.Ready)
                return false;

            return true;
        }
    }

    public bool BlocksOthers => blocksOtherAbilities && CurrentState == PartState.Active;

    public PartInstance(PartComponentData abilityData, PartContext ctx, CombatPartManager mgr, bool blocks = false, bool blocked = true)
    {
        data = abilityData;
        context = ctx;
        manager = mgr;
        blocksOtherAbilities = blocks;
        canBeBlocked = blocked;
        ctx.partInstance = this;

        if (data != null)
            MaxCooldown = data.cooldown;

        // Initialize components
        if (data != null && data.components != null)
        {
            foreach (var comp in data.components)
            {
                if (comp != null)
                    comp.Initialize(context);
            }
        }

        // Register with manager
        if (manager != null)
            manager.RegisterAbility(PartName, this);
    }

    // Called from ArmBehavior when button is pressed
    public void OnInputStarted(Animator animator)
    {

        Execute(animator);
    }

    // Called from ArmBehavior when button is released
    public void OnInputReleased()
    {

        if (data?.components != null)
        {
            foreach (var comp in data.components)
            {
                comp.OnReleased(context, comp.holdDuration);
            }
        }

        _heldDuration = 0f;
    }

    public void Execute(Animator animator)
    {
        if (MaxCooldown < manager.TimeBetweenAbilities)
            MaxCooldown = manager.TimeBetweenAbilities;

        // Prefer short InternalCooldown (e.g. Shinkansen combo) over full PartComponentData.cooldown
        RemainingCooldown = (InternalCooldown > 0) ? InternalCooldown : MaxCooldown;

        ChangeState(PartState.Active);

        //PlayAudio(data.audioClips, context.Owner.position);

        if (data != null && data.components != null)
        {
            foreach (var comp in data.components)
            {
                if (comp != null)
                    comp.OnExecute(context);
            }
        }

        if (data.animationTriggerName != null)
            PlayAnimation(animator, data.animationTriggerName);
        if (data.visualEffects != null)
            PlayVFX(data.visualEffects);
    }

    public void UpdateAbility(float deltaTime)
    {
        // Tick hold components every frame while button is held
        if (_isHeld)
        {
            _heldDuration += deltaTime;

            if (data?.components != null)
            {
                foreach (var comp in data.components)
                {
                    comp.OnHeld(context, _heldDuration, deltaTime);
                }
            }
        }

        // Update components 
        if (data != null && data.components != null)
        {
            foreach (var comp in data.components)
            {
                if (comp != null)
                    comp.OnUpdate(context, deltaTime);
            }
        }

        // Update cooldown
        if (RemainingCooldown > 0)
        {
            RemainingCooldown -= deltaTime;

            if (manager != null)
                manager.NotifyCooldownUpdated(this, RemainingCooldown);

            // Active to Cooldown transition
            if (CurrentState == PartState.Active && RemainingCooldown <= Mathf.Clamp
            (MaxCooldown - manager.TimeBetweenAbilities, 0, MaxCooldown))
                ChangeState(PartState.Cooldown);

            // End of Cooldown
            if (RemainingCooldown <= 0)
            {
                RemainingCooldown = 0;
                ChangeState(PartState.Ready);
            }
        }
    }

    public void ChangeState(PartState newState)
    {
        if (CurrentState == newState)
            return;

        var oldState = CurrentState;
        CurrentState = newState;

        if (manager != null)
            manager.NotifyStateChanged(this, oldState, newState);
    }

    private void PlayAnimation(Animator animator, string triggerName)
    {
        if (animator != null && !string.IsNullOrEmpty(triggerName))
            animator.SetTrigger(triggerName);
    }

    private void PlayVFX(VisualEffect[] vfxArray)
    {
        if (vfxArray != null)
        {
            foreach (var vfx in vfxArray)
            {
                if (vfx != null)
                    vfx.Play();
            }
        }
    }

    private void PlayAudio(AudioClip[] clips, Vector3 position)
    {
        if (clips != null && clips.Length > 0)
            AudioSource.PlayClipAtPoint(clips[0], position);
    }

    public void Cleanup()
    {
        if (manager != null)
            manager.UnregisterAbility(PartName);
    }
}