using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using Unity.VisualScripting;

public class PartSelection : MonoBehaviour
{

    [SerializeField] VictoryScreenController victoryScreenController;
    [SerializeField] ProgressionManager progressionManager;
    [SerializeField] Image background;
    [SerializeField] RectTransform blindBox;
    [SerializeField] GameObject partSelectPrefab;
    [SerializeField] List<GameObject> selectableParts;

    private int partsSelected = 0;

    void Start()
    {
        StartCoroutine(DisplayOptions());
    }

    IEnumerator DisplayOptions()
    {
        yield return new WaitForSeconds(2f);
        background.DOFade(0.25f, 1f);
        yield return new WaitForSeconds(1f);
        blindBox.DOLocalMoveY(-475, 1f);

        yield return new WaitForSeconds(1.5f);

        blindBox.DOShakeAnchorPos(2f, 50, 30);
        yield return new WaitForSeconds(1.5f);

        for (int i = 0; i < 3; i++)
        {
            GameObject instantiatedPart = Instantiate(partSelectPrefab, blindBox.position, blindBox.rotation, transform);
            selectableParts.Add(instantiatedPart);
            instantiatedPart.transform.localScale = Vector3.zero;
            
            PartSelectPrefab instantiatedComponent = instantiatedPart.GetComponent<PartSelectPrefab>();
            PartType randomPart = progressionManager.GetRandomPart();
            instantiatedComponent.Populate(randomPart);            
            
            switch (randomPart)
            {
                case ArmType arm:
                    if(RunData.availableArms.Contains(arm)) { instantiatedComponent.newNotif.SetActive(false); }
                    break;
                case ChassisType chassis:
                    if(RunData.availableChassis.Contains(chassis)) { instantiatedComponent.newNotif.SetActive(false); }
                    break;
                case LegType leg:
                    if(RunData.availableLegs.Contains(leg)) { instantiatedComponent.newNotif.SetActive(false); }
                    break;
            }
        }

        foreach (GameObject part in selectableParts)
        {
            part.transform.DOScale(1, 1f);
        }

        selectableParts[0].GetComponent<RectTransform>().DOAnchorPos(new Vector2(-500, 40), 1.1f);
        selectableParts[0].GetComponent<RectTransform>().DOLocalRotate(new Vector3(0, 0, 6), 1.2f);

        selectableParts[1].GetComponent<RectTransform>().DOAnchorPos(new Vector2(0, 120), 1.3f);

        selectableParts[2].GetComponent<RectTransform>().DOAnchorPos(new Vector2(500, 40), 1.4f);
        selectableParts[2].GetComponent<RectTransform>().DOLocalRotate(new Vector3(0, 0, -6), 1.5f);



        yield return null;
    }

    /*
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
    */
}
