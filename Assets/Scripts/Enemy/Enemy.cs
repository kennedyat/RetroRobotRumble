using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.VFX;
using DG.Tweening;
using TMPro;
using Cinemachine;

[RequireComponent(typeof(Rigidbody))]
public abstract class Enemy : MonoBehaviour
{
    #region Variables/References
    [Header("General Enemy Stats")]
    [SerializeField, Tooltip("A reference to the player's position")]
    protected Transform player;
    [SerializeField, Tooltip("A reference to this enemy's rigidbody, used for movements")]
    protected Rigidbody rb;
    [SerializeField, Tooltip("Move speed of this enemy")]
    protected float moveSpeed;
    [SerializeField, Tooltip("The health of this enemy")]
    protected int health;
    [SerializeField, Tooltip("The damage this enemy deals with whatever it attacks with")]
    protected int attackDamage;
    [SerializeField, Tooltip("The range this enemy needs to be within to initiate its attack")]
    protected float attackRange;
    [SerializeField, Tooltip("For the enemy spawner, the amount of points it needs to spawn this enemy")]
    protected int spawnCost;
    public int GetSpawnCost() { return spawnCost; }

    [Header("Health UI")]
    [SerializeField] protected GameObject EnemyCanvas;
    [SerializeField] protected Slider TEMP_EnemyHPBar;
    [SerializeField] protected VisualEffect hitEffect;
    [SerializeField] protected GameObject TEMPBoom;
    [SerializeField] protected GameObject TEMPDamageNumber;
    [SerializeField] protected float duration;

    //Imma just add all 'combat feel' (hitstop, white flash, base knockback) here.
    //Enemy stun and other CC effects would require more complex work. I'll leave them be, for now
    [Header("Combat Feel")]
    [SerializeField] protected CinemachineImpulseSource ImpulseSource;
    [SerializeField] protected float DefaultScreenshakeForce = 0.05f;
    [SerializeField] protected float DeathScreenshakeForce = 0.2f;
    [SerializeField] protected float GlobalHitstopTime = 0.02f;
    [SerializeField] protected float DeathHitstopTime = 0.08f;
    [SerializeField] protected float DefaultWhiteflashTime = 0.05f;
    [SerializeField] protected float DeathWhiteflashTime = 0.2f;

    #endregion

    /// <summary>
    /// Gets a reference to the player, the attached rigidbody, and the health UI
    /// </summary>
    protected virtual void Start()
    {
        player = GameObject.FindWithTag("Player").transform;
        rb = GetComponent<Rigidbody>();
        TEMP_EnemyHPBar.maxValue = health;
        TEMP_EnemyHPBar.value = health;
        DOTween.Init();
    }

    /// <summary>
    /// Returns true if the player reference is null, or if this enemy has no health left. 
    /// Also calls DeathState() if enemy has no health left
    /// </summary>
    protected virtual bool Terminate()
    {
        if (player == null)
        {
            return true;
        }
        if (health <= 0)
        {
            DeathState();
            return true;
        }

        return false;
    }

    #region Combat
    /// <summary>
    /// Deals damage to this enemy, shows VFX, and destroys it if it has <= 0 health left
    /// </summary>
    /// <param name="damageToDeal">How much damage to deal</param>
    /// <returns>The amount of damage dealt, in case it was modified by damage amplification or resistance</returns>
    public virtual int DealDamage(int damageToDeal)
    {
        int realDamage = damageToDeal;

        // insert any damage more calculations here
        // realDamage = damageToDeal * damageResist * damageMultiplier;

        // nile told me (kevin) dont subtract for overkill damage
        // if player deals 10 to a 5 hp enemy count it as 10 not 5
        if (BarkManager.Instance != null)
            BarkManager.Instance.StartBark("Fleck_Happy", "Enemy_Upset");
        health -= realDamage;
        // destroy when we have no health left
        if (health <= 0)
        {
            ImpulseSource.GenerateImpulseWithForce(DeathScreenshakeForce);
            StartCoroutine(nameof(DeathHitstop));
            //Boom plays INSTEAD of hitEffect. Once we have a VFX for boom instead of UI, use .Play instead of coroutine. 
            StartCoroutine(nameof(ShowBoom));
        }
        // these "normal" effects should only play if the enemy isn't dead from that attack.
        else
        {
            // hit VFX
            hitEffect.Play();
            // also play screenshake
            ImpulseSource.GenerateImpulseWithForce(DefaultScreenshakeForce);
            // also hitstop
            StartCoroutine(nameof(GlobalHitstop));
            //Also show damage numbers
            StartCoroutine(nameof(ShowDamageNumbers));
        }




        // and update the health bar to match
        TEMP_EnemyHPBar.value = health;

        // use the return value if we need access to how much damage it did
        // like lifesteal calculations or damage trackers
        return realDamage;
    }

    /// <summary>
    /// When enemies die, they are not instantly destroyed. This function must be overriden to keep the enemy still and stop all behavior.
    /// </summary>
    protected abstract void DeathState();

    IEnumerator ShowBoom()
    {
        TEMPBoom.SetActive(true);
        yield return new WaitForSecondsRealtime(2.0f);
        TEMPBoom.SetActive(false);
        this.DOKill();
        Destroy(gameObject);
    }

    IEnumerator ShowDamageNumbers()
    {
        yield return new WaitForSecondsRealtime(0.1f);
        GameObject DamageNumberCopy = Instantiate(TEMPDamageNumber, EnemyCanvas.transform, false);
        DamageNumber reference = DamageNumberCopy.GetComponent<DamageNumber>();
        reference.duration = duration;
        reference.SetDamage(attackDamage);
        reference.ShowNumber();
        yield return new WaitForSecondsRealtime(duration);
        Destroy(DamageNumberCopy);
    }

    //This hitstop is called for every hit
    IEnumerator GlobalHitstop()
    {
        Time.timeScale = 0.0f;
        yield return new WaitForSecondsRealtime(GlobalHitstopTime);
        Time.timeScale = 1.0f;
    }

    //This hitstop is called when the enemy dies, punchier
    IEnumerator DeathHitstop()
    {
        Time.timeScale = 0.0f;
        yield return new WaitForSecondsRealtime(DeathHitstopTime);
        Time.timeScale = 1.0f;
    }
    #endregion
    
    #region Helper Functions
    // helper function
    /// <summary>
    /// Returns a Vector3 with the y variable set to the second parameter
    /// </summary>
    /// <param name="input">The vector to modify (will pass a copy)</param>
    /// <param name="set">The value to change y to</param>
    /// <returns>input.x, set, input.y</returns>
    protected static Vector3 SetY(Vector3 input, float set)
    {
        input.y = set;
        return input;
    }
    #endregion
}
