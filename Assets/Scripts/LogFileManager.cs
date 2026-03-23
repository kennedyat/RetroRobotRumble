using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LogFileManager : MonoBehaviour
{
    // Singleton instance
    public static LogFileManager Instance { get; private set; }
    
    // Define a log file path
    [SerializeField] private string logFilePath;
    
    // Combat Tracking
    private int normalFireCount = 0;
    private int specialFireCount = 0;
    private int deathCount = 0;
    private int damageDealt = 0;
    private int damageTaken = 0;
    private int enemiesDefeated = 0;
    private int hitsLanded = 0;
    private int hitsMissed = 0;
    
    // Gameplay Actions
    private int pauseCount = 0;
    private int resumeCount = 0;
    private float sessionDuration = 0f;
    private float timePlayedInCombat = 0f;
    
    // Items and Resources
    private int itemsCollected = 0;
    private int partsEquipped = 0;
    private int stickersApplied = 0;
    
    // Robot Building
    private int timesVisitedBuildABot = 0;
    private int robotsBuilt = 0;
    
    // Movement and Distance
    private float distanceTraveled = 0f;
    
    // Buffs and Debuffs
    private int buffsApplied = 0;
    private int debuffsApplied = 0;
    
    // General Statistics
    private int totalPlaySessions = 0;
    private int levelOrZoneChanges = 0;

    void Start() 
    {
        // Initialize singleton
        if (Instance == null)
        {
            Instance = this;
        }
        else if (Instance != this)
        {
            Destroy(gameObject);
        }
        
        // Set up the log file path
        // logFilePath = Path.Combine(Application.persistentDataPath, "testLog.txt");
        Debug.Log("File Path: " + logFilePath);
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    protected void TrackNormalFireCount() 
    {
        normalFireCount++;
    }

    protected void TrackSpecialFireCount() 
    {
        specialFireCount++;
    }

    protected void TrackDeathCount() 
    {
        deathCount++;
    }

    // Combat Tracking Methods
    protected void TrackDamageDealt(int amount) 
    {
        damageDealt += amount;
    }

    protected void TrackDamageTaken(int amount) 
    {
        damageTaken += amount;
    }

    protected void TrackEnemyDefeated() 
    {
        enemiesDefeated++;
    }

    protected void TrackHitLanded() 
    {
        hitsLanded++;
    }

    protected void TrackHitMissed() 
    {
        hitsMissed++;
    }

    // Gameplay Action Methods
    protected void TrackPause() 
    {
        pauseCount++;
    }

    protected void TrackResume() 
    {
        resumeCount++;
    }

    protected void TrackSessionDuration(float duration) 
    {
        sessionDuration += duration;
    }

    protected void TrackCombatTime(float duration) 
    {
        timePlayedInCombat += duration;
    }

    // Items and Resources Methods
    protected void TrackItemCollected() 
    {
        itemsCollected++;
    }

    protected void TrackPartEquipped() 
    {
        partsEquipped++;
    }

    protected void TrackStickerApplied() 
    {
        stickersApplied++;
    }

    // Robot Building Methods
    protected void TrackBuildABotVisit() 
    {
        timesVisitedBuildABot++;
    }

    protected void TrackRobotBuilt() 
    {
        robotsBuilt++;
    }

    // Movement Methods
    protected void TrackDistanceTraveled(float distance) 
    {
        distanceTraveled += distance;
    }

    // Buff/Debuff Methods
    protected void TrackBuffApplied() 
    {
        buffsApplied++;
    }

    protected void TrackDebuffApplied() 
    {
        debuffsApplied++;
    }

    // General Statistics Methods
    protected void TrackPlaySession() 
    {
        totalPlaySessions++;
    }

    protected void TrackLevelChange() 
    {
        levelOrZoneChanges++;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode) 
    {
        if (scene.name == "MainMenu_WINTER") 
        {
            CreateLogFile();
        }
    }

    private void CreateLogFile() 
    {
        string logContent = $"Player test session ended at {System.DateTime.Now}\n";
        
        // Combat Statistics
        logContent += $"=== COMBAT STATISTICS ===\n";
        logContent += $"Normal Shots Fired: {normalFireCount}\n";
        logContent += $"Special Shots Fired: {specialFireCount}\n";
        logContent += $"Deaths: {deathCount}\n";
        logContent += $"Damage Dealt: {damageDealt}\n";
        logContent += $"Damage Taken: {damageTaken}\n";
        logContent += $"Enemies Defeated: {enemiesDefeated}\n";
        logContent += $"Hits Landed: {hitsLanded}\n";
        logContent += $"Hits Missed: {hitsMissed}\n";
        
        // Gameplay Actions
        logContent += $"\n=== GAMEPLAY ACTIONS ===\n";
        logContent += $"Times Paused: {pauseCount}\n";
        logContent += $"Times Resumed: {resumeCount}\n";
        logContent += $"Total Session Duration: {sessionDuration:F2} seconds\n";
        logContent += $"Combat Time: {timePlayedInCombat:F2} seconds\n";
        
        // Items and Resources
        logContent += $"\n=== ITEMS & RESOURCES ===\n";
        logContent += $"Items Collected: {itemsCollected}\n";
        logContent += $"Parts Equipped: {partsEquipped}\n";
        logContent += $"Stickers Applied: {stickersApplied}\n";
        
        // Robot Building
        logContent += $"\n=== ROBOT BUILDING ===\n";
        logContent += $"BuildABot Visits: {timesVisitedBuildABot}\n";
        logContent += $"Robots Built: {robotsBuilt}\n";
        
        // Movement
        logContent += $"\n=== MOVEMENT ===\n";
        logContent += $"Distance Traveled: {distanceTraveled:F2} units\n";
        
        // Buffs and Debuffs
        logContent += $"\n=== EFFECTS ===\n";
        logContent += $"Buffs Applied: {buffsApplied}\n";
        logContent += $"Debuffs Applied: {debuffsApplied}\n";
        
        // General Statistics
        logContent += $"\n=== GENERAL STATISTICS ===\n";
        logContent += $"Play Sessions: {totalPlaySessions}\n";
        logContent += $"Level/Zone Changes: {levelOrZoneChanges}\n";
        logContent += $"\n---\n\n";

        if (!File.Exists(logFilePath)) 
        {
            File.WriteAllText(logFilePath, logContent);
        }
        else 
        {
            File.AppendAllText(logFilePath, logContent);
        }

        Debug.Log("Log written: " + logContent);
    }
}
