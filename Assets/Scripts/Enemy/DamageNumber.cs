using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;

public class DamageNumber : MonoBehaviour
{
    public float duration = 1.0f;
    // Start is called before the first frame update
    void Start()
    {
        Debug.Log("damage number spawned");
        transform.DOScale(0f, duration).SetEase(Ease.OutSine);

        Vector3 randomPos = new Vector3(Random.Range(-1f, 1f), 2f, 0f);
        transform.DOLocalMove(randomPos, duration).SetEase(Ease.OutSine);
    }
}
