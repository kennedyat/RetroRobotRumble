using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class BuildABotEntry : MonoBehaviour
{
    private ChassisType _maybeChassis;
    private ArmType _maybeArm;
    private LegType _maybeLegs;
    private PartCommonData _data;
    private bool _equipped;

    [SerializeField] private Image _image;
    [SerializeField] private TextMeshProUGUI _name;
    [SerializeField] private TextMeshProUGUI _description;

    public void Initialize(ScriptableObject part, bool equipped)
    {
        _maybeChassis = null;
        _maybeArm = null;
        _maybeLegs = null;

        _equipped = equipped;

        if (part is ChassisType chassis)
        {
            _maybeChassis = chassis;
            _data = chassis.partCommonData;
        }
        else if (part is ArmType arm)
        {
            _maybeArm = arm;
            _data = arm.partCommonData;
        }
        else if (part is LegType leg)
        {
            _maybeLegs = leg;
            _data = leg.partCommonData;
        }

        _image.sprite = _data.spriteBuildABot;
        _name.text = _data.name;
        _description.text = _data.description;
    }

    public bool PartIsChassis()
    {
        return _maybeChassis is not null;
    }

    public bool PartIsArm()
    {
        return _maybeArm is not null;
    }

    public bool PartIsLegs()
    {
        return _maybeLegs is not null;
    }

    public void SetEquipped(bool equipped)
    {
        _equipped = equipped;
    }

    public void DoEquip(IGetSetPlayerEquips callback, Robot.Slot slot)
    {
        switch (slot)
        {
            case Robot.Slot.CHASSIS:
                callback.SetChassis(_maybeChassis);
                break;
            case Robot.Slot.LEFT_ARM:
                callback.SetLeftArm(_maybeArm);
                break;
            case Robot.Slot.RIGHT_ARM:
                callback.SetRightArm(_maybeArm);
                break;
            case Robot.Slot.LEGS:
                callback.SetLegs(_maybeLegs);
                break;
        }
    }
}
