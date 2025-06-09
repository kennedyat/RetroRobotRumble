using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class MechPartDisplay : MonoBehaviour, IDropHandler
{
    public void OnDrop(PointerEventData eventData)
    {
        GameObject dropped = eventData.pointerDrag;
        GetComponent<Image>().sprite = dropped.GetComponent<MechLimbUI>().CurrentPart.partSprite;
    }
}
