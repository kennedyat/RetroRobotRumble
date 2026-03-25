using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering.Universal;
using UnityEngine.UI;

public class StickerApplication : MonoBehaviour
{
    private GameObject activeSticker = null;
    //private bool stickerPicked = false;
    [SerializeField] Transform handColliders;
    [SerializeField] Transform stickerParent;
    [SerializeField] GameObject stickerPrefab;
    [SerializeField] Collider stickerSpawnArea;
    [SerializeField] List<Sticker> unlockedStickers;
    [SerializeField] GameObject doneButton;

    void Start()
    {
        ApplyExistingStickers();
        SpawnStickers();
    }
    
    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            RaycastHit hit = CastRay();
            
            if (hit.collider != null)
            {
                Debug.Log("clicked on " + hit.collider.gameObject.name);

                if (activeSticker == null)
                {
                    if (hit.collider.gameObject.CompareTag("Sticker"))
                    {
                        activeSticker = hit.collider.gameObject;
                        activeSticker.GetComponent<StickerPrefab>().active = true;
                        activeSticker.GetComponent<StickerPrefab>().applied = false;
                        activeSticker.transform.SetParent(stickerSpawnArea.transform, true);
                    }
                } else
                {
                    if (hit.collider.gameObject.transform.parent == handColliders)
                    {
                        activeSticker.GetComponent<StickerPrefab>().applied = true;
                        activeSticker.transform.SetParent(stickerParent, true);
                        Debug.Log("sticker position: " + activeSticker.transform.localPosition);
                        Debug.Log("sticker rotation: " + activeSticker.transform.localRotation.eulerAngles);
                    } else
                    {
                        activeSticker.GetComponent<StickerPrefab>().applied = false;
                    }
                    activeSticker.GetComponent<StickerPrefab>().active = false;
                    activeSticker = null;
                }
            }
        }

        // lmao    
        if (Input.GetKey(KeyCode.Space)) 
        {            
            SpawnStickers();
        }
        
        doneButton.SetActive(stickerParent.childCount == unlockedStickers.Count);
    }

    void ApplyExistingStickers()
    {
        //
    }

    void SpawnStickers()
    {
        foreach (Sticker sticker in unlockedStickers)
        {
            GameObject instantiatedSticker = Instantiate(stickerPrefab, 
                                                        GenerateSpawnPosition(stickerSpawnArea.bounds), 
                                                        GenerateSpawnRotation(), 
                                                        stickerSpawnArea.transform);

            instantiatedSticker.GetComponent<Image>().sprite = sticker.stickerSprite;
            instantiatedSticker.GetComponent<DecalProjector>().material = sticker.decalMaterial;
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

    private Vector3 GenerateSpawnPosition(Bounds bounds)
    {
        return new Vector3(Random.Range(bounds.min.x, bounds.max.x),
                           Random.Range(bounds.min.y, bounds.max.y),
                           stickerParent.transform.position.z);
    }

    private Quaternion GenerateSpawnRotation() {
        return Quaternion.Euler(0, 0, Random.Range(-60, 60));
    }
}
