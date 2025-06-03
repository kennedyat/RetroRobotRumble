using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class MechLimbUI : MonoBehaviour
{
    [SerializeField] Image image;
    [SerializeField] TextMeshProUGUI name;
    [SerializeField] TextMeshProUGUI description;

    [SerializeField] ChassisType example; //delete this later

    private void Start()
    {
        populateUI(example);
    }

    public void populateUI(LimbType limbType)
    {
        //limbType.
        // Need to figure out a good way to determine if it is right arm/left arm/leg
    }

    public void populateUI(ChassisType chassisType)
    {
        image.sprite = chassisType.chassisSprite;
        name.text = chassisType.chassisName;
        description.text = chassisType.chassisDescription;
    }
}
