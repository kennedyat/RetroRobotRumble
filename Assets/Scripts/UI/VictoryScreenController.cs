using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class VictoryScreenController : MonoBehaviour
{
    [Header("Screen References")]
    [SerializeField] GameObject rewardGrid;
    [SerializeField] GameObject rewardPrefab;
    
    [Header("Sticker Selection")]
    [SerializeField] private int stickerMin = 2;
    [SerializeField] private int stickerMax = 7;

    [Header("Awards")]
    [SerializeField] Transform[] awardTransforms;
    
    [Header("Dependencies")]
    [SerializeField] private ProgressionManager progressionManager;

    [Header("Juice")]
    [SerializeField] int wobbleFactor = 3;

    //private List<Sticker> currentStickerChoices = new List<Sticker>();
    //private List<Button> spawnedButtons = new List<Button>();

    public void StartVictorySequence()
    {
        PopulateRewards();
        DisplayAwards();
        //StartCoroutine(VictoryCoroutine());
    }

    IEnumerator VictoryCoroutine()
    {
        yield return null;
    }

    private void PopulateRewards()
    {
        int stickerAmount = UnityEngine.Random.Range(stickerMin, stickerMax);
        for (int index = 0; index < stickerAmount; index++)
        {
            Sticker sticker = progressionManager.GetUnlockSticker();

            if (sticker != null)
            {
                GameObject stickerReward = Instantiate(rewardPrefab, rewardGrid.transform);
                stickerReward.transform.DOLocalRotate(new Vector3(0, 0, CalculateWobble()), 0.5f).SetEase(Ease.InOutBack);
                stickerReward.GetComponent<Image>().sprite = sticker.stickerSprite;
                progressionManager.UnlockSticker(sticker);
            }
        }
        
        PartType unlockedPart = progressionManager.GetUnlockedPart();        
        if (unlockedPart != null && unlockedPart.partSprite != null)
        {
            GameObject partReward = Instantiate(rewardPrefab, rewardGrid.transform);            
            partReward.transform.DOLocalRotate(new Vector3(0, 0, CalculateWobble()), 0.5f).SetEase(Ease.InOutBack);            
            partReward.GetComponent<Image>().sprite = unlockedPart.partSprite;
            partReward.transform.GetChild(0).gameObject.SetActive(true);
        }

    }

    private void DisplayAwards()
    {
        foreach (Transform award in awardTransforms)
        {
            award.DOLocalRotate(new Vector3(0, 0, CalculateWobble()), 0.5f).SetEase(Ease.InOutBack); 
        }
    }

    private int CalculateWobble()
    {
        int direction = 1;

        if (UnityEngine.Random.Range(0, 2) <= 0)
        {
            direction = -1;
        }

        return UnityEngine.Random.Range(1, 4) * wobbleFactor * direction;
    }

    /*
    private void ShowStickerSelection()
    {
        stickerSelectionScreen.SetActive(true);
        partUnlockScreen.SetActive(false);
        
        PopulateStickerChoices();
    }

    private void PopulateStickerChoices()
    {
        // Clear old stickers
        currentStickerChoices.Clear();
        foreach (Button btn in spawnedButtons)
        {
            Destroy(btn.gameObject);
        }
        spawnedButtons.Clear();

        // Generate new stickers
        int stickerAmount = UnityEngine.Random.Range(stickerMin, stickerMax);
        for(int index = 0; index < stickerAmount; index++)
        {
            Sticker sticker = progressionManager.GetUnlockSticker();
            
            if (sticker != null)
            {
                currentStickerChoices.Add(sticker);
                
                Button button = Instantiate(buttonPrefab, stickerSelectionScreen.transform);
                
                // Set the button's image to the sticker sprite
                Image buttonImage = button.GetComponent<Image>();
                if (buttonImage != null && sticker.stickerSprite != null)
                {
                    buttonImage.sprite = sticker.stickerSprite;
                }
                
                // Add click listener
                int capturedIndex = index;
                button.onClick.AddListener(() => OnStickerSelected(capturedIndex));
                
                spawnedButtons.Add(button);
            }
        }
    }

    private void OnStickerSelected(int choiceIndex)
    {
        if (choiceIndex >= 0 && choiceIndex < currentStickerChoices.Count)
        {
            Sticker selectedSticker = currentStickerChoices[choiceIndex];
            progressionManager.UnlockSticker(selectedSticker);
        }
        
        ShowPartUnlock();
    }

    private void ShowPartUnlock()
    {
        stickerSelectionScreen.SetActive(false);
        partUnlockScreen.SetActive(true);
        
        DisplayUnlockedPart();
    }

    private void DisplayUnlockedPart()
    {
        PartType unlockedPart = progressionManager.GetUnlockedPart();
        
        if (unlockedPart != null && unlockedPart.partSprite != null)
        {
            partUnlockImage.sprite = unlockedPart.partSprite;
            
            foreach (GameObject ui in partUnlockUIElements)
            {
                ui.SetActive(true);
            }
        }
    }

    public void OnContinueFromPartUnlock()
    {
        RunData.EndCurrentRun();
    }
    */
}