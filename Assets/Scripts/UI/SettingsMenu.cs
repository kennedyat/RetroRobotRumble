using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class SettingsMenu : MonoBehaviour
{
    [SerializeField] RectTransform[] uiElements;
    [SerializeField] Image background;
    // Start is called before the first frame update
    void Start()
    {
        StartCoroutine(SettingsIntroSequence());
    }

    IEnumerator SettingsIntroSequence()
    {
        background.DOFade(0.5f, 1f).SetEase(Ease.OutExpo);
        yield return new WaitForSeconds(0.25f);

        uiElements[0].DOMoveY(500, 1f).SetEase(Ease.OutBack);
        yield return new WaitForSeconds(0.5f);

        uiElements[1].DOMoveY(425, 1f).SetEase(Ease.OutBack);
        yield return new WaitForSeconds(0.25f);

        uiElements[2].DOMoveY(400, 1f).SetEase(Ease.OutBack);
        yield return new WaitForSeconds(0.25f);

        uiElements[3].DOMoveY(100, 1f).SetEase(Ease.OutBack);
        yield return null;
    }

    public void ExitSettings()
    {
        SceneManager.UnloadSceneAsync("Settings");
    }
}
