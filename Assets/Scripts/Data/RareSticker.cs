using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "RareSticker", menuName = "ScriptableObjects/Stickers/RareSticker", order = 1)]

public class RareSticker : Sticker
{
   public int attackDamage;
    public int criticalChance;
    public int maxHealth;
    public int moveSpeed;
    public int attackSpeed;
    public int specialCooldown;
    public int ultimateCharge;

}
