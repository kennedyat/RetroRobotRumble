using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BAB_ArmPrefab : MonoBehaviour
{
    [Tooltip("The name of the arm")]
    public string _armName = "Arm Name";

    [Tooltip("The description of the arm")]
    public string _armDescription = "Arm Description";
    
    [Tooltip("The name of the arm's basic attack")]
    public string _basicName = "Basic Attack Name";

    [Tooltip("The description of the arm's basic attack")]
    public string _basicDescription = "Basic Attack Description";
    
    [Tooltip("The name of the arm's special attack")]
    public string _specialName = "Special Attack Name";

    [Tooltip("The description of the arm's special attack")]
    public string _specialDescription = "Special Attack Description";

}
