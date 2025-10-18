using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Spawns multiple AI agents on a generated terrain that implements IMapGenerator. Agents are spawned within a given radius, live for a set time, and are automatically destroyed.
/// </summary>

public class AIAgentSpawner : MonoBehaviour
{
    [Header("References")]
    public GameObject agentPrefab; // Prefab of the AI agent to spawn
    public MonoBehaviour mapGeneratorScript; // Reference to any map generator script implementing IMapGenerator

    private IMapGenerator mapGenerator; // Interface reference for flexible map interaction

    [Header("Spawn Settings")]
    public int numberOfAgents = 10;  // Number of agents to spawn
    public float spawnRadius = 50f; // Radius around this object to spawn agents

    [Header("Agent Lifetime")]
    public float agentLifetime = 10f; // How long each agent remains active before being destroyed

    private List<GameObject> spawnedAgents = new List<GameObject>(); // Keeps track of all spawned agents

    // When the game starts
    void Start()
    {
        // Attempt to cast the MonoBehaviour to IMapGenerator
        mapGenerator = mapGeneratorScript as IMapGenerator;
        if (mapGenerator == null)
        {
            Debug.LogError("mapGeneratorScript must implement IMapGenerator!");
            return;
        }

        // Generate the map and build a NavMesh before spawning agents
        mapGenerator.GenerateMap();
        mapGenerator.RebuildNavMesh();

        // Begin spawning agents
        StartCoroutine(SpawnAgents());
    }

    // Coroutine that spawns all agents gradually.
    IEnumerator SpawnAgents()
    {
        for (int i = 0; i < numberOfAgents; i++)
        {
            // Find a valid point on the terrain to spawn the agent
            Vector3 spawnPos = GetValidSpawnPosition();

            if (spawnPos != Vector3.zero)
            {
                // Create the agent at the spawn position
                GameObject agent = Instantiate(agentPrefab, spawnPos, Quaternion.identity);
                // Assign the map generator reference to the agent’s AI script
                AIAgent ai = agent.GetComponent<AIAgent>();
                if (ai != null)
                {
                    ai.mapGeneratorScript = mapGeneratorScript; // Assign reference
                }
                spawnedAgents.Add(agent);
                // Schedule the agent for destruction after its lifetime expires
                StartCoroutine(DespawnAgentAfterDelay(agent, agentLifetime));
            }
            // Wait a frame before spawning the next agent
            yield return null;
        }
    }

    // Finds a valid ground position by raycasting downward from random points within the spawn radius.
    Vector3 GetValidSpawnPosition()
    {
        for (int attempt = 0; attempt < 30; attempt++)
        {
            Vector3 randomPos = transform.position + Random.insideUnitSphere * spawnRadius;
            randomPos.y = 100f; // Start ray above the terrain

            if (Physics.Raycast(randomPos, Vector3.down, out RaycastHit hit, 200f))
            {
                // Return the point where the ray hits the ground
                return hit.point;
            }
        }
        // No valid spawn position found
        return Vector3.zero;
    }

    // Waits for a delay, then destroys the agent and removes it from the tracking list.
    IEnumerator DespawnAgentAfterDelay(GameObject agent, float delay)
    {
        yield return new WaitForSeconds(delay);

        if (agent != null)
        {
            Destroy(agent);
            spawnedAgents.Remove(agent);
        }
    }
}
