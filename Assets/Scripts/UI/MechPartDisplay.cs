using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class MechPartDisplay : MonoBehaviour, IDropHandler
{
    [SerializeField] private MechPartType mechPartType;
    public void OnDrop(PointerEventData eventData)
    {
        MechLimbUI mechLimbUI = eventData.pointerDrag.GetComponent<MechLimbUI>(); // This obtains the item the player is currently selecting
        if (mechLimbUI.CurrentPart.mechPartType == mechPartType)
        {
            GetComponent<Image>().sprite = mechLimbUI.CurrentPart.partSprite; // This updates the mech display
            mechLimbUI.SendSignal(); // This calls the signal for other methods to listen to
        }
    }
}
