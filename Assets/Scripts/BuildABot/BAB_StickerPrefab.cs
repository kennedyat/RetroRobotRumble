using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using UnityEngine.Rendering.Universal;
using UnityEngine.UI;

public class BAB_StickerPrefab : MonoBehaviour
{
    [SerializeField] private Image background;
    [SerializeField] private Image sprite;
    [SerializeField] private GameObject decalPrefab;

    private Transform cursorSticker;
    private bool selected = false;

    void Start()
    {
        cursorSticker = sprite.transform;
    }

    void Update()
    {
        if (selected)
        {
            Vector3 pos = new Vector3(Input.mousePosition.x, Input.mousePosition.y, Camera.main.WorldToScreenPoint(cursorSticker.position).z);
            Vector3 worldPos = Camera.main.ScreenToWorldPoint(pos);
            cursorSticker.position = new Vector3(worldPos.x, cursorSticker.position.y, worldPos.z);

            if (Input.mouseScrollDelta.y != 0)
            {
                float rotationDelta = 5 * Input.mouseScrollDelta.y;

                cursorSticker.localRotation = Quaternion.Euler(cursorSticker.localEulerAngles.x, 
                                                               cursorSticker.localEulerAngles.y, 
                                                               cursorSticker.localEulerAngles.z + rotationDelta);
            }
        }
    }


    public void SelectSticker()
    {
        if (!selected)
        {
            selected = true;
        } else
        {
            selected = false;

            RaycastHit hit = CastRay();

            if (hit.collider != null)
            {
                if (hit.collider.gameObject.name == "HandCollider")
                {
                    Debug.Log("clicked on hand collider yay");
                    // instantiate decal
                    GameObject decal = Instantiate(decalPrefab, transform);
                    Material decalMat = decal.GetComponent<DecalProjector>().material;                    
                    // either hide or destroy the prefab
                } else
                {
                    Debug.Log("cannot place sticker here");
                    cursorSticker.DOMove(transform.position, 0.5f).SetEase(Ease.OutCirc);
                    cursorSticker.DOLocalRotate(transform.localEulerAngles, 0.5f).SetEase(Ease.OutCirc);
                }
            }
        }
    }

    public void UpdateSprite(Sprite newSprite)
    {
        background.sprite = newSprite;
        sprite.sprite = newSprite;
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
