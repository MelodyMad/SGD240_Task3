using UnityEngine;

public interface IMapGenerator
{
    /// <summary>
    /// Returns the world height at a specific position.
    /// </summary>
    float GetHeightAtPosition(Vector3 worldPosition);

    /// <summary>
    /// Clamps a position to stay inside the map boundaries.
    /// </summary>
    Vector3 ClampToMap(Vector3 position, float margin = 1f);

    /// <summary>
    /// Optional: Returns normalized height (0-1) at a position.
    /// </summary>
    float GetNormalizedHeightAtPosition(Vector3 worldPosition);

    void ApplyErosionAtPosition(Vector3 worldPosition);

    // Methods all map generators must implement
    void GenerateMap();         // Generate the terrain mesh
    void RebuildNavMesh();      // Build or update the NavMesh
}

