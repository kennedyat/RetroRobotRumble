using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PartSelectPrefab : MonoBehaviour
{
    public PartType part;
    public Image partSprite;
    public TextMeshProUGUI partInfo;
    public GameObject newNotif;

    public void Populate(PartType partType)
    {
        part = partType;
        if (part != null)
        {
            partSprite.sprite = part.partSprite;
            partInfo.text = part.partCommonData.name;
        }
    }

    public void Unbox()
    {
        Debug.Log("unboxed!");
    }
}
