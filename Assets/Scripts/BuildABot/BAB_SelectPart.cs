using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;

public class BAB_SelectPart : MonoBehaviour
{
    [HideInInspector] public GameObject selectedPart;
    private Rigidbody selectedRB;

    [HideInInspector] public GameObject activeSlot = null;

    [SerializeField, Tooltip("The height the selected part will snap to")] float _selectionHeight = 3f;
    [SerializeField, Tooltip("The time taken for the part to rotate after being selected")] float _selectionSpeed = 0.1f;

    [SerializeField, Tooltip("The factor to scale up a part by after selecting it")] float _selectionScale = 2f;

    [SerializeField] GameObject _resetArea;



    void Update()
    {
        if (Input.GetMouseButtonDown(0)) // replace with proper input later
        {
            if (selectedPart == null)
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
                        selectedPart.transform.DORotate(new Vector3(90, 0, Random.Range(-30, 30)), _selectionSpeed);
                        //selectedRB.DOMoveY(_selectionHeight, _selectionSpeed);
                        selectedRB.isKinematic = true;
                        selectedPart.transform.DOScale(Vector3.one * _selectionScale, _selectionSpeed);
                    }
                }
            }
            else
            {
                if (activeSlot != null)
                {
                    if (activeSlot.CompareTag(selectedPart.tag))
                    {
                        GameObject currentlyEquippedPart = activeSlot.GetComponent<BAB_EquipPart>().equippedPart;

                        if (currentlyEquippedPart != null)
                        {
                            currentlyEquippedPart.transform.SetParent(transform);
                            ResetPart(currentlyEquippedPart);
                        }

                        activeSlot.GetComponent<BAB_EquipPart>().equippedPart = selectedPart;

                        Transform selectedTransform = selectedPart.transform;

                        selectedTransform.SetParent(activeSlot.transform);
                        selectedTransform.DOLocalMove(Vector3.zero, _selectionSpeed);
                        selectedTransform.DOLocalRotate(Vector3.zero, _selectionSpeed);
                        if (activeSlot.name == "Left Arm Equip")
                        {
                            //selectedTransform.localScale = new Vector3(-selectedTransform.localScale.x, selectedTransform.localScale.y, selectedTransform.localScale.z);
                            selectedTransform.DOLocalRotate(new Vector3(0, 180, 0), _selectionSpeed * 2);
                        }
                        selectedRB = null;
                        selectedPart = null;
                    }
                    else
                    {
                        ResetPart(selectedPart);
                        selectedRB = null;
                        selectedPart = null;
                    }
                }
                else
                {
                    ResetPart(selectedPart);
                    selectedRB = null;
                    selectedPart = null;
                }
            }
        }

        if (selectedPart != null)
        {
            Vector3 pos = new Vector3(Input.mousePosition.x, Input.mousePosition.y, Camera.main.WorldToScreenPoint(selectedPart.transform.position).z);
            Vector3 worldPos = Camera.main.ScreenToWorldPoint(pos);
            selectedRB.position = new Vector3(worldPos.x, selectedRB.position.y, worldPos.z);
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

    void ResetPart(GameObject partToReset)
    {
        Rigidbody rbToReset = partToReset.GetComponent<Rigidbody>();

        if (rbToReset != null)
        {
            rbToReset.isKinematic = false;
        }

        partToReset.transform.DOScale(Vector3.one, _selectionSpeed * 2);

        Collider resetCollider = _resetArea.GetComponent<Collider>();
        Vector3 resetPosition;

        if (resetCollider != null)
        {
            resetPosition = resetCollider.ClosestPoint(partToReset.transform.position);
        }
        else
        {
            resetPosition = _resetArea.transform.position;
        }

        rbToReset.DOMoveX(resetPosition.x, _selectionSpeed * 5);
        rbToReset.DOMoveZ(resetPosition.z, _selectionSpeed * 5);
    }
}
