using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Final Boss/GM1")]
public class Gungnir_M1 : FinalBossAttackData
{
    [Header("Lance Charge Stats")]
    public int chargeCount;
    public float trackingLetGo;
    public float chargeDelay;
}
