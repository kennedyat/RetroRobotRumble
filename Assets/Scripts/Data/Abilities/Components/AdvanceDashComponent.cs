using System.Collections.Generic;
using UnityEngine;
using Assets.Scripts.Combat.Robot;


[CreateAssetMenu(menuName = "ScriptableObjects/Components/AdvanceDash")]
public class AdnvanceDashComponent : PartComponent
{
    [Header("Dash Settings")]
    public float dashSpeed = 15f;
    public float dashDuration = 0.3f;

    [Header("Multi-Dash Settings")]
    [Tooltip("Number of consecutive dashes")]
    public int dashCount = 2;
    [Tooltip("Time window after a dash to trigger the next one")]
    public float inputWindow = 0.5f;

    [Header("Modifiers")]
    public List<StickerModifierEntry> modifierEntries = new List<StickerModifierEntry>();

    // Runtime state
    private bool isDashing;
    private bool waitingForInput;
    private int dashesRemaining;
    private float dashTimeRemaining;
    private float inputWindowRemaining;
    private Vector3 dashDirection;

    public override void Initialize(PartContext context)
    {
        isDashing = false;
        waitingForInput = false;
        dashesRemaining = 0;
        dashTimeRemaining = 0;
        inputWindowRemaining = 0;
    }

    public override void OnExecute(PartContext context)
    {
        if (waitingForInput && dashesRemaining > 0)
        {
            // Chained dash within window
            dashesRemaining--;
            inputWindowRemaining = 0;
            StartDash(context);
            return;
        }

        if (!isDashing && !waitingForInput)
        {
            // Fresh dash sequence
            dashesRemaining = dashCount - 1;
            StartDash(context);
        }
    }

    public override void OnUpdate(PartContext context, float deltaTime)
    {
        if (isDashing)
        {
            if (context.Rigidbody == null || context.Owner == null) return;

            context.Rigidbody.MovePosition(context.Owner.position + dashDirection * dashSpeed * deltaTime);
            dashTimeRemaining -= deltaTime;

            if (dashTimeRemaining <= 0)
            {
                isDashing = false;

                if (dashesRemaining > 0)
                {
                    // Open input window for next dash
                    waitingForInput = true;
                    inputWindowRemaining = inputWindow;
                    context.partInstance.ChangeState(PartState.Ready);
                }
                else
                {
                    CompleteDashes(context);
                }
            }
            return;
        }

        if (waitingForInput)
        {
            inputWindowRemaining -= deltaTime;

            if (inputWindowRemaining <= 0)
            {
                waitingForInput = false;
                CompleteDashes(context);
                context.partInstance.ChangeState(PartState.Cooldown);
            }
        }
    }

    private void StartDash(PartContext context)
    {
        var robot = context.Owner.GetComponentInParent<CombatRobot>();
        Vector3 moveInput = robot != null ? robot.worldspaceMoveInput : Vector3.zero;
        dashDirection = moveInput.magnitude > 0.1f ? moveInput.normalized : context.Owner.forward;

        dashTimeRemaining = dashDuration;
        isDashing = true;
        waitingForInput = false;

        ActivateHitbox(context);
        ActivateModifiers(context, duringDash: true);
        }

    private void CompleteDashes(PartContext context)
    {
        isDashing = false;
        waitingForInput = false;
        dashesRemaining = 0;

        ActivateModifiers(context, duringDash: false);

        float longestAfterDuration = 0f;
        foreach (var entry in modifierEntries)
        {
            if (!entry.isDuring && entry.sticker != null)
                longestAfterDuration = Mathf.Max(longestAfterDuration, entry.sticker.activationDuration);
        }

        if (longestAfterDuration > 0f)
            context.partInstance.InternalCooldown = context.partInstance.MaxCooldown + longestAfterDuration;

        Debug.Log($"[AdvanceDash] All dashes complete. Cooldown delayed by {longestAfterDuration}s.");
    }

    private void ActivateModifiers(PartContext context, bool duringDash)
    {
        foreach (var entry in modifierEntries)
        {
            if (entry.sticker == null) continue;
            if (entry.isDuring == duringDash)
                entry.sticker.Activate(context);
        }
    }
}

[System.Serializable]
public class StickerModifierEntry
{
    public Sticker sticker;
    [Tooltip("True = triggers on each dash, False = triggers after all dashes complete")]
    public bool isDuring = false;
}