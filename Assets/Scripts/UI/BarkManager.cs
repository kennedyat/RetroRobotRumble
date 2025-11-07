using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
public class BarkManager : MonoBehaviour
{
    //private VerticalLayoutGroup layoutGroup;

    [SerializeField] GameObject[] barkPrefabs;

    [SerializeField] Sprite[] fleckBarkSprites;
    [SerializeField] Sprite[] enemyBarkSprites;
    [SerializeField] Sprite[] announcerBarkSprites;

    protected void Start()
    {
        //layoutGroup = GetComponent<VerticalLayoutGroup>();
    }

    protected void Update()
    {
        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            SpawnBark("fleck", 0, "Alright!");
        }
        if (Input.GetKeyDown(KeyCode.Alpha2))
        {
            SpawnBark("fleck", 1, "YEAHHHHHH!!!");
        }
        if (Input.GetKeyDown(KeyCode.Alpha3))
        {
            SpawnBark("fleck", 2, "Oh no....");
        }
        if (Input.GetKeyDown(KeyCode.Alpha4))
        {
            SpawnBark("fleck", 3, "WAAAAAAAAAAAAAAAAAAAAAAAAGGGGGGGGGHHHHHHH");
        }
        if (Input.GetKeyDown(KeyCode.Alpha5))
        {
            SpawnBark("enemy", 0, "Let's go!");
        }
        if (Input.GetKeyDown(KeyCode.Alpha6))
        {
            SpawnBark("enemy", 1, "I WIN!!!!");
        }
        if (Input.GetKeyDown(KeyCode.Alpha7))
        {
            SpawnBark("enemy", 2, "Huh...?!");
        }
        if (Input.GetKeyDown(KeyCode.Alpha8))
        {
            SpawnBark("enemy", 3, "NOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOO");
        }
        if (Input.GetKeyDown(KeyCode.Alpha9))
        {
            SpawnBark("announcer", 0, "Amazing!");
        }
        if (Input.GetKeyDown(KeyCode.Alpha0))
        {
            SpawnBark("announcer", 1, "???????????????");
        }
    }
    
    private void SpawnBark(string character, int expression, string dialogue)
    {
        GameObject spawnedBark;

        switch (character)
        {
            case "fleck":
                spawnedBark = Instantiate(barkPrefabs[0], this.transform);
                spawnedBark.GetComponent<Image>().sprite = fleckBarkSprites[expression];
                spawnedBark.transform.GetChild(0).GetComponent<TextMeshProUGUI>().text = dialogue;
                break;

            case "enemy":
                spawnedBark = Instantiate(barkPrefabs[1], this.transform);
                spawnedBark.GetComponent<Image>().sprite = enemyBarkSprites[expression];
                spawnedBark.transform.GetChild(0).GetComponent<TextMeshProUGUI>().text = dialogue;
                break;

            case "announcer":
                spawnedBark = Instantiate(barkPrefabs[1], this.transform);
                spawnedBark.GetComponent<Image>().sprite = announcerBarkSprites[expression];
                spawnedBark.transform.GetChild(0).GetComponent<TextMeshProUGUI>().text = dialogue;
                break;

            default:
                spawnedBark = null;
                break;
        }
        
        // if (spawnedBark != null)
        // {
        //     spawnedBark.GetComponent<RectTransform>().anchoredPosition = Vector2.zero;
        // }
    }
}
