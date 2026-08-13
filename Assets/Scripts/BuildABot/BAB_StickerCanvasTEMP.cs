using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BAB_StickerCanvasTEMP : MonoBehaviour
{
    // Start is called before the first frame update 
    [SerializeField] private GameObject stickerPrefab; 
    [SerializeField] private Transform parent; // Layout group parent
    
    void Start()
    {
        AddButtons();
    }

    public void AddButtons()
    {
        foreach(Sticker sticker in RunData.availableStickers)
        {
            GameObject stickerObj = Instantiate(stickerPrefab, parent);
            
            stickerObj.GetComponent<BAB_StickerPrefab>().UpdateSprite(sticker.stickerSprite);
        }
    }    
}
