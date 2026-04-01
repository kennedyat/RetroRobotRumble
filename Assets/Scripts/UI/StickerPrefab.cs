using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

public class StickerPrefab : MonoBehaviour
{
    public bool active = false;
    public bool applied = false;

    private Collider collider;
    private Image sprite;

    public int stickerIndex;

    public Vector3 stickerPosition;
    public Vector3 stickerRotation;

    void Start()
    {
        collider = GetComponent<Collider>();
        sprite = GetComponent<Image>();
    }

    // Update is called once per frame
    void Update()
    {
        if (active)
        {
            updatePosition();
            updateRotation();
        } else
        {
            stickerPosition = transform.localPosition;
            stickerRotation = transform.localEulerAngles;
        }
        collider.enabled = !active;
        sprite.enabled = !applied;
    }

    void updatePosition()
    {
        Vector3 pos = new Vector3(Input.mousePosition.x, Input.mousePosition.y, Camera.main.WorldToScreenPoint(transform.position).z);
        Vector3 worldPos = Camera.main.ScreenToWorldPoint(pos);
        transform.position = new Vector3(worldPos.x, worldPos.y, transform.position.z);
        
    }

    void updateRotation()
    {
        if (Input.mouseScrollDelta.y != 0)
        {
            float rotationDelta = Input.mouseScrollDelta.y * 5f;

            transform.localRotation = Quaternion.Euler(transform.localEulerAngles.x, 
                                                        transform.localEulerAngles.y, 
                                                        transform.localEulerAngles.z + rotationDelta);

            // transform.DOLocalRotate(new Vector3(transform.localEulerAngles.x, 
            //                                     transform.localEulerAngles.y, 
            //                                     transform.localEulerAngles.z + rotationDelta), 0.1f);
        }
    }
}
