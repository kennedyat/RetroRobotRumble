using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;

public class BAB_SelectPart : MonoBehaviour
{
    public GameObject selectedPart;
    private Rigidbody selectedRB;

    [SerializeField, Tooltip("The height the selected part will snap to")] float _selectionHeight = 3f;
    [SerializeField, Tooltip("The time taken for the part to rotate after being selected")] float _selectionSpeed = 0.1f;

    [SerializeField, Tooltip("The factor to scale up a part by after selecting it")] float _selectionScale = 2f;


    void Update()
    {
        if (Input.GetMouseButtonDown(0)) // replace with proper input later
        {
            RaycastHit hit = CastRay();

            if (hit.collider != null)
            {
                if (hit.collider.CompareTag("BAB_Arm") || hit.collider.CompareTag("BAB_Chassis") || hit.collider.CompareTag("BAB_Legs"))
                {
                    selectedPart = hit.collider.gameObject;
                    selectedRB = selectedPart.GetComponent<Rigidbody>();
                    if (selectedRB == null)
                    {
                        selectedPart = null;
                        return;
                    }
                    selectedRB.DORotate(new Vector3(90, 0, 0), _selectionSpeed);
                    selectedRB.DOMoveY(_selectionHeight, _selectionSpeed);
                    selectedRB.isKinematic = true;
                    selectedPart.transform.DOScale(Vector3.one * _selectionScale, _selectionSpeed);
                }
            }
        }

        if (selectedPart != null)
        {
            Vector3 pos = new Vector3(Input.mousePosition.x, Input.mousePosition.y, Camera.main.WorldToScreenPoint(selectedPart.transform.position).z);
            Vector3 worldPos = Camera.main.ScreenToWorldPoint(pos);
            selectedRB.position = new Vector3(worldPos.x, selectedRB.position.y, worldPos.z);

            if (Input.GetMouseButtonUp(0)) // replace with proper input later
            {
                if (selectedRB != null)
                {
                    selectedRB.isKinematic = false;
                }
                selectedRB = null;
                selectedPart.transform.DOScale(Vector3.one, _selectionSpeed);
                selectedPart = null;
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
