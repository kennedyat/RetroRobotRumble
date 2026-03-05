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

    void Start()
    {
        if (part != null)
        {
            partSprite.sprite = part.partSprite;
            partInfo.text = part.name;
        }
    }
}
