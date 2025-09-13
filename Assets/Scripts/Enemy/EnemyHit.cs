using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.VFX;
using UnityEngine.UI; 
using DG.Tweening;
using TMPro; 
public class EnemyHit : MonoBehaviour
{

    [SerializeField] private GameObject EnemyCanvas; 
    [SerializeField] private int TEMP_HP = 100;
    [SerializeField] private Slider TEMP_EnemyHPBar; 
    [SerializeField] private VisualEffect hitEffect;
    [SerializeField] private GameObject TEMPBoom;
    [SerializeField] private GameObject TEMPDamageNumber; 
    private int damage = 5;
    [SerializeField] private float duration = 1.0f; 
    private void Start()
    {
        TEMP_EnemyHPBar.maxValue = TEMP_HP;
        TEMP_EnemyHPBar.value = TEMP_HP;
        DOTween.Init(); 
    }

    // Update is called once per frame

    private void OnTriggerEnter (Collider collision)
    {
        if (collision.gameObject.tag == "Player")
        {
            Debug.Log($"Nope! This enemy got hit by {collision.name}");
            hitEffect.Play();
            StartCoroutine("ShowDamageNumbers");
            TEMP_HP -= damage;
            if (TEMP_HP <= 0)
            {
                StartCoroutine("ShowBoom");
                return;
            }
            TEMP_EnemyHPBar.value = TEMP_HP;
        }
    }

    IEnumerator ShowBoom()
    {
        TEMPBoom.SetActive(true);
        yield return new WaitForSecondsRealtime(2.0f);
        TEMPBoom.SetActive(false);
        this.DOKill();
    }

    IEnumerator ShowDamageNumbers()
    {
        yield return new WaitForSecondsRealtime(0.1f); 
        GameObject DamageNumberCopy = Instantiate(TEMPDamageNumber, EnemyCanvas.transform, false);
        DamageNumberCopy.GetComponent<DamageNumber>().duration = duration;
        yield return new WaitForSecondsRealtime(duration);
        DOTween.KillAll(); 
        Destroy(DamageNumberCopy); 
    }
}
