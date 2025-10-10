using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshSurface))]

public class RuntimeNavMeshGenerator : MonoBehaviour
{
    private NavMeshSurface surface;

    void Start()
    {
        surface = GetComponent<NavMeshSurface>();
        // Wait a frame so the terrain is fully generated
        StartCoroutine(BakeNavMeshAfterGeneration());
    }

    private IEnumerator BakeNavMeshAfterGeneration()
    {
        yield return new WaitForEndOfFrame();
        surface.BuildNavMesh();
        Debug.Log("NavMesh baked at runtime!");
    }
}
