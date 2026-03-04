using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "LegendarySticker", menuName = "ScriptableObjects/Stickers/LegendarySticker", order = 2)]

public class LegendarySticker : Sticker
{
    public int lifesteal;
    public int damageRes;
    public int stickerBoost;
    public int holoDrop;
}
