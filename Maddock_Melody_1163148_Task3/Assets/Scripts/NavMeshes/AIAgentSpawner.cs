using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// This script spawns a set number of AI agents within a radius around the spawner and automatically destory the agents after a set lifetime.
/// </summary>

public class AIAgentSpawner : MonoBehaviour
{
    [Header("References")]
    public GameObject agentPrefab; // Prefab of the AI agent to spawn
    public ErosionMapGenerator mapGenerator; // Reference to the map generator for the agents

    [Header("Spawn Settings")]
    public int numberOfAgents = 10; // Number of agents to spawn
    public float spawnRadius = 50f; // Radius around spawner for random positions

    [Header("Agent Lifetime")]
    public float agentLifetime = 10f; // Time in seconds before the agents are destoryed

    private List<GameObject> spawnedAgents = new List<GameObject>(); // To keep track of the spawned agents

    // When the game starts
    void Start()
    {
        // Ensure references are set correctly
        if (agentPrefab == null || mapGenerator == null)
        {
            Debug.LogError("Spawner missing references!");
            return;
        }

        // Begin spawning agents
        StartCoroutine(SpawnAgents());
    }

    // Spawns the agents one by one and starts their despawn timers
    IEnumerator SpawnAgents()
    {
        for (int i = 0; i < numberOfAgents; i++)
        {
            // Create a valid spawn position
            Vector3 spawnPos = GetValidSpawnPosition();

            if (spawnPos != Vector3.zero)
            {
                // Create the agent at a valid spawn point
                GameObject agent = Instantiate(agentPrefab, spawnPos, Quaternion.identity);
                AIAgent ai = agent.GetComponent<AIAgent>();

                if (ai != null)
                {
                    ai.mapGenerator = mapGenerator; // Assign map generator
                }

                spawnedAgents.Add(agent);

                // Start timer to destroy the agent after its lifetime
                StartCoroutine(DespawnAgentAfterDelay(agent, agentLifetime));
            }

            yield return null; // Small delay to prevent frame issues
        }
    }

    // Finds a valid spawn position within the radius using a downward raycast
    Vector3 GetValidSpawnPosition()
    {
        for (int attempt = 0; attempt < 30; attempt++)
        {
            Vector3 randomPos = transform.position + Random.insideUnitSphere * spawnRadius;
            randomPos.y = 100f; // Start raycast high above

            if (Physics.Raycast(randomPos, Vector3.down, out RaycastHit hit, 200f))
            {
                return hit.point; // Return the surface point
            }
        }

        return Vector3.zero; // Fallback if no valid point is found
    }

    // Destroys an agent after a delay and removes it from the list of objects in the scene
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


