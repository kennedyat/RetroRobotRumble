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
    [SerializeField] private GameObject DamageNumber;

    [SerializeField] private float duration = 1.0f;

    public float currentHealth;
    private float lastDamageTaken = 0f;
    void Awake()
    {
        
    }
    void Start()
    {
        healthBar.maxValue = maxHealth;
        healthBar.value = maxHealth;

        currentHealth = maxHealth;
        

        ModifyHealth(StickerBehavior.Instance.GetMaxHealthBonus());
        
        DOTween.Init();
    }

    public void ModifyHealth(int addedHealth)
    {
        healthBar.maxValue += addedHealth;
        healthBar.value += addedHealth;
        currentHealth +=addedHealth;
        maxHealth += addedHealth;
        healthText.text = currentHealth + " / " + maxHealth;
    }
    public void TakeDamage(float amount)
    {
        lastDamageTaken = amount;
        hitEffect.Play();
        StartCoroutine(nameof(ShowDamageNumbers));

        currentHealth -= amount;
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
    }

    IEnumerator ShowDamageNumbers()
    {
        yield return new WaitForSecondsRealtime(0.1f);

        GameObject copy = Instantiate(DamageNumber, PlayerCanvas.transform, false);
        DamageNumber dmgComponent = copy.GetComponent<DamageNumber>();
        if (dmgComponent != null)
        {
            dmgComponent.duration = duration;
            dmgComponent.SetDamage(lastDamageTaken);
        }


        yield return new WaitForSecondsRealtime(duration);

        Destroy(copy);
    }
}
