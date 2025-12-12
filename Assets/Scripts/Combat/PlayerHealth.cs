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

    void Start()
    {
        healthBar.maxValue = maxHealth;
        healthBar.value = maxHealth;

        currentHealth = maxHealth;
        DOTween.Init();
    }

    public void TakeDamage(int amount)
    {
        hitEffect.Play();

        // probably want to add some damage resist calculations here
        // int realDamage = 
        StartCoroutine(ShowDamageNumbers(amount));

        currentHealth -= amount;
       
        //BarkManager.Instance.StartBark("Enemy_Happy", "Fleck_Upset");

        healthBar.value = currentHealth;
        healthText.text = currentHealth + " / " + maxHealth;

        if (currentHealth <= 0)
        {
            // Uhh we should probs have something for when player dies AF
        }
    }

    IEnumerator ShowDamageNumbers(int incomingDamage)
    {
        yield return new WaitForSecondsRealtime(0.1f);

        GameObject copy = Instantiate(DamageNumber, PlayerCanvas.transform, false);
        DamageNumber dmgComponent = copy.GetComponent<DamageNumber>();
        dmgComponent.duration = duration;
        dmgComponent.SetDamage(incomingDamage);
        dmgComponent.ShowNumber();

        yield return new WaitForSecondsRealtime(duration);
        Destroy(copy);
    }
}
