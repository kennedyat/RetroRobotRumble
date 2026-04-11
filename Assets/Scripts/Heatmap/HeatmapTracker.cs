using UnityEngine;

/// <summary>
/// HeatmapTracker wraps the GridSystem and manages continuous player position tracking.
/// Acts as a singleton for easy access during combat.
/// </summary>
public class HeatmapTracker : MonoBehaviour
{
    public static HeatmapTracker Instance { get; private set; }

    [SerializeField] private Transform playerTransform;
    [SerializeField] private float minX = -5f;
    [SerializeField] private float maxX = 42f;
    [SerializeField] private float minZ = -12f;
    [SerializeField] private float maxZ = 0f;
    [SerializeField] private float gridCellSize = 1f;
    [SerializeField] private float trackingInterval = 0.1f;  // 10Hz tracking
    [SerializeField] private bool trackPlayerPosition = true;
    
    private GridSystem gridSystem;
    private float trackingTimer;

    private void Awake()
    {
        Debug.Log("[HeatmapTracker] Awake() called");
        
        if (Instance != null && Instance != this)
        {
            Debug.LogWarning("[HeatmapTracker] Multiple instances detected. Destroying duplicate.");
            Destroy(gameObject);
            return;
        }
        
        Instance = this;
        
        // Initialize grid system early (in Awake, not Start)
        gridSystem = new GridSystem(minX, maxX, minZ, maxZ, gridCellSize);
        
        Debug.Log($"[HeatmapTracker] ✓ GridSystem initialized: {gridSystem.GetGridWidth()}x{gridSystem.GetGridDepth()} cells");
    }

    private void Start()
    {
        Debug.Log("[HeatmapTracker] Start() called");
        
        // Auto-find player if not assigned
        if (playerTransform == null)
        {
            PlayerController playerController = FindObjectOfType<PlayerController>();
            if (playerController != null)
            {
                playerTransform = playerController.transform;
                Debug.Log("[HeatmapTracker] Auto-found PlayerController");
            }
            else
            {
                Debug.LogError("[HeatmapTracker] Player not found! Make sure PlayerController exists in the scene or assign Player Transform manually.");
                enabled = false;
                return;
            }
        }

        trackingTimer = 0f;

        Debug.Log($"[HeatmapTracker] ✓ Initialized with bounds X[{minX}, {maxX}] Z[{minZ}, {maxZ}]");
    }

    private void Update()
    {
        if (!trackPlayerPosition || playerTransform == null || gridSystem == null)
            return;

        // Accumulate time until tracking interval is reached
        trackingTimer -= Time.deltaTime;
        if (trackingTimer <= 0f)
        {
            Vector3 playerPos = playerTransform.position;
            gridSystem.RecordPosition(playerPos);
            
            // Debug: Log occasionally
            if (gridSystem.GetTotalVisits() % 20 == 0)
            {
                Debug.Log($"[HeatmapTracker] Recording position {playerPos}. Total visits: {gridSystem.GetTotalVisits()}");
            }
            
            trackingTimer = trackingInterval;
        }
    }

    /// <summary>
    /// Get the grid system (for visualization or queries).
    /// </summary>
    public GridSystem GetGridSystem()
    {
        return gridSystem;
    }

    /// <summary>
    /// Enable/disable heatmap tracking.
    /// </summary>
    public void SetTrackingActive(bool active)
    {
        trackPlayerPosition = active;
        Debug.Log($"[HeatmapTracker] Tracking {(active ? "enabled" : "disabled")}");
    }

    /// <summary>
    /// Reset heatmap data (clears all tracking history).
    /// </summary>
    public void ResetHeatmap()
    {
        if (gridSystem != null)
        {
            gridSystem.Reset();
            trackingTimer = 0f;
            Debug.Log("[HeatmapTracker] Heatmap reset.");
        }
    }

    /// <summary>
    /// Log heatmap statistics for debugging.
    /// </summary>
    public void LogStatistics()
    {
        if (gridSystem == null)
            return;

        Debug.Log($"[HeatmapTracker] Statistics:\n" +
            $"  Total Visits: {gridSystem.GetTotalVisits()}\n" +
            $"  Grid Size: {gridSystem.GetGridWidth()}x{gridSystem.GetGridDepth()}\n" +
            $"  Max Heat Value: {gridSystem.GetMaxHeatValue()}\n" +
            $"  Average Heat: {gridSystem.GetAverageHeatValue():F2}");
    }
}
