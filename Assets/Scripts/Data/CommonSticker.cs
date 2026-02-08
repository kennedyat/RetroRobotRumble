using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "CommonSticker", menuName = "ScriptableObjects/Stickers/CommonSticker", order = 0)]
public class CommonSticker : Sticker
{
    // Start is called before the first frame update
    public int attackDamage;
    public int criticalChance;
    public int attackSpeed;
    public int specialCooldown;


}
