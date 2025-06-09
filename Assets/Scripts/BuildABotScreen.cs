using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BuildABotScreen : MonoBehaviour
{
    [SerializeField] private Image[] _mechDisplays;

    public void SetNewMechPart(int mechIndex, Sprite newDisplay)
    {
        _mechDisplays[mechIndex].sprite = newDisplay;
    }


}
