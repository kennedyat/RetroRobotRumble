using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public partial class BuildABotDropTarget : MonoBehaviour
{
    [SerializeField] private Robot.Slot _part;
    [SerializeField] private BuildABotEntry _equipped; // Nullable!

    // Nullable!
    public void Initialize(BuildABotEntry entry)
    {
        _equipped = entry;

        if (entry is not null)
        {
            DoEquip(entry);
        }
    }

    private void DoEquip(BuildABotEntry entry)
    {
        BuildABotEntryImage tempThing2 = entry.GetComponentInChildren<BuildABotEntryImage>();
        Image tempThing = tempThing2.GetComponent<Image>();
        GetComponent<Image>().sprite = tempThing.sprite;

        _equipped?.SetEquipped(false);
        entry.SetEquipped(true);
        _equipped = entry;

        _equipped.DoEquip2(_part);
    }
}

public partial class BuildABotDropTarget : IDropHandler
{
    public void OnDrop(PointerEventData eventData)
    {
        Debug.Log("Dropped!");
        BuildABotEntry entry = eventData.pointerDrag.GetComponentInParent<BuildABotEntry>();

        if (_part == Robot.Slot.CHASSIS && entry.PartIsChassis())
        {
            DoEquip(entry);
        }
        else if (_part == Robot.Slot.LEFT_ARM && entry.PartIsArm())
        {
            DoEquip(entry);
        }
        else if (_part == Robot.Slot.RIGHT_ARM && entry.PartIsArm())
        {
            DoEquip(entry);
        }
        else if (_part == Robot.Slot.LEGS && entry.PartIsLegs())
        {
            DoEquip(entry);
        }
    }
}
