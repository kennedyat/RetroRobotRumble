using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.VFX;
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
    protected void Start()
    {
        TEMP_EnemyHPBar.maxValue = TEMP_HP;
        TEMP_EnemyHPBar.value = TEMP_HP;
        DOTween.Init();
    }

    // Update is called once per frame

    protected void OnTriggerEnter(Collider collision)
    {
        if (collision.gameObject.CompareTag("PlayerProjectile"))
        {
            Debug.Log($"Nope! This enemy got hit by {collision.name}");
            hitEffect.Play();
            StartCoroutine(nameof(ShowDamageNumbers));
            TEMP_HP -= damage;
            if (TEMP_HP <= 0)
            {
                StartCoroutine(nameof(ShowBoom));
                TEMP_HP = 0;
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
        Destroy(this.gameObject);
    }

    IEnumerator ShowDamageNumbers()
    {
        yield return new WaitForSecondsRealtime(0.1f);
        GameObject DamageNumberCopy = Instantiate(TEMPDamageNumber, EnemyCanvas.transform, false);
        DamageNumberCopy.GetComponent<DamageNumber>().duration = duration;
        yield return new WaitForSecondsRealtime(duration);
        //DOTween.KillAll();
        Destroy(DamageNumberCopy);
    }

    // get accessor
    public int GetHealth() { return TEMP_HP; }
}
