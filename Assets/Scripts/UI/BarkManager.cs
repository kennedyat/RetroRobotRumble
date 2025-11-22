using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
public class BarkManager : MonoBehaviour
{
    //private VerticalLayoutGroup layoutGroup;

    [System.Serializable]
    public class DialogueEntry
    {
        public string key;
        public List<Sprite> barkExpressions;
        public GameObject barkLayout;
        public List<string> lines;
    }

    [SerializeField]
    List<DialogueEntry> dialogueEntries;

    private Dictionary<string, DialogueEntry> dialogueList;


    [SerializeField] GameObject[] barkPrefabs;

    [SerializeField] Sprite[] fleckBarkSprites;
    [SerializeField] Sprite[] enemyBarkSprites;
    [SerializeField] Sprite[] announcerBarkSprites;

    [SerializeField] float barkSpacing = 500f;

    private int index;

    private bool canBark = true;

    public static BarkManager Instance { get; private set; }

    protected void Start()
    {
         if (Instance != null && Instance != this)
        {
            Destroy(this);
            return;
        }

        Instance = this;
        dialogueList = new Dictionary<string, DialogueEntry>();
        //layoutGroup = GetComponent<VerticalLayoutGroup>();
        foreach (DialogueEntry entry in dialogueEntries)
        {
            dialogueList[entry.key] = entry;
        }
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
            SpawnBark("announcer", 1, "?!?!?!?!?!?!?!?!?!?!?!?!?!?!");
        }
    }

    private void SpawnBark(string character, int expression, string dialogue)
    {
        if (!canBark)
        {
            return;
        }

        StartCoroutine(BarkCooldown(1f));
        foreach (Transform bark in transform)
        {
            RectTransform barkTransform = bark.GetComponent<RectTransform>();

            barkTransform.DOAnchorPosY(barkTransform.anchoredPosition.y - barkSpacing, 0.5f, true).SetEase(Ease.OutExpo);
        }

        GameObject spawnedBark;
        spawnedBark = Instantiate(barkPrefabs[0], this.transform);
        spawnedBark.GetComponent<Image>().sprite = fleckBarkSprites[expression];
        spawnedBark.transform.GetChild(0).GetComponent<TextMeshProUGUI>().text = dialogue;


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
    }

    public void StartBark(params string[] entryNames)
    {
        if (entryNames.Length == 0) return;

        SpawnBarkRevised(entryNames[index]);

        index = (index + 1) % entryNames.Length;
    }
    public void SpawnBarkRevised(string entryName)
    {
        if (!canBark)
        {
            return;
        }

        StartCoroutine(BarkCooldown(1f));
        foreach (Transform bark in transform)
        {
            RectTransform barkTransform = bark.GetComponent<RectTransform>();

            barkTransform.DOAnchorPosY(barkTransform.anchoredPosition.y - barkSpacing, 0.5f, true).SetEase(Ease.OutExpo);
        }

        GameObject spawnedBark;
        var entry = dialogueList[entryName];
        var randDialogue = entry.lines[UnityEngine.Random.Range(0, entry.lines.Count)];


        spawnedBark = Instantiate(entry.barkLayout, this.transform);
        spawnedBark.GetComponent<Image>().sprite = entry.barkExpressions[UnityEngine.Random.Range(0, entry.barkExpressions.Count)];
        spawnedBark.transform.GetChild(0).GetComponent<TextMeshProUGUI>().text = randDialogue;
    }

        IEnumerator BarkCooldown(float duration)
        {
            canBark = false;
            yield return new WaitForSeconds(duration);
            canBark = true;
        }
    
}
