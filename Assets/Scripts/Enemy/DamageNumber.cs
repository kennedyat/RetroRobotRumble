using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using TMPro;
using UnityEngine;

public class DamageNumber : MonoBehaviour
{
    public float duration = 1.0f;
    [SerializeField] private TextMeshProUGUI damageText;
    private float damageValue = 0f;

    public void ShowNumber()
    {
        transform.DOScale(0f, duration).SetEase(Ease.OutSine);
        Vector3 randomPos = new Vector3(Random.Range(-1f, 1f), 2f, 0f);
        transform.DOLocalMove(randomPos, duration).SetEase(Ease.OutSine);
    }

    public void SetDamage(float amount)
    {
        damageValue = amount;
        if (damageText != null)
        {
            damageText.enableWordWrapping = false;
            damageText.text = Mathf.RoundToInt(amount).ToString();
        }
        else
        {
            Debug.LogWarning("No DT Assigned");
        }
    }
}
