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

    private float currentHealth;
    private float lastDamageTaken = 0f;

    void Start()
    {
        healthBar.maxValue = maxHealth;
        healthBar.value = maxHealth;

        currentHealth = maxHealth;
        DOTween.Init();
    }

    public void TakeDamage(float amount)
    {
        lastDamageTaken = amount;
        hitEffect.Play();
        StartCoroutine(nameof(ShowDamageNumbers));

        currentHealth -= amount;
       
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
