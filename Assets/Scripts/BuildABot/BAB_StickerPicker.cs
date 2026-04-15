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
    [SerializeField] float rotationSpeed = 1f;

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
                    Instantiate(stickerPrefab, stickerSpawnPos, Quaternion.Euler(90, 0, cursorSticker.localEulerAngles.z), stickerParent);

                    cursorSticker.transform.SetLocalPositionAndRotation(Vector3.zero, Quaternion.identity);
                }
            }
        }

        if (stickerPicked)
        {
            Vector3 pos = new Vector3(Input.mousePosition.x, Input.mousePosition.y, Camera.main.WorldToScreenPoint(cursorSticker.position).z);
            Vector3 worldPos = Camera.main.ScreenToWorldPoint(pos);
            cursorSticker.position = new Vector3(worldPos.x, cursorSticker.position.y, worldPos.z);

            if (Input.mouseScrollDelta.y != 0)
            {
                float rotationDelta = rotationSpeed * Input.mouseScrollDelta.y;

                cursorSticker.localRotation = Quaternion.Euler(cursorSticker.localEulerAngles.x, 
                                                               cursorSticker.localEulerAngles.y, 
                                                               cursorSticker.localEulerAngles.z + rotationDelta);
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
