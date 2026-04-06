using UnityEngine;

/// <summary>
/// HeatmapRenderer visualizes the grid heatmap using Gizmos (debug visualization).
/// Shows grid cells colored by heat intensity in the Scene view.
/// </summary>
public class HeatmapRenderer : MonoBehaviour
{
    [SerializeField] private bool renderHeatmap = true;
    [SerializeField] private Color coldColor = new Color(0f, 0f, 1f, 1f);    // Blue
    [SerializeField] private Color hotColor = new Color(1f, 0f, 0f, 1f);     // Red
    [SerializeField] private float yHeight = 0.1f;  // Height to draw grid at

    private GridSystem gridSystem;

    private void Start()
    {
        Debug.Log("[HeatmapRenderer] Start() called");
        
        HeatmapTracker tracker = HeatmapTracker.Instance;
        if (tracker == null)
        {
            Debug.LogError("[HeatmapRenderer] HeatmapTracker.Instance is NULL!");
            enabled = false;
            return;
        }

        gridSystem = tracker.GetGridSystem();
        if (gridSystem == null)
        {
            Debug.LogError("[HeatmapRenderer] GridSystem is NULL!");
            enabled = false;
            return;
        }

        Debug.Log($"[HeatmapRenderer] ✓ Initialized for {gridSystem.GetGridWidth()}x{gridSystem.GetGridDepth()} grid");
    }

    private void OnDrawGizmos()
    {
        if (!renderHeatmap)
            return;

        HeatmapTracker tracker = HeatmapTracker.Instance;
        if (tracker == null)
            return;

        GridSystem grid = tracker.GetGridSystem();
        if (grid == null)
            return;

        DrawGrid(grid);
    }

    private void DrawGrid(GridSystem grid)
    {
        int gridWidth = grid.GetGridWidth();
        int gridDepth = grid.GetGridDepth();
        int maxHeat = grid.GetMaxHeatValue();

        if (maxHeat == 0)
            maxHeat = 1;  // Avoid division by zero

        for (int x = 0; x < gridWidth; x++)
        {
            for (int z = 0; z < gridDepth; z++)
            {
                int heatValue = grid.GetHeatValue(x, z);
                
                // Only draw cells with heat (visited cells)
                if (heatValue == 0)
                    continue;
                
                float normalizedHeat = (float)heatValue / maxHeat;
                
                // Lerp between cold and hot colors
                Color cellColor = Color.Lerp(coldColor, hotColor, normalizedHeat);
                
                // Get cell center position
                Vector3 cellPos = grid.GetCellWorldPosition(x, z);
                cellPos.y = yHeight;
                
                float cellSize = grid.GetCellSize();
                
                // Draw cell as a cube
                Gizmos.color = cellColor;
                Gizmos.DrawCube(cellPos, new Vector3(cellSize, 0.1f, cellSize));
                
                // Draw wireframe
                Gizmos.color = Color.black;
                DrawWireCube(cellPos, new Vector3(cellSize, 0.1f, cellSize));
            }
        }
    }

    /// <summary>
    /// Draw a wireframe cube using Gizmos.
    /// </summary>
    private void DrawWireCube(Vector3 center, Vector3 size)
    {
        Vector3 halfSize = size * 0.5f;
        
        // Front face
        Gizmos.DrawLine(center + new Vector3(-halfSize.x, -halfSize.y, halfSize.z), 
                        center + new Vector3(halfSize.x, -halfSize.y, halfSize.z));
        Gizmos.DrawLine(center + new Vector3(halfSize.x, -halfSize.y, halfSize.z), 
                        center + new Vector3(halfSize.x, halfSize.y, halfSize.z));
        Gizmos.DrawLine(center + new Vector3(halfSize.x, halfSize.y, halfSize.z), 
                        center + new Vector3(-halfSize.x, halfSize.y, halfSize.z));
        Gizmos.DrawLine(center + new Vector3(-halfSize.x, halfSize.y, halfSize.z), 
                        center + new Vector3(-halfSize.x, -halfSize.y, halfSize.z));

        // Back face
        Gizmos.DrawLine(center + new Vector3(-halfSize.x, -halfSize.y, -halfSize.z), 
                        center + new Vector3(halfSize.x, -halfSize.y, -halfSize.z));
        Gizmos.DrawLine(center + new Vector3(halfSize.x, -halfSize.y, -halfSize.z), 
                        center + new Vector3(halfSize.x, halfSize.y, -halfSize.z));
        Gizmos.DrawLine(center + new Vector3(halfSize.x, halfSize.y, -halfSize.z), 
                        center + new Vector3(-halfSize.x, halfSize.y, -halfSize.z));
        Gizmos.DrawLine(center + new Vector3(-halfSize.x, halfSize.y, -halfSize.z), 
                        center + new Vector3(-halfSize.x, -halfSize.y, -halfSize.z));

        // Vertical edges
        Gizmos.DrawLine(center + new Vector3(-halfSize.x, -halfSize.y, halfSize.z), 
                        center + new Vector3(-halfSize.x, -halfSize.y, -halfSize.z));
        Gizmos.DrawLine(center + new Vector3(halfSize.x, -halfSize.y, halfSize.z), 
                        center + new Vector3(halfSize.x, -halfSize.y, -halfSize.z));
        Gizmos.DrawLine(center + new Vector3(halfSize.x, halfSize.y, halfSize.z), 
                        center + new Vector3(halfSize.x, halfSize.y, -halfSize.z));
        Gizmos.DrawLine(center + new Vector3(-halfSize.x, halfSize.y, halfSize.z), 
                        center + new Vector3(-halfSize.x, halfSize.y, -halfSize.z));
    }

    /// <summary>
    /// Enable/disable heatmap rendering.
    /// </summary>
    public void SetRenderActive(bool active)
    {
        renderHeatmap = active;
    }

    /// <summary>
    /// Debug method - call this from console to see if heatmap is working.
    /// </summary>
    public void DebugPrintStatus()
    {
        if (gridSystem == null)
        {
            Debug.LogError("[HeatmapRenderer] GridSystem is null!");
            return;
        }

        Debug.Log($"[HeatmapRenderer Debug]\n" +
            $"  Render Active: {renderHeatmap}\n" +
            $"  Grid Size: {gridSystem.GetGridWidth()}x{gridSystem.GetGridDepth()}\n" +
            $"  Total Visits: {gridSystem.GetTotalVisits()}\n" +
            $"  Max Heat: {gridSystem.GetMaxHeatValue()}\n" +
            $"  Average Heat: {gridSystem.GetAverageHeatValue():F2}");
    }
}
