using System.Collections;
using System.Collections.Generic;
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
    [SerializeField] private GameObject TEMPDamageNumber;
    private int damage = 5;
    private float currentHealth;
    [SerializeField] private float duration = 1.0f;
    protected void Start()
    {
        healthBar.maxValue = maxHealth;
        healthBar.value = maxHealth;

        currentHealth = maxHealth;
        DOTween.Init();
    }

    // Update is called once per frame

    protected void OnTriggerEnter(Collider collision)
    {
        if (collision.gameObject.CompareTag("EnemyProjectile"))
        {

            hitEffect.Play();
            StartCoroutine(nameof(ShowDamageNumbers));
            currentHealth -= damage;
            if (currentHealth <= 0)
            {
                currentHealth = 0;
            }
            healthBar.value = currentHealth;
            healthText.text = currentHealth + " / " + maxHealth;
        }
    }



    IEnumerator ShowDamageNumbers()
    {
        yield return new WaitForSecondsRealtime(0.1f);
        GameObject DamageNumberCopy = Instantiate(TEMPDamageNumber, PlayerCanvas.transform, false);
       
        DamageNumberCopy.GetComponent<DamageNumber>().duration = duration;
        yield return new WaitForSecondsRealtime(duration);
        //DOTween.KillAll();
        Destroy(DamageNumberCopy);
    }
}
