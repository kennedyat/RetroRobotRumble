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
        BAB_PartPrefab partInfo = selectedPart.GetComponent<BAB_PartPrefab>();

        rectTransform.DOAnchorPos(new Vector2(54, -13), 0.5f);
        rectTransform.DOLocalRotate(new Vector3(0, 0, -3), 0.5f);
        Transform paper;

        if (selectedPart.CompareTag("BAB_Arm"))
        {

            paper = transform.GetChild(0);
            paper.gameObject.SetActive(true);

            TMP_Text[] textFields = paper.GetComponent<BAB_NotebookPage>().textFields;

            for (int i = 0; i < textFields.Length; i++)
            {
                textFields[i].text = partInfo.partInfo[i];
            }
        }
        if (selectedPart.CompareTag("BAB_Chassis"))
        {
            BAB_ChassisPrefab chassisInfo = selectedPart.GetComponent<BAB_ChassisPrefab>();

            paper = transform.GetChild(1);
            paper.gameObject.SetActive(true);

            TMP_Text[] textFields = paper.GetComponent<BAB_NotebookPage>().textFields;

            for (int i = 0; i < textFields.Length; i++)
            {
                textFields[i].text = partInfo.partInfo[i];
            }
        }
        if (selectedPart.CompareTag("BAB_Legs"))
        {
            BAB_LegsPrefab legsInfo = selectedPart.GetComponent<BAB_LegsPrefab>();

            paper = transform.GetChild(2);
            paper.gameObject.SetActive(true);

            TMP_Text[] textFields = paper.GetComponent<BAB_NotebookPage>().textFields;

            for (int i = 0; i < textFields.Length; i++)
            {
                textFields[i].text = partInfo.partInfo[i];
            }
        }
    }

    public void DisableNotebook()
    {
        rectTransform.DOAnchorPos(new Vector2(-58, 5), 0.5f);
        rectTransform.DOLocalRotate(new Vector3(0, 0, 7), 0.5f);

        foreach (Transform child in transform)
        {
            child.gameObject.SetActive(false);
        }
    }
}
