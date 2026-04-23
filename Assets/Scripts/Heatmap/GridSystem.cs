using UnityEngine;

/// <summary>
/// GridSystem manages a discretized grid overlay of the arena for heatmap tracking.
/// Converts continuous 3D positions into grid cells and maintains visit frequency data.
/// </summary>
public class GridSystem
{
    private int gridWidth;      // Number of cells along X axis
    private int gridDepth;      // Number of cells along Z axis
    private float cellSize;     // Size of each grid cell in world units
    
    private float minX;         // World space minimum X coordinate
    private float maxX;         // World space maximum X coordinate
    private float minZ;         // World space minimum Z coordinate
    private float maxZ;         // World space maximum Z coordinate
    
    private int[,] heatData;    // 2D array storing visit count per cell
    private int totalVisits;    // Total visit count across all cells

    /// <summary>
    /// Initialize the grid with specified bounds and cell size.
    /// </summary>
    public GridSystem(float minX, float maxX, float minZ, float maxZ, float cellSize = 1f)
    {
        this.minX = minX;
        this.maxX = maxX;
        this.minZ = minZ;
        this.maxZ = maxZ;
        this.cellSize = cellSize;
        
        // Calculate grid dimensions
        float rangeX = maxX - minX;
        float rangeZ = maxZ - minZ;
        
        gridWidth = Mathf.CeilToInt(rangeX / cellSize);
        gridDepth = Mathf.CeilToInt(rangeZ / cellSize);
        
        // Initialize heatmap data
        heatData = new int[gridWidth, gridDepth];
        totalVisits = 0;
        
        Debug.Log($"[GridSystem] Initialized {gridWidth}x{gridDepth} grid " +
            $"({rangeX:F2} x {rangeZ:F2} units with {cellSize}m cells)");
    }

    /// <summary>
    /// Record a player position in the grid.
    /// Increments the visit count for the cell containing the position.
    /// </summary>
    public void RecordPosition(Vector3 worldPosition)
    {
        if (!IsPositionInBounds(worldPosition))
            return;

        int cellX = GetCellX(worldPosition.x);
        int cellZ = GetCellZ(worldPosition.z);
        
        // Ensure indices are within bounds
        cellX = Mathf.Clamp(cellX, 0, gridWidth - 1);
        cellZ = Mathf.Clamp(cellZ, 0, gridDepth - 1);
        
        heatData[cellX, cellZ]++;
        totalVisits++;
    }

    /// <summary>
    /// Get the heat value (visit count) at a specific grid cell.
    /// </summary>
    public int GetHeatValue(int cellX, int cellZ)
    {
        if (cellX < 0 || cellX >= gridWidth || cellZ < 0 || cellZ >= gridDepth)
            return 0;
        
        return heatData[cellX, cellZ];
    }

    /// <summary>
    /// Get the normalized heat value (0-1) for visualization purposes.
    /// </summary>
    public float GetNormalizedHeatValue(int cellX, int cellZ)
    {
        if (totalVisits == 0)
            return 0f;
        
        int heatValue = GetHeatValue(cellX, cellZ);
        return (float)heatValue / totalVisits;
    }

    /// <summary>
    /// Get the world position of a grid cell's center.
    /// </summary>
    public Vector3 GetCellWorldPosition(int cellX, int cellZ)
    {
        float worldX = minX + cellX * cellSize + cellSize * 0.5f;
        float worldZ = minZ + cellZ * cellSize + cellSize * 0.5f;
        return new Vector3(worldX, 0f, worldZ);
    }

    /// <summary>
    /// Convert world X coordinate to grid X index.
    /// </summary>
    public int GetCellX(float worldX)
    {
        return Mathf.FloorToInt((worldX - minX) / cellSize);
    }

    /// <summary>
    /// Convert world Z coordinate to grid Z index.
    /// </summary>
    public int GetCellZ(float worldZ)
    {
        return Mathf.FloorToInt((worldZ - minZ) / cellSize);
    }

    /// <summary>
    /// Check if a world position is within the grid bounds.
    /// </summary>
    public bool IsPositionInBounds(Vector3 worldPosition)
    {
        return worldPosition.x >= minX && worldPosition.x <= maxX &&
               worldPosition.z >= minZ && worldPosition.z <= maxZ;
    }

    /// <summary>
    /// Clear all heatmap data.
    /// </summary>
    public void Reset()
    {
        heatData = new int[gridWidth, gridDepth];
        totalVisits = 0;
    }

    /// <summary>
    /// Get grid dimensions.
    /// </summary>
    public int GetGridWidth() => gridWidth;
    public int GetGridDepth() => gridDepth;
    public float GetCellSize() => cellSize;
    public int GetTotalVisits() => totalVisits;
    
    /// <summary>
    /// Get the maximum heat value in the grid.
    /// </summary>
    public int GetMaxHeatValue()
    {
        int maxValue = 0;
        for (int x = 0; x < gridWidth; x++)
        {
            for (int z = 0; z < gridDepth; z++)
            {
                if (heatData[x, z] > maxValue)
                    maxValue = heatData[x, z];
            }
        }
        return maxValue;
    }

    /// <summary>
    /// Get the average heat value across all cells.
    /// </summary>
    public float GetAverageHeatValue()
    {
        if (gridWidth == 0 || gridDepth == 0)
            return 0f;
        
        int totalCells = gridWidth * gridDepth;
        return (float)totalVisits / totalCells;
    }
}
