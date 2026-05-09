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
    [SerializeField] RectTransform stickerGrid;
    [SerializeField] RectTransform stickerBuffs;
    [SerializeField] RectTransform notebook;

    void Start()
    {
        StartSequence();
    }

    void StartSequence()
    {
        background.DOFade(0.4f, duration).SetEase(Ease.OutQuart);
        hand.DORotate(new Vector3(0, -152, -90), duration).SetEase(Ease.OutBack);
        stickerPicker.DOMove(new Vector3(-0.2f, 15, -2.75f), duration).SetEase(Ease.OutCirc);

        stickerGrid.DOAnchorPos(new Vector2(364, -22), duration * 1.5f).SetEase(Ease.InOutQuart);
        stickerGrid.DOLocalRotate(new Vector3(0, 0, -8), duration * 1.5f).SetEase(Ease.InOutExpo);

        stickerBuffs.DOAnchorPos(new Vector2(-15, 300), duration * 1.6f).SetEase(Ease.InOutExpo);

        notebook.DOAnchorPos(new Vector2(-440, -200), duration).SetEase(Ease.InOutCirc);
        notebook.DOLocalRotate(new Vector3(0, 0, -12), duration).SetEase(Ease.InOutCirc);
    }

    void EndSequence()
    {
        
    }
}
