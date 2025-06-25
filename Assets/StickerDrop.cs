using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.EventSystems;

public class StickerDrop : MonoBehaviour, IDragHandler, IDropHandler
{
    [SerializeField] private Canvas canvas; 
    [SerializeField] private RectTransform sticker; 
    public void OnDrag(PointerEventData data)
    {
        sticker.anchoredPosition += data.delta / canvas.scaleFactor; 
    }

    public void OnDrop(PointerEventData data)
    {

    }
}
