using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public partial class BuildABotDropTarget : MonoBehaviour
{
    [SerializeField] private Robot.Slot _part;
    [SerializeField] private BuildABotEntry _equipped; // Nullable!

    // Nullable!
    public void Initialize(BuildABotEntry equipped)
    {
        _equipped = equipped;

        if (equipped is not null)
        {
            BuildABotEntryImage tempThing2 = equipped.GetComponentInChildren<BuildABotEntryImage>();
            Image tempThing = tempThing2.GetComponent<Image>();
            GetComponent<Image>().sprite = tempThing.sprite;
        }
    }

    private void DoEquip(BuildABotEntry entry)
    {
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
        Image tempThing = eventData.pointerDrag.GetComponent<Image>();

        if (_part == Robot.Slot.CHASSIS && entry.PartIsChassis())
        {
            DoEquip(entry);
            GetComponent<Image>().sprite = tempThing.sprite;
        }
        else if (_part == Robot.Slot.LEFT_ARM && entry.PartIsArm())
        {
            DoEquip(entry);
            GetComponent<Image>().sprite = tempThing.sprite;
        }
        else if (_part == Robot.Slot.RIGHT_ARM && entry.PartIsArm())
        {
            DoEquip(entry);
            GetComponent<Image>().sprite = tempThing.sprite;
        }
        else if (_part == Robot.Slot.LEGS && entry.PartIsLegs())
        {
            DoEquip(entry);
            GetComponent<Image>().sprite = tempThing.sprite;
        }
    }
}
