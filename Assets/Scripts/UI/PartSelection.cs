using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

public class PartSelection : MonoBehaviour
{

    [SerializeField] VictoryScreenController victoryScreenController;
    [SerializeField] Image background;
    [SerializeField] RectTransform blindBox;

    private int partsSelected = 0;

    public IEnumerator BeginSelection(int partCount)
    {
        yield return new WaitForSeconds(3f);
        Debug.Log("PartSelection: Begin Selection of " + partCount + " parts");
        background.DOFade(0.25f, 1f);
        yield return new WaitForSeconds(1f);
        blindBox.DOLocalMoveY(-475, 1f);

        yield return new WaitForSeconds(0.5f);

        while (partsSelected < partCount)
        {
            yield return StartCoroutine(OfferChoices());
        }

        yield return null;
    }
    
    IEnumerator OfferChoices()
    {
        Debug.Log("PartSelection: Offer choices, round " + partsSelected);
        partsSelected++;

        yield return new WaitForSeconds(0.5f);

        yield return null;
    }
}
