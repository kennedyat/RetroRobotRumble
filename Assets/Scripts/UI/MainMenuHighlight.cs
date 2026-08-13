using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using TMPro;
using UnityEngine;

public class MainMenuHighlight : MonoBehaviour
{
    private bool highlighted = false;
    [SerializeField] private TextMeshProUGUI text;
    protected void Update()
    {
        RaycastHit hit = CastRay();

        if (hit.collider != null)
        {
            if (hit.collider.gameObject == gameObject)
            {
                if (!highlighted)
                {
                    highlighted = true;
                    text.DOColor(Color.white, 0.1f).SetEase(Ease.OutQuint);
                    DOTween.To(() => text.fontSize, x => text.fontSize = x, 40, 0.1f).SetEase(Ease.OutQuint);
                    Debug.Log("highlighting " + gameObject.name);
                }
            } else
            {
                if (highlighted)
                {
                    highlighted = false;
                    text.DOColor(new Color(0.9333333f, 1f, 0.254902f), 0.1f).SetEase(Ease.OutQuint);
                    DOTween.To(() => text.fontSize, x => text.fontSize = x, 36, 0.1f).SetEase(Ease.OutQuint);
                    Debug.Log("not highlighting " + gameObject.name);
                }
            }
        }
    }

    private RaycastHit CastRay()
    {
        Vector3 screenMousePosFar = new Vector3(Input.mousePosition.x, Input.mousePosition.y, Camera.main.farClipPlane);

        Vector3 screenMousePosNear = new Vector3(Input.mousePosition.x, Input.mousePosition.y, Camera.main.nearClipPlane);

        Vector3 worldMousePosFar = Camera.main.ScreenToWorldPoint(screenMousePosFar);
        Vector3 worldMousePosNear = Camera.main.ScreenToWorldPoint(screenMousePosNear);
        RaycastHit hit;
        Physics.Raycast(worldMousePosNear, worldMousePosFar - worldMousePosNear, out hit);

        return hit;
    }
}
