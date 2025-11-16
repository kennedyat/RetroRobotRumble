using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;

public class BAB_EquipPart : MonoBehaviour
{
    [SerializeField, Tooltip("Color of sprite when a part is in the correct slot")] Color _correctSlotColor = Color.green;
    [SerializeField, Tooltip("Color of sprite when a part is in the wrong slot")] Color _wrongSlotColor = Color.red;
    [SerializeField, Tooltip("Color of sprite when a part is in the wrong slot")] Color _defaultColor = Color.white;

    [SerializeField] BAB_SelectPart selectPart;
    [SerializeField] GameObject partsParent;

    private SpriteRenderer sprite;
    private GameObject selectedPart;

    [HideInInspector] public GameObject equippedPart = null;

    void Start()
    {
        sprite = GetComponentInChildren<SpriteRenderer>();
    }

    void Update()
    {
        selectedPart = selectPart.selectedPart;

        if (selectedPart == null)
        {
            selectPart.activeSlot = null;
        }

    }

    void OnMouseEnter()
    {
        if (selectedPart != null)
        {
            selectPart.activeSlot = this.gameObject;
            if (selectedPart.CompareTag(this.gameObject.tag))
            {
                sprite.DOColor(_correctSlotColor, 0.25f);
            }
            else
            {
                sprite.DOColor(_wrongSlotColor, 0.25f);
            }
        }
    }

    void OnMouseExit()
    {
        ResetColor();
        selectPart.activeSlot = null;
    }

    public void ResetColor()
    {
        sprite.DOColor(_defaultColor, 0.25f);        
    }
}
