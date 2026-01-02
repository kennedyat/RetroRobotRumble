using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;

public class BAB_StickerSequence : MonoBehaviour
{
    [SerializeField] float duration = 1f;
    [SerializeField] SpriteRenderer background;
    [SerializeField] Transform hand;
    [SerializeField] Transform stickerPicker;
    [SerializeField] RectTransform page;

    void Start()
    {
        StartSequence();
    }

    void StartSequence()
    {
        background.DOFade(0.4f, duration).SetEase(Ease.OutQuart);
        hand.DORotate(new Vector3(0, -152, -90), duration).SetEase(Ease.OutBack);
        stickerPicker.DOMove(new Vector3(-0.2f, 15, -2.75f), duration).SetEase(Ease.OutCirc);

        page.DOAnchorPos(new Vector2(364, -22), duration * 1.5f).SetEase(Ease.InOutQuart);
        page.DORotate(new Vector3(0, 0, -8), duration * 1.5f).SetEase(Ease.InOutExpo);
    }

    void EndSequence()
    {
        
    }
}
