using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem.Interactions;
using UnityEngine.UI;

public class BuildABotEntry : MonoBehaviour
{
    private ChassisType _maybeChassis;
    private ArmType _maybeArm;
    private LegType _maybeLegs;
    private PartCommonData _data;
    private bool _equipped;
    private int _index;

    [SerializeField] private Image _image;
    [SerializeField] private TextMeshProUGUI _name;
    [SerializeField] private TextMeshProUGUI _description;

    public void Initialize(ScriptableObject part, int index)
    {
        _maybeChassis = null;
        _maybeArm = null;
        _maybeLegs = null;

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
        else
        {
            // My fault for downcasting.
            Debug.LogError("Invalid Scriptable Object was passed");
            _data = new PartCommonData
            {
                name = "This is a bug.",
                description = "Let someone know what happened before you saw this."
            };
        }

        _index = index;

        //_image.sprite = _data.spriteBuildABot;
        _name.text = _data.name;
        _description.text = _data.description;
    }

    public bool PartIsChassis()
    {
        return _maybeChassis != null;
    }

    public bool PartIsArm()
    {
        return _maybeArm != null;
    }

    public bool PartIsLegs()
    {
        return _maybeLegs != null;
    }

    public void SetEquipped(bool equipped)
    {
        _equipped = equipped;
        _name.color = _equipped ? Color.green : Color.white;
    }

    public void DoEquip2(Robot.Slot slot)
    {
        switch (slot)
        {
            case Robot.Slot.CHASSIS:
                // TODO
                break;
            case Robot.Slot.LEFT_ARM:
                RunData.currentRun.equippedLeftArm = _index;
                break;
            case Robot.Slot.RIGHT_ARM:
                RunData.currentRun.equippedRightArm = _index;
                break;
            case Robot.Slot.LEGS:
                // TODO
                break;
        }
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
