using UnityEngine;

public class DragonScaleRuntime : MonoBehaviour
{
    [Header("Config")]
    public float baseCooldown = 90f;
    public float lingerAfterHit = 1.5f;

    // Remaining time until shield becomes active again
    public float RemainingCooldown { get; private set; }

    private bool shieldReady = false;      // shield is active & invuln (pre-hit)
    private bool shieldConsumed = false;   // got hit once, now lingering
    private float lingerTimer = 0f;

    // AUDIO
    [SerializeField] public AK.Wwise.Event ShieldOnSFX;
    [SerializeField] public AK.Wwise.Event ShieldOffSFX;

    private PlayerHealth ph;

    public void Initialize(PlayerHealth playerHealth, float cooldownSeconds, float lingerSeconds)
    {
        ph = playerHealth;
        baseCooldown = cooldownSeconds;
        lingerAfterHit = lingerSeconds;

        RemainingCooldown = baseCooldown;
        shieldReady = false;
        shieldConsumed = false;
        lingerTimer = 0f;

        // Subscribe to "damage instance attempted"
        ph.OnDamageAttempted -= OnDamageAttempted;
        ph.OnDamageAttempted += OnDamageAttempted;
    }

    public void Tick(float dt)
    {
        // If shield is up, keep invulnerable true
        if (shieldReady || shieldConsumed)
        {
            if (ph != null)
                ph.SetInvulnerable(true);
        }

        // Linger phase after first hit
        if (shieldConsumed)
        {
            lingerTimer -= dt;
            if (lingerTimer <= 0f)
            {
                shieldConsumed = false;
                shieldReady = false;

                if (ph != null)
                    ph.SetInvulnerable(false);

                // Start cooldown again
                RemainingCooldown = baseCooldown;

                //AUDIO SHIELD OFF
                ShieldOffSFX?.Post(gameObject);
            }
            return;
        }

        // Cooldown ticking while shield not active
        if (!shieldReady)

        {
            RemainingCooldown -= dt;
            if (RemainingCooldown <= 0f)
            {
                RemainingCooldown = 0f;
                shieldReady = true;

                if (ph != null)
                    ph.SetInvulnerable(true);

                // AUDIO SHIELD ON
                ShieldOnSFX?.Post(gameObject);

            }
        }
    }

    // Called when ANY player hit happens (reduces remaining cooldown by 1s)
    public void ReduceCooldown(float seconds)
    {
        if (shieldReady || shieldConsumed)
            return; // already active; spec only talks about recharge
        RemainingCooldown = Mathf.Max(0f, RemainingCooldown - seconds);
    }

    private void OnDamageAttempted(float dmg)
    {
        // If shield is ready (pre-hit), consume it on first damage instance
        if (shieldReady && !shieldConsumed)
        {
            shieldReady = false;
            shieldConsumed = true;
            lingerTimer = lingerAfterHit;

            // stays invulnerable during linger
            if (ph != null)
                ph.SetInvulnerable(true);
        }
    }

    private void OnDestroy()
    {
        if (ph != null)
            ph.OnDamageAttempted -= OnDamageAttempted;
    }
}
