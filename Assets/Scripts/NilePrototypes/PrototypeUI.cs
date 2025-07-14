using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PrototypeUI : MonoBehaviour
{
    public void SwitchScene(string sceneName)
    {
        DOTween.Clear(true);
        SceneManager.LoadScene(sceneName);
    }
}
