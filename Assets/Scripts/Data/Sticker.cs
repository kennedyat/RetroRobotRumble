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
    public Vector3 handPosition;
    public Vector3 handRotation;
    public float activationDuration = 2f;

    public virtual void Activate(PartContext context)
    {
        StickerBehavior.Instance?.ActivateTemporary(this, activationDuration);
    }
}
