using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class CreditManager : MonoBehaviour
{
    public float scrollSpeed = 40f;
    public bool trackEndCredits;
    private RectTransform rectTransform;
    private float canvasHalfHeight;

    void Start()
    {
        rectTransform = GetComponent<RectTransform>();

        Canvas canvas = GetComponentInParent<Canvas>();
        canvasHalfHeight = canvas.GetComponent<RectTransform>().rect.height / 2f;
    }

    void Update()
    {
        rectTransform.anchoredPosition += new Vector2(0, scrollSpeed * Time.deltaTime);

        if (rectTransform.anchoredPosition.y > rectTransform.rect.height + canvasHalfHeight && trackEndCredits)
        {
            OnCreditsEnd();
        }
        if (Input.GetKeyDown(KeyCode.Return))
        {
            OnCreditsEnd();
            return;
        }
    }

    void OnCreditsEnd()
    {
  
      
        SceneManager.LoadScene("MainMenu");
        enabled = false; 
    }
}