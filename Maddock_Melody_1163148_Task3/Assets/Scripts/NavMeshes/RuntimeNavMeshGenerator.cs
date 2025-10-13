using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

/// <summary>
/// This script generates a NavMesh at runtime after the terrain has been generated which ensures that the AI agents can navigate dynamically through the environment.
/// </summary>

// Ensure the GameObject has a NavMeshSurface component
[RequireComponent(typeof(NavMeshSurface))]

public class RuntimeNavMeshGenerator : MonoBehaviour
{
    private NavMeshSurface surface; // Get a reference to the NavMeshSurface component

    // When the game starts
    void Start()
    {
        surface = GetComponent<NavMeshSurface>();
        // Start baking the NavMesh after the terrain has been generated
        StartCoroutine(BakeNavMeshAfterGeneration());
    }

    // Waits until the end of the frame to ensure terrain is fully generated, then builds the NavMesh at runtime.
    private IEnumerator BakeNavMeshAfterGeneration()
    {
        yield return new WaitForEndOfFrame(); // Wait a frame
        surface.BuildNavMesh(); // Bake the NavMesh
        Debug.Log("NavMesh baked at runtime!"); 
    }
}
