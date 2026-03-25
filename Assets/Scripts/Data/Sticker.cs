using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Sticker : ScriptableObject
{
    public string name;
    public Sprite stickerSprite;
    public Material decalMaterial;
    public string description;
    public float activationDuration = 2f;

    public virtual void Activate(PartContext context)
    {
        StickerBehavior.Instance?.ActivateTemporary(this, activationDuration);
    }
}
