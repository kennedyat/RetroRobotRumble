using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Final Boss/GM1")]
public class Gungnir_M1 : FB_P1AttackData
{
    [Header("Lance Charge Stats")]
    public int chargeCount;
    public float trackingLetGo;
    public float chargeDelay;
    public float chargeSpeed;
}
