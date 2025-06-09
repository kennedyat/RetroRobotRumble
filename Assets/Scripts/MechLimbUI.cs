using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using TMPro;

public class MechLimbUI : MonoBehaviour
{
    [SerializeField] Image _image;
    [SerializeField] TextMeshProUGUI _name;
    [SerializeField] TextMeshProUGUI _description;

    public UnityEvent SelectLimb; // Sends out signals that the current limb is selected

    private ChassisType CurrentChassis;
    private LimbType CurrentLimb;

    [SerializeField] ChassisType _example1; //delete this later
    [SerializeField] ChassisType _example2; //delete this later
    [SerializeField] ChassisType _example3; //delete this later
    [SerializeField] LimbType _example4; //delete this later
    [SerializeField] LimbType _example5; //delete this later
    [SerializeField] LimbType _example6; //delete this later
    [SerializeField] LimbType _example7; //delete this later

    private void Start()
    {
        var chassisList = new List<ChassisType> { _example1, _example2, _example3 };
        var limbList = new List<LimbType> { _example4, _example5, _example6, _example7 };

        bool chooseChassis = Random.value < 0.5f;

        if (chooseChassis)
        {
            ChassisType randomChassis = chassisList[Random.Range(0, chassisList.Count)];
            PopulateUI(randomChassis);
        }
        else
        {
            LimbType randomLimb = limbList[Random.Range(0, limbList.Count)];
            PopulateUI(randomLimb);
        }
    }

    public void PopulateUI(LimbType limbType)
    {
        switch (limbType.LimbIndex)
        {
            case 0: // left arm
                _image.sprite = limbType.leftArmData.leftArmSprite;
                _name.text = limbType.leftArmData.leftArmName;
                _description.text = limbType.leftArmData.leftArmDescription;
                CurrentLimb = limbType;
                break;
            case 1: // right arm
                _image.sprite = limbType.rightArmData.rightArmSprite;
                _name.text = limbType.rightArmData.rightArmName;
                _description.text = limbType.rightArmData.rightArmDescription;
                CurrentLimb = limbType;
                break;
            case 2: // legs
                _image.sprite = limbType.legsData.legsSprite;
                _name.text = limbType.legsData.legsName;
                _description.text = limbType.legsData.legsDescription;
                CurrentLimb = limbType;
                break;
            default:
                Debug.Log("No limb type assigned for: " + limbType);
                break;
        }
    }

    public void PopulateUI(ChassisType chassisType)
    {
        _image.sprite = chassisType.chassisSprite;
        _name.text = chassisType.chassisName;
        _description.text = chassisType.chassisDescription;
        CurrentChassis = chassisType;
    }

    public void SendSignal()
    {
        SelectLimb.Invoke();
        if (CurrentChassis)
        {
            //Set chassis display
            GameObject.FindGameObjectWithTag("BuildABotScreen").GetComponent<BuildABotScreen>().SetNewMechPart(0, CurrentChassis.chassisSprite);
        }
        else
        {
            // set a limb display
            switch (CurrentLimb.LimbIndex)
            {
                case 0: // left arm
                    GameObject.FindGameObjectWithTag("BuildABotScreen").GetComponent<BuildABotScreen>().SetNewMechPart(CurrentLimb.LimbIndex + 1, CurrentLimb.leftArmData.leftArmSprite);
                    break;
                case 1: // right arm
                    GameObject.FindGameObjectWithTag("BuildABotScreen").GetComponent<BuildABotScreen>().SetNewMechPart(CurrentLimb.LimbIndex + 1, CurrentLimb.rightArmData.rightArmSprite);
                    break;
                case 2: // legs
                    GameObject.FindGameObjectWithTag("BuildABotScreen").GetComponent<BuildABotScreen>().SetNewMechPart(CurrentLimb.LimbIndex + 1, CurrentLimb.legsData.legsSprite);
                    break;
                default:
                    Debug.Log("No limb type assigned for: " + CurrentLimb);
                    break;
            }
        }
    }

    public void selectedLimb()
    {
        if (CurrentChassis)
        {
            Debug.Log("Selected Limb:" + CurrentChassis.chassisName);
        }
        else
        {
            Debug.Log("Selected Limb:" + CurrentLimb);
        }
    }
}
