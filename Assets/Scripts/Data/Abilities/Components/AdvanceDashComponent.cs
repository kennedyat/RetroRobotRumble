using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "ScriptableObjects/Components/AdvanceDash")]
public class AdnvanceDashComponent : PartComponent
{
    [Header("Dash Settings")]
    public float dashSpeed = 15f;
    public float dashDuration = 0.3f;

    [Header("Multi-Dash Settings")]
    [Tooltip("Number of consecutive dashes")]
    public int dashCount = 1;
    [Tooltip("Time window after a dash to trigger the next one")]
    public float inputWindow = 0.5f;

    [Header("Modifiers")]
    public List<StickerModifierEntry> modifierEntries = new List<StickerModifierEntry>();

    // Runtime state
    private int dashesRemaining;
    private float dashTimeRemaining;
    private float inputWindowRemaining;
    private bool waitingForInput;
    private bool allDashesComplete;
    private Vector3 dashDirection;

    public override void Initialize(PartContext context)
    {
        dashesRemaining = 0;
        dashTimeRemaining = 0;
        inputWindowRemaining = 0;
        waitingForInput = false;
        allDashesComplete = true;
    }

    public override void OnExecute(PartContext context)
    {
        Debug.Log($"[DashComponent] Dash started, {dashesRemaining} remaining after this.");

        // Chained dash — player hit execute during the input window
        if (waitingForInput && dashesRemaining > 0)
        {
            waitingForInput = false;
            inputWindowRemaining = 0;
            StartDash(context);
            return;
        }

        // Fresh activation
        dashesRemaining = dashCount - 1; // -1 because we start the first immediately
        allDashesComplete = false;
        waitingForInput = false;
        StartDash(context);
    }

    public override void OnUpdate(PartContext context, float deltaTime)
    {
        if (allDashesComplete) return;

        // Currently dashing
        if (dashTimeRemaining > 0)
        {
            if (context.Rigidbody == null || context.Owner == null)
            {
                dashTimeRemaining = 0;
                return;
            }

            context.Rigidbody.MovePosition(context.Owner.position + dashDirection * dashSpeed * deltaTime);
            dashTimeRemaining -= deltaTime;
            return;
        }

        // Dash just finished, more remaining — open input window
        if (waitingForInput)
        {
            inputWindowRemaining -= deltaTime;

            if (inputWindowRemaining <= 0)
            {
                // Window expired, no more dashes
                waitingForInput = false;
                CompleteDashes(context);
            }
            return;
        }

        // Nothing active and not waiting — dashes are done
        if (!allDashesComplete)
            CompleteDashes(context);
    }

    private void StartDash(PartContext context)
    {
        Vector3 vel = context.Rigidbody.velocity;
        dashDirection = vel.magnitude > 0.1f ? vel.normalized : context.Owner.forward;
        context.Rigidbody.velocity = Vector3.zero;
        dashTimeRemaining = dashDuration;

        ActivateHitbox(context);
        ActivateModifiers(context, duringDash: true);

        Debug.Log($"[DashComponent] Dash started, {dashesRemaining} remaining after this.");

        // If more dashes available, open input window when this dash ends
        if (dashesRemaining > 0)
            QueueInputWindow();
    }

    private void QueueInputWindow()
    {
        // This gets picked up in OnUpdate once dashTimeRemaining hits 0
        waitingForInput = true;
        inputWindowRemaining = inputWindow;
    }

    private void CompleteDashes(PartContext context)
    {
        allDashesComplete = true;
        dashesRemaining = 0;

        ActivateModifiers(context, duringDash: false);

        // Delay cooldown start by the longest modifier duration so buff expires first
        float longestAfterDuration = 0f;
        foreach (var entry in modifierEntries)
        {
            if (!entry.isDuring && entry.sticker != null)
                longestAfterDuration = Mathf.Max(longestAfterDuration, entry.sticker.activationDuration);
        }

        if (longestAfterDuration > 0f)
            context.partInstance.InternalCooldown = context.partInstance.MaxCooldown + longestAfterDuration;

        Debug.Log($"[DashComponent] All dashes complete. Cooldown delayed by {longestAfterDuration}s.");
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