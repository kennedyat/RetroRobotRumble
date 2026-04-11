using System.Collections;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.VFX;

public class PlayerHealth : MonoBehaviour
{
    [SerializeField] private GameObject PlayerCanvas;
    [SerializeField] private int maxHealth = 100;
    [SerializeField] private Slider healthBar;
    [SerializeField] private TextMeshProUGUI healthText;
    [SerializeField] private VisualEffect hitEffect;
    [SerializeField] private ParticleSystem healEffect;
    [SerializeField] private GameObject DamageNumber;
    

    [SerializeField] private float duration = 1.0f;

    public float currentHealth;
    private float lastDamageTaken = 0f;
    public GameObject HitStopManagerObject;
    private HitStopManager HSMScript;


    public bool IsInvulnerable { get; private set; } = false;
    public System.Action<float> OnDamageAttempted;
    public void SetInvulnerable(bool value)
    {
        IsInvulnerable = value;
    }

    protected void Awake()
    {
        HitStopManagerObject = GameObject.Find("CombatFeelManager");
        HSMScript = (HitStopManager)HitStopManagerObject.GetComponent(typeof(HitStopManager));

    }
    protected void Start()
    {
        healthBar.maxValue = maxHealth;
        healthBar.value = maxHealth;

        currentHealth = maxHealth;
        
        if(StickerBehavior.Instance!=null)
            ModifyMaxHealth(StickerBehavior.Instance.GetMaxHealthBonus());
        
        DOTween.Init();
    }

    public void ModifyMaxHealth(int addedHealth)
    {
        healthBar.maxValue += addedHealth;
        healthBar.value += addedHealth;
        currentHealth +=addedHealth;
        maxHealth += addedHealth;
        healthText.text = currentHealth + " / " + maxHealth;
    }
    public void TakeDamage(float amount)
    {
        OnDamageAttempted?.Invoke(amount);
        if (IsInvulnerable)
        {
            return;
        }

        float damageTaken = amount;

        if (StickerBehavior.Instance != null)
        {
            float rawReducedDamage = damageTaken * (StickerBehavior.Instance.GetDamageResBonus() / 100f);
            int adjustedReducedDamage = Mathf.CeilToInt(rawReducedDamage);

            if (adjustedReducedDamage < 0)
            {
                adjustedReducedDamage = 0;
            }

            damageTaken -= adjustedReducedDamage;
        }

        lastDamageTaken = damageTaken;
        hitEffect.Play();
        StartCoroutine(nameof(ShowDamageNumbers));

        currentHealth -= damageTaken;
        if(BarkManager.Instance != null)
            BarkManager.Instance.StartBark("Enemy_Happy", "Fleck_Upset");
        
        if (currentHealth < 0)
        {
            currentHealth = 0;
        }
            
        healthBar.value = currentHealth;
        healthText.text = currentHealth + " / " + maxHealth;

        if (currentHealth <= 0)
        {
            // Uhh we should probs have something for when player dies AF
        }

        //Massive HP Loss Hitstop
        if ((int)amount >= (int)20)
        {
            //UnityEngine.Debug.Log($"we triggered Hit Stop");
            HSMScript.hitStopinitiator(.5f);
        }


        //IFRAMES Check
        if ((int)amount >= (int)10)
        {
            //IFRAMES BLOCK
            //GetComponent<CapsuleCollider>().enabled = false;
            //HSMScript.IFrameinitiator(2f);
            //GetComponent<CapsuleCollider>().enabled = true;
        }


    }

    public void AddHealing(int amount)
    {
        if (currentHealth > 0 && currentHealth < maxHealth && amount > 0)
        {
            currentHealth += amount;
            if (currentHealth > maxHealth)
            {
                currentHealth = maxHealth;
            }         
            healthBar.value = currentHealth;
            healthText.text = currentHealth + " / " + maxHealth;
            
            healEffect.Play();
        }
    }

    IEnumerator ShowDamageNumbers()
    {
        yield return new WaitForSecondsRealtime(0.1f);

        GameObject copy = Instantiate(DamageNumber, PlayerCanvas.transform, false);
        DamageNumber dmgComponent = copy.GetComponent<DamageNumber>();
        if (dmgComponent != null)
        {
            dmgComponent.duration = duration;
            dmgComponent.SetDamage(lastDamageTaken, false);
        }


        yield return new WaitForSecondsRealtime(duration);

        Destroy(copy);
    }
}
