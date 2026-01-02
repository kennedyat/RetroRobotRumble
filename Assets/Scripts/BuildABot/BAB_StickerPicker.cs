using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BAB_StickerPicker : MonoBehaviour
{
    private bool stickerPicked = false;
    [SerializeField] Transform cursorSticker;
    [SerializeField] Transform handCollider;
    [SerializeField] Transform stickerParent;
    [SerializeField] GameObject stickerPrefab;

    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            RaycastHit hit = CastRay();

            if (hit.collider != null)
            {
                Debug.Log(hit.collider.gameObject.name);

                if (hit.collider.gameObject == this.gameObject)
                {
                    stickerPicked = true;
                }

                if (hit.collider.gameObject.transform.parent == handCollider)
                {
                    stickerPicked = false;

                    Vector3 stickerSpawnPos = new Vector3(cursorSticker.position.x, 15, cursorSticker.position.z);
                    Instantiate(stickerPrefab, stickerSpawnPos, Quaternion.Euler(90, 0, 0), stickerParent);

                    cursorSticker.transform.localPosition = Vector3.zero;
                }
            }
        }

        if (stickerPicked)
        {
            Vector3 pos = new Vector3(Input.mousePosition.x, Input.mousePosition.y, Camera.main.WorldToScreenPoint(cursorSticker.position).z);
            Vector3 worldPos = Camera.main.ScreenToWorldPoint(pos);
            cursorSticker.position = new Vector3(worldPos.x, cursorSticker.position.y, worldPos.z);
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
