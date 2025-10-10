using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;

public class BAB_NotebookUI : MonoBehaviour
{
    private RectTransform rectTransform;

    private void Start()
    {
        rectTransform = GetComponent<RectTransform>();
    }

    public void EnableNotebook(GameObject selectedPart)
    {
        rectTransform.DOAnchorPos(new Vector2(54, -13), 0.5f);
        rectTransform.DORotate(new Vector3(0, 0, -3), 0.5f);
        Transform paper;

        if (selectedPart.CompareTag("BAB_Arm"))
        {
            BAB_ArmPrefab armInfo = selectedPart.GetComponent<BAB_ArmPrefab>();

            paper = transform.GetChild(0);
            paper.gameObject.SetActive(true);

            TMP_Text[] textFields = paper.GetComponentsInChildren<TMP_Text>();

            textFields[0].text = armInfo._armName;
            textFields[1].text = armInfo._armDescription;
            textFields[2].text = armInfo._basicName;
            textFields[3].text = armInfo._basicDescription;
            textFields[4].text = armInfo._specialName;
            textFields[5].text = armInfo._specialDescription;
        }
        if (selectedPart.CompareTag("BAB_Chassis"))
        {
            BAB_ChassisPrefab chassisInfo = selectedPart.GetComponent<BAB_ChassisPrefab>();

            paper = transform.GetChild(1);
            paper.gameObject.SetActive(true);

            TMP_Text[] textFields = paper.GetComponentsInChildren<TMP_Text>();

            textFields[0].text = chassisInfo._chassisName;
            textFields[1].text = chassisInfo._chassisDescription;
            textFields[2].text = chassisInfo._passiveName;
            textFields[3].text = chassisInfo._passiveDescription;
            textFields[4].text = chassisInfo._ultimateName;
            textFields[5].text = chassisInfo._ultimateDescription;
        }
        if (selectedPart.CompareTag("BAB_Legs"))
        {
            BAB_LegsPrefab legsInfo = selectedPart.GetComponent<BAB_LegsPrefab>();

            paper = transform.GetChild(2);
            paper.gameObject.SetActive(true);

            TMP_Text[] textFields = paper.GetComponentsInChildren<TMP_Text>();

            textFields[0].text = legsInfo._legsName;
            textFields[1].text = legsInfo._legsDescription;
            textFields[2].text = legsInfo._passiveName;
            textFields[3].text = legsInfo._passiveDescription;
        }
    }

    public void DisableNotebook()
    {
        rectTransform.DOAnchorPos(new Vector2(-58, 5), 0.5f);
        rectTransform.DORotate(new Vector3(0, 0, 7), 0.5f);

        foreach (Transform child in transform)
        {
            child.gameObject.SetActive(false);
        }
    }
}
