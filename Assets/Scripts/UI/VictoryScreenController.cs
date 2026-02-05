using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class VictoryScreenController : MonoBehaviour
{
    [Header("Screen References")]
    [SerializeField] private GameObject stickerSelectionScreen;
    [SerializeField] private GameObject partUnlockScreen;
    
    [Header("Sticker Selection")]
    [SerializeField] private int stickerMin = 2;
    [SerializeField] private int stickerMax = 7;
    [SerializeField] private Button buttonPrefab;  // Add this!
    
    [Header("Part Unlock")]
    [SerializeField] private Image partUnlockImage;
    [SerializeField] private GameObject[] partUnlockUIElements;
    
    [Header("Dependencies")]
    [SerializeField] private ProgressionManager progressionManager;

    private List<Sticker> currentStickerChoices = new List<Sticker>();
    private List<Button> spawnedButtons = new List<Button>();

    public void StartVictorySequence()
    {
        //ShowStickerSelection();
        StartCoroutine(VictoryCoroutine());
    }

    IEnumerator VictoryCoroutine()
    {
        yield return null;
    }

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
}