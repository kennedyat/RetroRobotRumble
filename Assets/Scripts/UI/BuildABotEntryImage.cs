using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEditor.UI;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public partial class BuildABotEntryImage : MonoBehaviour
{

}

public partial class BuildABotEntryImage : IBeginDragHandler, IDragHandler, IEndDragHandler
{
    [SerializeField] private GameObject _drag;

    public void OnBeginDrag(PointerEventData eventData)
    {
        _drag.SetActive(true);

        // If this was called before, this does nothing.
        _drag.transform.SetParent(_drag.transform.root);

        var myImage = GetComponent<Image>();
        var copyImage = _drag.GetComponentInChildren<Image>();
        copyImage.sprite = myImage.sprite;
    }

    public void OnDrag(PointerEventData eventData)
    {
        _drag.transform.position = eventData.position;
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        _drag.SetActive(false);
    }
}
