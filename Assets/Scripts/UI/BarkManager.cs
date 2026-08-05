using System.Collections;
using System.Collections.Generic;
using System.Linq;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public enum BarkSpeaker
{
    Fleck,
    Announcer,
    Bentley,
    Carly,
    Maxine,
    Milo,
    Rufus,
    Seito,
    Stacy,
    Fairwell,
    Sophie,
    Enemy
}

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

    [System.Serializable]
    public class SpeakerBarkPrefab
    {
        public BarkSpeaker speaker;
        public GameObject barkLayout;
        public List<Sprite> barkExpressions;
    }

    [System.Serializable]
    public class BarkLine
    {
        public string lineNumber;
        public string line;
        public BarkSpeaker speaker;
        public string audioResourcePath;
        public string triggerEvent;
        public string source;
        public string section;
        public string scene;
        public bool tutorial;
        public bool happyExpression;
    }

    [System.Serializable]
    public class BarkDatabase
    {
        public List<BarkLine> lines = new List<BarkLine>();
    }

    [System.Serializable]
    public class BarkEventRule
    {
        public string triggerEvent;
        public int priority = 0;
        [Range(0f, 1f)] public float silenceChance = 0.35f;
        public bool canPlayDuringCooldown;
    }

    private class BarkRequest
    {
        public BarkSpeaker? speaker;
        public string triggerEvent;
        public string source;
        public bool tutorial;
        public string tutorialSection;
        public string tutorialScene;
        public string lineNumber;
        public float requestTime;
        public bool skipSilenceRoll;
        public bool guaranteed;
        public bool audioOnly;
    }

    [SerializeField]
    List<DialogueEntry> dialogueEntries = new List<DialogueEntry>();

    [SerializeField] TextAsset barkDatabaseJson;
    [SerializeField] List<SpeakerBarkPrefab> speakerBarkPrefabs = new List<SpeakerBarkPrefab>();
    [SerializeField] bool playBarkAudio = true;
    [SerializeField, Range(0f, 5f)] float defaultBarkVolume = 4f;
    [SerializeField] bool duckMusicDuringBarks = true;
    [SerializeField, Range(0f, 1f)] float defaultSilenceChance = 0.35f;
    [SerializeField] float fallbackBarkLockDuration = 2.5f;
    // Extra tiny gap after a bark ends so the next one doesn't feel like it is stepping on it.
    [SerializeField] float postBarkCooldown = 0.25f;
    /* These are the main numbers to tune in the editor.
       silenceChance is the percent chance NOTHING plays, so lower = more barks.
       canPlayDuringCooldown lets important stuff ignore the tiny post-bark gap, but not overlap active audio. */
    [SerializeField] List<BarkEventRule> eventRules = new List<BarkEventRule>();

    private Dictionary<string, DialogueEntry> dialogueList;
    private Dictionary<BarkSpeaker, SpeakerBarkPrefab> speakerPrefabLookup;
    private Dictionary<string, BarkEventRule> eventRuleLookup;
    private BarkDatabase barkDatabase = new BarkDatabase();
    private AudioSource audioSource;
    private static GameObject sharedBarkAudioObject;
    private static AudioSource sharedBarkAudioSource;
    private static AudioListener sharedBarkAudioListener;
    public const string BarkVolumePrefsKey = "BarkVolume_v4";
    private float barkVolume = 1f;
    private string recentPlayerBarkSource = "";
    private float recentPlayerBarkSourceTime = -999f;
    private const float RecentPlayerBarkSourceWindow = 20f;
    private float cooldownUntil;
    private float currentBarkDuration;
    private readonly Queue<string> recentLineNumbers = new Queue<string>();
    private const int RecentLineHistoryLimit = 8;
    private string lastGuaranteedKey = "";
    private float lastGuaranteedTime = -999f;
    private const float GuaranteedDuplicateWindow = 6f;


    [SerializeField] GameObject[] barkPrefabs;

    [SerializeField] Sprite[] fleckBarkSprites;
    [SerializeField] Sprite[] enemyBarkSprites;
    [SerializeField] Sprite[] announcerBarkSprites;

    [SerializeField] float barkSpacing = 500f;

    private int index;

    private bool canBark = true;

    public static BarkManager Instance { get; private set; }

    protected void Awake()
    {
         if (Instance != null && Instance != this)
        {
            Destroy(this);
            return;
        }

        Instance = this;
        dialogueList = new Dictionary<string, DialogueEntry>();
        speakerPrefabLookup = new Dictionary<BarkSpeaker, SpeakerBarkPrefab>();
        eventRuleLookup = new Dictionary<string, BarkEventRule>();
        //layoutGroup = GetComponent<VerticalLayoutGroup>();
        foreach (DialogueEntry entry in dialogueEntries)
        {
            if (entry == null || string.IsNullOrWhiteSpace(entry.key))
                continue;

            dialogueList[entry.key] = entry;
        }

        foreach (SpeakerBarkPrefab speakerPrefab in speakerBarkPrefabs)
        {
            if (speakerPrefab == null || speakerPrefab.barkLayout == null)
                continue;

            speakerPrefabLookup[speakerPrefab.speaker] = speakerPrefab;
        }

        if (barkDatabaseJson == null)
        {
            barkDatabaseJson = Resources.Load<TextAsset>("Barks/BarkDatabase");
        }

        if (barkDatabaseJson != null)
        {
            barkDatabase = JsonUtility.FromJson<BarkDatabase>(barkDatabaseJson.text) ?? new BarkDatabase();
        }

        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }

        audioSource.playOnAwake = false;
        audioSource.spatialBlend = 0f;
        EnsureSharedBarkAudioSource();
        SetBarkVolume(PlayerPrefs.GetFloat(BarkVolumePrefsKey, defaultBarkVolume));

        EnsureAudioListenerExists();
        InitializeEventRules();
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

    private void OnDestroy()
    {
        if (Instance == this)
        {
            Instance = null;
        }
    }

    private void SpawnBark(string character, int expression, string dialogue)
    {
        if (!canBark)
        {
            return;
        }

        foreach (Transform bark in transform)
        {
            RectTransform barkTransform = bark.GetComponent<RectTransform>();

            barkTransform.DOAnchorPosY(barkTransform.anchoredPosition.y - barkSpacing, 0.5f, true).SetEase(Ease.OutExpo);
        }

        switch (character)
        {
            case "fleck":
                GameObject fleckBark = SpawnLegacyBark(barkPrefabs[0], fleckBarkSprites, expression, dialogue);
                StartBarkLock(fleckBark);
                break;

            case "enemy":
                GameObject enemyBark = SpawnLegacyBark(barkPrefabs[1], enemyBarkSprites, expression, dialogue);
                StartBarkLock(enemyBark);
                break;

            case "announcer":
                GameObject announcerBark = SpawnLegacyBark(barkPrefabs[1], announcerBarkSprites, expression, dialogue);
                StartBarkLock(announcerBark);
                break;
        }
    }

    public void StartBark(params string[] entryNames)
    {
        if (entryNames.Length == 0 || this == null) return;

        string entryName = entryNames[index];
        if (dialogueList.ContainsKey(entryName))
        {
            SpawnBarkRevised(entryName);
        }
        else
        {
            PlayBark(entryName);
        }

        index = (index + 1) % entryNames.Length;
    }

    public void StartBark(BarkSpeaker speaker, params string[] entryNames)
    {
        if (entryNames.Length == 0 || this == null) return;

        string entryName = entryNames[index];
        if (dialogueList.ContainsKey(entryName))
        {
            SpawnBarkRevised(speaker, entryName);
        }
        else
        {
            PlayBark(speaker, entryName);
        }

        index = (index + 1) % entryNames.Length;
    }

    public void PlayBark(string triggerEvent, string source = "")
    {
        if (string.IsNullOrWhiteSpace(triggerEvent))
        {
            return;
        }

        TryPlayBarkRequest(null, triggerEvent, source, false);
    }

    public void PlayGuaranteedBark(string triggerEvent, string source = "")
    {
        if (string.IsNullOrWhiteSpace(triggerEvent))
        {
            return;
        }

        TryPlayBarkRequest(null, triggerEvent, source, false, true);
    }

    public void PlayPriorityBark(string triggerEvent, string source = "")
    {
        if (string.IsNullOrWhiteSpace(triggerEvent))
        {
            return;
        }

        TryPlayBarkRequest(null, triggerEvent, source, false, true);
    }

    public void PlayPriorityBark(BarkSpeaker speaker, string triggerEvent, string source = "")
    {
        if (string.IsNullOrWhiteSpace(triggerEvent))
        {
            return;
        }

        TryPlayBarkRequest(speaker, triggerEvent, source, false, true);
    }

    public void PlayGuaranteedBark(BarkSpeaker speaker, string triggerEvent, string source = "")
    {
        if (string.IsNullOrWhiteSpace(triggerEvent))
        {
            return;
        }

        TryPlayBarkRequest(speaker, triggerEvent, source, false, true);
    }

    public void PlayRoundStartAnnouncerBark(string source)
    {
        TryPlayBarkRequest(null, "Round Start", source, false, true);
    }

    public void PlayRoundStartAnnouncerBarkAudioOnly(string source)
    {
        TryPlayBarkRequest(null, "Round Start", source, false, true, false, true);
    }

    public void PlayBarkForPart(string triggerEvent, string partName, string fallbackSource)
    {
        PlayBarkForPart(triggerEvent, partName, fallbackSource, "");
    }

    public void PlayBarkForPart(string triggerEvent, string partName, string fallbackSource, string partIdentity)
    {
        string source = ResolvePartSource(partName, fallbackSource);
        if (!string.IsNullOrWhiteSpace(partIdentity))
        {
            source = ResolvePartSource(partIdentity, source);
        }

        RememberPlayerBarkSource(source);

        TryPlayBarkRequest(null, triggerEvent, source, false);
    }

    public void PlayBark(BarkSpeaker speaker, string triggerEvent, string source = "")
    {
        if (string.IsNullOrWhiteSpace(triggerEvent))
        {
            return;
        }

        TryPlayBarkRequest(speaker, triggerEvent, source, false);
    }

    public void PlayBarkForSpeaker(string speaker, string triggerEvent, string source = "")
    {
        if (TryParseSpeaker(speaker, out BarkSpeaker parsedSpeaker))
        {
            PlayBark(parsedSpeaker, triggerEvent, source);
            return;
        }

        PlayBark(triggerEvent, source);
    }

    public void PlayTutorialBark(string section, string scene = "", BarkSpeaker? speaker = null)
    {
        TryPlayTutorialBark(section, scene, speaker);
    }

    public void PlayLineNumber(string lineNumber)
    {
        TryPlayLineNumber(lineNumber);
    }

    public void SpawnBarkRevised(string entryName)
    {
        SpawnBarkRevised(null, entryName);
    }

    public void SpawnBarkRevised(BarkSpeaker? speaker, string entryName)
    {
        if (!canBark)
        {
            return;
        }

        foreach (Transform bark in transform)
        {
            RectTransform barkTransform = bark.GetComponent<RectTransform>();

            barkTransform.DOAnchorPosY(barkTransform.anchoredPosition.y - barkSpacing, 0.5f, true).SetEase(Ease.OutExpo);
        }

        GameObject spawnedBark;
        var entry = dialogueList[entryName];
        var randDialogue = entry.lines[UnityEngine.Random.Range(0, entry.lines.Count)];
        SpeakerBarkPrefab speakerPrefab = speaker.HasValue ? GetSpeakerPrefab(speaker.Value) : null;
        GameObject layout = speakerPrefab != null ? speakerPrefab.barkLayout : entry.barkLayout;
        List<Sprite> expressions = speakerPrefab != null && speakerPrefab.barkExpressions.Count > 0
            ? speakerPrefab.barkExpressions
            : entry.barkExpressions;


        if (layout == null)
        {
            return;
        }

        spawnedBark = Instantiate(layout, this.transform);
        SetBarkVisuals(spawnedBark, expressions, randDialogue);
        StartBarkLock(spawnedBark);
    }

    private void SpawnBarkLine(BarkLine line)
    {
        SpawnBarkLine(line, false);
    }

    private void SpawnBarkLine(BarkLine line, bool audioOnly)
    {
        if (line == null || !canBark)
        {
            return;
        }

        if (audioOnly)
        {
            float audioOnlyDuration = PlayAudio(line);
            StartBarkLock(null, audioOnlyDuration);
            return;
        }

        MoveExistingBarks();

        SpeakerBarkPrefab speakerPrefab = GetSpeakerPrefab(line.speaker);
        GameObject layout = speakerPrefab != null ? speakerPrefab.barkLayout : GetFallbackLayout(line.speaker);
        List<Sprite> expressions = speakerPrefab != null ? speakerPrefab.barkExpressions : null;

        if (layout == null)
        {
            return;
        }

        GameObject spawnedBark = Instantiate(layout, this.transform);
        SetBarkVisuals(spawnedBark, expressions, line.line);
        float audioDuration = PlayAudio(line);
        MatchBarkLifetimeToAudio(spawnedBark, audioDuration);
        StartBarkLock(spawnedBark, audioDuration);
    }

    private BarkLine PickLine(BarkSpeaker? speaker, string triggerEvent, string source, bool tutorial)
    {
        /* The bark picker works like:
           1. match the trigger
           2. optionally lock to one speaker
           3. filter to sources that make sense for this exact context
           4. pick randomly from that final pool */
        IEnumerable<BarkLine> candidates = barkDatabase.lines.Where(line =>
            line.tutorial == tutorial &&
            TriggerMatches(line.triggerEvent, triggerEvent));

        if (speaker.HasValue)
        {
            candidates = candidates.Where(line => line.speaker == speaker.Value);
        }

        HashSet<string> validSources = GetValidSources(triggerEvent, source);
        List<BarkLine> matches = candidates
            .Where(line => validSources.Count == 0 || validSources.Contains(line.source))
            .ToList();

        if (matches.Count == 0)
        {
            return null;
        }

        List<BarkLine> freshMatches = matches
            .Where(line => !recentLineNumbers.Contains(line.lineNumber))
            .ToList();

        if (freshMatches.Count > 0)
        {
            matches = freshMatches;
        }

        return matches[UnityEngine.Random.Range(0, matches.Count)];
    }

    private void TryPlayBarkRequest(BarkSpeaker? speaker, string triggerEvent, string source, bool tutorial, bool guaranteed = false, bool ignoreDuplicateWindow = false, bool audioOnly = false)
    {
        if (guaranteed && !ignoreDuplicateWindow && IsDuplicateGuaranteedRequest(triggerEvent, source))
        {
            return;
        }

        BarkRequest request = new BarkRequest
        {
            speaker = speaker,
            triggerEvent = triggerEvent,
            source = source,
            tutorial = tutorial,
            requestTime = Time.time,
            skipSilenceRoll = guaranteed,
            guaranteed = guaranteed,
            audioOnly = audioOnly
        };

        TryResolveAndPlay(request);
    }

    private bool IsDuplicateGuaranteedRequest(string triggerEvent, string source)
    {
        string key = $"{triggerEvent}|{source}";
        if (key == lastGuaranteedKey && Time.time - lastGuaranteedTime <= GuaranteedDuplicateWindow)
        {
            return true;
        }

        lastGuaranteedKey = key;
        lastGuaranteedTime = Time.time;
        return false;
    }

    private void TryPlayTutorialBark(string section, string scene, BarkSpeaker? speaker)
    {
        BarkRequest request = new BarkRequest
        {
            speaker = speaker,
            tutorial = true,
            tutorialSection = section,
            tutorialScene = scene,
            requestTime = Time.time
        };

        TryResolveAndPlay(request);
    }

    private void TryPlayLineNumber(string lineNumber)
    {
        BarkRequest request = new BarkRequest
        {
            lineNumber = lineNumber,
            requestTime = Time.time
        };

        TryResolveAndPlay(request);
    }

    private void TryResolveAndPlay(BarkRequest request)
    {
        BarkEventRule rule = GetRequestRule(request);

        // Round start/end lines should feel instant, so they can clear whatever was already talking.
        if (request.guaranteed && IsPriorityBark(request.triggerEvent))
        {
            StopActiveBarkPlayback();
        }

        // If somebody is already talking, we drop the new bark instead of playing it late.
        if (!canBark)
        {
            return;
        }

        if (!CanAttemptRequest(rule, request.skipSilenceRoll, request.guaranteed))
        {
            return;
        }

        BarkLine line = ResolveRequestLine(request);
        if (line != null)
        {
            RememberLine(line);
            SpawnBarkLine(line, request.audioOnly);
        }
        else
        {
            Debug.LogWarning($"No bark line found for trigger '{request.triggerEvent}' and source '{request.source}'.", this);
        }
    }

    private BarkLine ResolveRequestLine(BarkRequest request)
    {
        if (!string.IsNullOrWhiteSpace(request.lineNumber))
        {
            return barkDatabase.lines.FirstOrDefault(candidate => candidate.lineNumber == request.lineNumber);
        }

        if (request.tutorial)
        {
            return PickTutorialLine(request.tutorialSection, request.tutorialScene, request.speaker);
        }

        return PickLine(request.speaker, request.triggerEvent, request.source, false);
    }

    private bool TriggerMatches(string lineTrigger, string requestedTrigger)
    {
        if (string.Equals(lineTrigger, requestedTrigger, System.StringComparison.OrdinalIgnoreCase))
        {
            return true;
        }

        // Specific enemy defeat barks can use generic defeat lines too, but not other specific variants.
        if (requestedTrigger.StartsWith("Enemy Defeated", System.StringComparison.OrdinalIgnoreCase))
        {
            return string.Equals(lineTrigger, "Enemy Defeated", System.StringComparison.OrdinalIgnoreCase) ||
                   string.Equals(lineTrigger, requestedTrigger, System.StringComparison.OrdinalIgnoreCase);
        }

        // Same idea for player damage: hazard/projectile can use generic damage, but not each other.
        if (requestedTrigger.StartsWith("Player Take Damage", System.StringComparison.OrdinalIgnoreCase))
        {
            return string.Equals(lineTrigger, "Player Take Damage", System.StringComparison.OrdinalIgnoreCase) ||
                   string.Equals(lineTrigger, requestedTrigger, System.StringComparison.OrdinalIgnoreCase);
        }

        if (requestedTrigger == "Player Basic Attack")
        {
            return lineTrigger.StartsWith("Player Basic Attack", System.StringComparison.OrdinalIgnoreCase);
        }

        return false;
    }

    private bool CanAttemptRequest(BarkEventRule rule, bool skipSilenceRoll, bool guaranteed)
    {
        if (rule == null)
        {
            return false;
        }

        bool isCoolingDown = Time.time < cooldownUntil;
        if (isCoolingDown && !rule.canPlayDuringCooldown && !guaranteed)
        {
            return false;
        }

        return skipSilenceRoll || UnityEngine.Random.value >= rule.silenceChance;
    }

    private BarkEventRule GetRequestRule(BarkRequest request)
    {
        if (request == null)
        {
            return GetEventRule("");
        }

        if (!string.IsNullOrWhiteSpace(request.lineNumber) || request.tutorial)
        {
            return GetEventRule("Tutorial");
        }

        return GetEventRule(request.triggerEvent);
    }

    private void InitializeEventRules()
    {
        if (eventRules == null)
        {
            eventRules = new List<BarkEventRule>();
        }

        // These defaults only get used if the prefab has no rules serialized yet.
        if (eventRules.Count == 0)
        {
            eventRules.Add(new BarkEventRule { triggerEvent = "Game Over", priority = 100, silenceChance = 0f, canPlayDuringCooldown = true });
            eventRules.Add(new BarkEventRule { triggerEvent = "Victory", priority = 100, silenceChance = 0f, canPlayDuringCooldown = true });
            eventRules.Add(new BarkEventRule { triggerEvent = "Tutorial", priority = 100, silenceChance = 0f, canPlayDuringCooldown = true });
            eventRules.Add(new BarkEventRule { triggerEvent = "Round Start", priority = 100, silenceChance = 0f, canPlayDuringCooldown = true });
            eventRules.Add(new BarkEventRule { triggerEvent = "Start Phase 2", priority = 95, silenceChance = 0.05f, canPlayDuringCooldown = true });
            eventRules.Add(new BarkEventRule { triggerEvent = "Omega 1", priority = 90, silenceChance = 0f, canPlayDuringCooldown = true });
            eventRules.Add(new BarkEventRule { triggerEvent = "Omega 2", priority = 90, silenceChance = 0f, canPlayDuringCooldown = true });
            eventRules.Add(new BarkEventRule { triggerEvent = "Player Ultimate", priority = 85, silenceChance = 0.1f, canPlayDuringCooldown = true });
            eventRules.Add(new BarkEventRule { triggerEvent = "Lance Omega Melee", priority = 85, silenceChance = 0f, canPlayDuringCooldown = true });
            eventRules.Add(new BarkEventRule { triggerEvent = "Lance Omega Ranged", priority = 85, silenceChance = 0f, canPlayDuringCooldown = true });
            eventRules.Add(new BarkEventRule { triggerEvent = "Shotgun Omega", priority = 85, silenceChance = 0f, canPlayDuringCooldown = true });
            eventRules.Add(new BarkEventRule { triggerEvent = "Trident Omega", priority = 85, silenceChance = 0f, canPlayDuringCooldown = true });
            eventRules.Add(new BarkEventRule { triggerEvent = "Enemy Defeated", priority = 80, silenceChance = 0.25f, canPlayDuringCooldown = true });
            eventRules.Add(new BarkEventRule { triggerEvent = "Lance Big Laser", priority = 75, silenceChance = 0f, canPlayDuringCooldown = true });
            eventRules.Add(new BarkEventRule { triggerEvent = "Lance Crash Down", priority = 75, silenceChance = 0f, canPlayDuringCooldown = true });
            eventRules.Add(new BarkEventRule { triggerEvent = "Lance Straight Charge", priority = 75, silenceChance = 0f, canPlayDuringCooldown = true });
            eventRules.Add(new BarkEventRule { triggerEvent = "Lance Tracking Laser", priority = 75, silenceChance = 0f, canPlayDuringCooldown = true });
            eventRules.Add(new BarkEventRule { triggerEvent = "Shotgun Fire", priority = 75, silenceChance = 0f, canPlayDuringCooldown = true });
            eventRules.Add(new BarkEventRule { triggerEvent = "Shotgun Panic", priority = 75, silenceChance = 0f, canPlayDuringCooldown = true });
            eventRules.Add(new BarkEventRule { triggerEvent = "Trident Stab", priority = 75, silenceChance = 0f, canPlayDuringCooldown = true });
            eventRules.Add(new BarkEventRule { triggerEvent = "Trident Sweep", priority = 75, silenceChance = 0f, canPlayDuringCooldown = true });
            eventRules.Add(new BarkEventRule { triggerEvent = "Player Take Damage", priority = 70, silenceChance = 0.35f, canPlayDuringCooldown = true });
            eventRules.Add(new BarkEventRule { triggerEvent = "Player Special Attack", priority = 55, silenceChance = 0.45f });
            eventRules.Add(new BarkEventRule { triggerEvent = "Spin Out", priority = 45, silenceChance = 0.35f, canPlayDuringCooldown = true });
            eventRules.Add(new BarkEventRule { triggerEvent = "Dash Away From Player", priority = 45, silenceChance = 0.45f, canPlayDuringCooldown = true });
            eventRules.Add(new BarkEventRule { triggerEvent = "Dash Towards Player", priority = 45, silenceChance = 0.45f, canPlayDuringCooldown = true });
            eventRules.Add(new BarkEventRule { triggerEvent = "Enemy Spawn", priority = 40, silenceChance = 0.45f });
            eventRules.Add(new BarkEventRule { triggerEvent = "Attacking Player", priority = 35, silenceChance = 0.45f });
            eventRules.Add(new BarkEventRule { triggerEvent = "Launch Forward", priority = 35, silenceChance = 0.45f });
            eventRules.Add(new BarkEventRule { triggerEvent = "Move Towards Player", priority = 35, silenceChance = 0.45f });
            eventRules.Add(new BarkEventRule { triggerEvent = "Wind Up", priority = 35, silenceChance = 0.45f });
            eventRules.Add(new BarkEventRule { triggerEvent = "Enemy Take Damage", priority = 30, silenceChance = 0.45f });
            eventRules.Add(new BarkEventRule { triggerEvent = "Player Basic Attack", priority = 20, silenceChance = 0.45f });
            eventRules.Add(new BarkEventRule { triggerEvent = "Player Basic Attack (Ranged)", priority = 20, silenceChance = 0.45f });
            eventRules.Add(new BarkEventRule { triggerEvent = "Player Dash", priority = 45, silenceChance = 0.5f, canPlayDuringCooldown = true });
        }

        eventRuleLookup.Clear();
        foreach (BarkEventRule eventRule in eventRules)
        {
            if (eventRule == null || string.IsNullOrWhiteSpace(eventRule.triggerEvent))
            {
                continue;
            }

            eventRuleLookup[eventRule.triggerEvent] = eventRule;
        }
    }

    private BarkEventRule GetEventRule(string triggerEvent)
    {
        if (!string.IsNullOrWhiteSpace(triggerEvent))
        {
            if (eventRuleLookup.TryGetValue(triggerEvent, out BarkEventRule exactRule))
            {
                return exactRule;
            }

            if (triggerEvent.StartsWith("Enemy Defeated", System.StringComparison.OrdinalIgnoreCase) &&
                eventRuleLookup.TryGetValue("Enemy Defeated", out BarkEventRule enemyDefeatedRule))
            {
                return enemyDefeatedRule;
            }

            if (triggerEvent.StartsWith("Player Take Damage", System.StringComparison.OrdinalIgnoreCase) &&
                eventRuleLookup.TryGetValue("Player Take Damage", out BarkEventRule playerDamageRule))
            {
                return playerDamageRule;
            }
        }

        return new BarkEventRule { triggerEvent = triggerEvent, priority = 10, silenceChance = defaultSilenceChance };
    }

    private HashSet<string> GetValidSources(string triggerEvent, string source)
    {
        HashSet<string> validSources = new HashSet<string>();
        string normalizedSource = source ?? "";

        /* Enemy defeated is the big mixed pool:
           generic enemy lines + exact enemy lines + elite generic lines if needed
           + the player's most recent arm lines, if an arm was used recently. */
        if (triggerEvent.StartsWith("Enemy Defeated", System.StringComparison.OrdinalIgnoreCase))
        {
            validSources.Add("Enemy (Any)");
            AddSpecificAndGenericEnemySource(validSources, normalizedSource);

            if (Time.time - recentPlayerBarkSourceTime <= RecentPlayerBarkSourceWindow)
            {
                AddSpecificAndGenericPartSource(validSources, recentPlayerBarkSource, "Arm (Any)");
            }

            return validSources;
        }

        if (triggerEvent.StartsWith("Player Take Damage", System.StringComparison.OrdinalIgnoreCase))
        {
            validSources.Add("Player Health");
            AddSpecificAndGenericEnemySource(validSources, normalizedSource);
            return validSources;
        }

        if (triggerEvent == "Game Over")
        {
            if (string.Equals(normalizedSource, "Player Health", System.StringComparison.OrdinalIgnoreCase))
            {
                validSources.Add("Round Manager");
            }

            if (!string.IsNullOrWhiteSpace(normalizedSource))
            {
                validSources.Add(normalizedSource);
            }

            return validSources;
        }

        if (triggerEvent == "Player Basic Attack" || triggerEvent == "Player Special Attack")
        {
            // Every arm attack can use exact arm lines plus the generic arm lines.
            AddSpecificAndGenericPartSource(validSources, normalizedSource, "Arm (Any)");

            if (triggerEvent == "Player Basic Attack" && IsRangedArmSource(normalizedSource))
            {
                validSources.Add("Arm (Ranged)");
            }

            return validSources;
        }

        if (triggerEvent == "Player Ultimate")
        {
            AddSpecificAndGenericPartSource(validSources, normalizedSource, "Chassis (Any)");
            return validSources;
        }

        if (!string.IsNullOrWhiteSpace(normalizedSource))
        {
            validSources.Add(normalizedSource);
        }

        return validSources;
    }

    private void AddSpecificAndGenericEnemySource(HashSet<string> validSources, string source)
    {
        if (string.IsNullOrWhiteSpace(source))
        {
            return;
        }

        validSources.Add(source);

        if (source.StartsWith("Elite Enemy", System.StringComparison.OrdinalIgnoreCase))
        {
            validSources.Add("Elite Enemy (Any)");
        }
    }

    private void AddSpecificAndGenericPartSource(HashSet<string> validSources, string source, string genericSource)
    {
        if (!string.IsNullOrWhiteSpace(genericSource))
        {
            validSources.Add(genericSource);
        }

        if (!string.IsNullOrWhiteSpace(source))
        {
            validSources.Add(source);
        }
    }

    private void RememberPlayerBarkSource(string source)
    {
        if (string.IsNullOrWhiteSpace(source))
        {
            return;
        }

        recentPlayerBarkSource = source;
        recentPlayerBarkSourceTime = Time.time;
    }

    private bool IsRangedArmSource(string source)
    {
        return string.Equals(source, "Tiger Arm", System.StringComparison.OrdinalIgnoreCase) ||
               string.Equals(source, "Shark Arm", System.StringComparison.OrdinalIgnoreCase);
    }

    private string ResolvePartSource(string partName, string fallbackSource)
    {
        if (string.IsNullOrWhiteSpace(partName))
        {
            return fallbackSource;
        }

        // Part display names and prefab names do not always match the bark sheet names.
        string normalized = partName.Replace(" ", "").Replace("-", "").ToLowerInvariant();
        if (normalized.Contains("tiger") || normalized.Contains("stripedshredder"))
            return "Tiger Arm";
        if (normalized.Contains("shark") || normalized.Contains("greatwhitewatergun"))
            return "Shark Arm";
        if (normalized.Contains("snake"))
            return "Snake Arm";
        if (normalized.Contains("katana") || normalized.Contains("samurai") || normalized.Contains("cursedkatana") || normalized.Contains("onisamurai"))
            return "Katana Arm";
        if (normalized.Contains("locomotive") || normalized.Contains("fullsteamfist"))
            return "Locomotive Arm";
        if (normalized.Contains("shinkansen") || normalized.Contains("shinkanstriker"))
            return "Shinkansen Arm";
        if (normalized.Contains("dragon") || normalized.Contains("scaleblazer"))
            return "Dragon Chassis";
        if (normalized.Contains("eagle") || normalized.Contains("avianarmor"))
            return "Eagle Chassis";
        if (normalized.Contains("train") || normalized.Contains("stationcenturion"))
            return "Train Chassis";

        return fallbackSource;
    }

    private BarkLine PickTutorialLine(string section, string scene, BarkSpeaker? speaker)
    {
        IEnumerable<BarkLine> candidates = barkDatabase.lines.Where(line =>
            line.tutorial &&
            string.Equals(line.section, section, System.StringComparison.OrdinalIgnoreCase));

        if (!string.IsNullOrWhiteSpace(scene))
        {
            candidates = candidates.Where(line => string.Equals(line.scene, scene, System.StringComparison.OrdinalIgnoreCase));
        }

        if (speaker.HasValue)
        {
            candidates = candidates.Where(line => line.speaker == speaker.Value);
        }

        List<BarkLine> matches = candidates.ToList();
        if (matches.Count == 0)
        {
            return null;
        }

        List<BarkLine> freshMatches = matches
            .Where(line => !recentLineNumbers.Contains(line.lineNumber))
            .ToList();

        if (freshMatches.Count > 0)
        {
            matches = freshMatches;
        }

        return matches[UnityEngine.Random.Range(0, matches.Count)];
    }

    private SpeakerBarkPrefab GetSpeakerPrefab(BarkSpeaker speaker)
    {
        if (speakerPrefabLookup != null && speakerPrefabLookup.TryGetValue(speaker, out SpeakerBarkPrefab speakerPrefab))
        {
            return speakerPrefab;
        }

        return null;
    }

    private GameObject GetFallbackLayout(BarkSpeaker speaker)
    {
        if (speaker == BarkSpeaker.Fleck && barkPrefabs != null && barkPrefabs.Length > 0)
        {
            return barkPrefabs[0];
        }

        if (barkPrefabs != null && barkPrefabs.Length > 1)
        {
            return barkPrefabs[1];
        }

        return barkPrefabs != null && barkPrefabs.Length > 0 ? barkPrefabs[0] : null;
    }

    private void SetBarkVisuals(GameObject spawnedBark, List<Sprite> expressions, string dialogue)
    {
        if (spawnedBark == null)
        {
            return;
        }

        Image image = spawnedBark.GetComponent<Image>();
        if (image != null && expressions != null && expressions.Count > 0)
        {
            image.sprite = expressions[UnityEngine.Random.Range(0, expressions.Count)];
        }

        TextMeshProUGUI text = spawnedBark.GetComponentInChildren<TextMeshProUGUI>();
        if (text != null)
        {
            text.text = dialogue;
        }
    }

    private GameObject SpawnLegacyBark(GameObject layout, Sprite[] sprites, int expression, string dialogue)
    {
        if (layout == null)
        {
            return null;
        }

        GameObject spawnedBark = Instantiate(layout, this.transform);
        Image image = spawnedBark.GetComponent<Image>();
        if (image != null && sprites != null && expression >= 0 && expression < sprites.Length)
        {
            image.sprite = sprites[expression];
        }

        TextMeshProUGUI text = spawnedBark.GetComponentInChildren<TextMeshProUGUI>();
        if (text != null)
        {
            text.text = dialogue;
        }

        return spawnedBark;
    }

    private float PlayAudio(BarkLine line)
    {
        if (!playBarkAudio || audioSource == null || string.IsNullOrWhiteSpace(line.audioResourcePath))
        {
            return 0f;
        }

        AudioClip clip = Resources.Load<AudioClip>(line.audioResourcePath);
        if (clip != null)
        {
            EnsureSharedBarkAudioSource();
            AudioSource source = sharedBarkAudioSource != null ? sharedBarkAudioSource : audioSource;
            source.PlayOneShot(clip, barkVolume);
            if (duckMusicDuringBarks && AudioManager.Instance != null)
            {
                AudioManager.Instance.DuckMusicForSeconds(clip.length);
            }

            return clip.length;
        }
        else
        {
            Debug.LogWarning($"Bark audio clip not found at Resources path '{line.audioResourcePath}' for line {line.lineNumber}.", this);
        }

        return 0f;
    }

    public void SetBarkVolume(float volume)
    {
        barkVolume = Mathf.Clamp(volume, 0f, 5f);
        PlayerPrefs.SetFloat(BarkVolumePrefsKey, barkVolume);

        if (audioSource != null)
        {
            audioSource.volume = 1f;
        }

        if (sharedBarkAudioSource != null)
        {
            sharedBarkAudioSource.volume = 1f;
        }
    }

    public float GetBarkVolume()
    {
        return barkVolume;
    }

    public float GetCurrentBarkDuration()
    {
        return currentBarkDuration;
    }

    private void EnsureAudioListenerExists()
    {
        AudioListener[] listeners = FindObjectsOfType<AudioListener>();
        bool hasEnabledListener = listeners.Any(listener =>
            listener != null &&
            listener.enabled &&
            listener.gameObject.activeInHierarchy);

        if (hasEnabledListener)
        {
            return;
        }

        EnsureSharedBarkAudioSource();
        if (sharedBarkAudioListener == null && sharedBarkAudioObject != null)
        {
            sharedBarkAudioListener = sharedBarkAudioObject.AddComponent<AudioListener>();
        }

        if (sharedBarkAudioListener != null)
        {
            sharedBarkAudioListener.enabled = true;
        }

        Debug.LogWarning("No enabled AudioListener was found, so BarkManager added one for voice line playback.", this);
    }

    private void EnsureSharedBarkAudioSource()
    {
        if (sharedBarkAudioSource != null)
        {
            return;
        }

        sharedBarkAudioObject = new GameObject("Bark Audio Source");
        DontDestroyOnLoad(sharedBarkAudioObject);
        sharedBarkAudioSource = sharedBarkAudioObject.AddComponent<AudioSource>();
        sharedBarkAudioSource.playOnAwake = false;
        sharedBarkAudioSource.spatialBlend = 0f;
        sharedBarkAudioSource.volume = 1f;
    }

    private void MoveExistingBarks()
    {
        foreach (Transform bark in transform)
        {
            RectTransform barkTransform = bark.GetComponent<RectTransform>();

            barkTransform.DOAnchorPosY(barkTransform.anchoredPosition.y - barkSpacing, 0.5f, true).SetEase(Ease.OutExpo);
        }
    }

    private bool TryParseSpeaker(string speaker, out BarkSpeaker parsedSpeaker)
    {
        if (string.IsNullOrWhiteSpace(speaker))
        {
            parsedSpeaker = default;
            return false;
        }

        string normalizedSpeaker = speaker.Replace(" ", string.Empty).Replace("-", string.Empty);
        return System.Enum.TryParse(normalizedSpeaker, true, out parsedSpeaker);
    }

    private void StartBarkLock(GameObject spawnedBark)
    {
        StartBarkLock(spawnedBark, 0f);
    }

    private void MatchBarkLifetimeToAudio(GameObject spawnedBark, float audioDuration)
    {
        if (spawnedBark == null || audioDuration <= 0f)
        {
            return;
        }

        if (spawnedBark.TryGetComponent(out BarkLifetime barkLifetime))
        {
            barkLifetime.MatchAudioDuration(audioDuration);
        }
    }

    private void StartBarkLock(GameObject spawnedBark, float audioDuration)
    {
        float duration = fallbackBarkLockDuration;
        if (spawnedBark != null && spawnedBark.TryGetComponent(out BarkLifetime barkLifetime))
        {
            duration = barkLifetime.TotalLifetime;
        }

        duration = Mathf.Max(duration, audioDuration);
        currentBarkDuration = duration;
        StartCoroutine(BarkCooldown(duration));
    }

    private bool IsPriorityBark(string triggerEvent)
    {
        return string.Equals(triggerEvent, "Round Start", System.StringComparison.OrdinalIgnoreCase) ||
               string.Equals(triggerEvent, "Victory", System.StringComparison.OrdinalIgnoreCase) ||
               string.Equals(triggerEvent, "Game Over", System.StringComparison.OrdinalIgnoreCase);
    }

    private void StopActiveBarkPlayback()
    {
        StopAllCoroutines();
        canBark = true;
        cooldownUntil = 0f;
        currentBarkDuration = 0f;

        if (sharedBarkAudioSource != null)
        {
            sharedBarkAudioSource.Stop();
        }

        for (int i = transform.childCount - 1; i >= 0; i--)
        {
            Destroy(transform.GetChild(i).gameObject);
        }
    }

    private void RememberLine(BarkLine line)
    {
        if (line == null || string.IsNullOrWhiteSpace(line.lineNumber))
        {
            return;
        }

        recentLineNumbers.Enqueue(line.lineNumber);
        while (recentLineNumbers.Count > RecentLineHistoryLimit)
        {
            recentLineNumbers.Dequeue();
        }
    }

    IEnumerator BarkCooldown(float duration)
    {
        canBark = false;
        yield return new WaitForSeconds(duration);
        canBark = true;
        currentBarkDuration = 0f;
        cooldownUntil = Time.time + postBarkCooldown;
    }

    public void StopCurrentBark()
    {
        StopActiveBarkPlayback(); // already exists, just make it public
    }
    
}
