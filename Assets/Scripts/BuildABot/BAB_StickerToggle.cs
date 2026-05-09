using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class BAB_StickerToggle : MonoBehaviour
{
    private bool stickerLoaded = false;
    [SerializeField] string sceneName = "Sticker_Prototype";
    [SerializeField] BAB_SelectPart parts;

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Tab) && parts.selectedPart == null)
        {
            if (stickerLoaded)
            {
                SceneManager.UnloadSceneAsync(sceneName);
            } else
            {
                SceneManager.LoadScene(sceneName, LoadSceneMode.Additive);   
            }
            stickerLoaded = !stickerLoaded;
        }
    }
}
