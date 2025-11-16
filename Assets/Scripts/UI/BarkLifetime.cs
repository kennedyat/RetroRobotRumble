using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;

[RequireComponent(typeof(RectTransform))]
public class BarkLifetime : MonoBehaviour
{
    [SerializeField] float lifetime = 2f;
    RectTransform rectTransform;
    void Start()
    {
        rectTransform = GetComponent<RectTransform>();
        StartCoroutine(CreateBark());
    }

    IEnumerator CreateBark()
    {
        rectTransform.anchoredPosition = new Vector2(1000, 0);
        //rectTransform.localScale = Vector3.one * 0.5f;
        
        GetComponent<RectTransform>().DOAnchorPosX(0, 0.5f, true).SetEase(Ease.OutExpo);
        //GetComponent<RectTransform>().DOScale(Vector3.one, 0.5f).SetEase(Ease.OutExpo);

        yield return new WaitForSeconds(lifetime);
        StartCoroutine(DestroyBark());
    }

    IEnumerator DestroyBark()
    {
        GetComponent<RectTransform>().DOAnchorPosX(-1000, 0.5f, true).SetEase(Ease.InExpo);
        GetComponent<RectTransform>().DOScale(Vector3.one * 0.5f, 0.5f).SetEase(Ease.InExpo);
        yield return new WaitForSeconds(0.5f);
        Destroy(this.gameObject);
    }
}
