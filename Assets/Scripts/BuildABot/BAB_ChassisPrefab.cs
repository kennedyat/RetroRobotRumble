using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BAB_ChassisPrefab : MonoBehaviour
{
    [Tooltip("The name of the chassis")]
    public string _chassisName = "Chassis Name";

    [Tooltip("The description of the chassis")]
    public string _chassisDescription = "Chassis Description";
    
    [Tooltip("The name of the chassis's passive ability")]
    public string _passiveName = "Passive Ability Name";

    [Tooltip("The description of the chassis's passive ability")]
    public string _passiveDescription = "Passive Ability Description";
    
    [Tooltip("The name of the chassis's ultimate ability")]
    public string _ultimateName = "Ultimate Ability Name";

    [Tooltip("The description of the chassis's ultimate ability")]
    public string _ultimateDescription = "Ultimate Ability Description";
}
