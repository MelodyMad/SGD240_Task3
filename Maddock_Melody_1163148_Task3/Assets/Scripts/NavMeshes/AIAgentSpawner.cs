using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AIAgentSpawner : MonoBehaviour
{
    [Header("References")]
    public GameObject agentPrefab; // Prefab of the AI agent to spawn
    public MonoBehaviour mapGeneratorScript; // Any map generator implementing IMapGenerator

    private IMapGenerator mapGenerator; // Interface reference

    [Header("Spawn Settings")]
    public int numberOfAgents = 10;
    public float spawnRadius = 50f;

    [Header("Agent Lifetime")]
    public float agentLifetime = 10f;

    private List<GameObject> spawnedAgents = new List<GameObject>();

    void Start()
    {
        // Cast the MonoBehaviour to the interface
        mapGenerator = mapGeneratorScript as IMapGenerator;
        if (mapGenerator == null)
        {
            Debug.LogError("mapGeneratorScript must implement IMapGenerator!");
            return;
        }

        // Generate map and build NavMesh
        mapGenerator.GenerateMap();
        mapGenerator.RebuildNavMesh();

        StartCoroutine(SpawnAgents());
    }

    IEnumerator SpawnAgents()
    {
        for (int i = 0; i < numberOfAgents; i++)
        {
            Vector3 spawnPos = GetValidSpawnPosition();

            if (spawnPos != Vector3.zero)
            {
                GameObject agent = Instantiate(agentPrefab, spawnPos, Quaternion.identity);

                AIAgent ai = agent.GetComponent<AIAgent>();
                if (ai != null)
                {
                    ai.mapGeneratorScript = mapGeneratorScript; // Assign reference
                }

                spawnedAgents.Add(agent);
                StartCoroutine(DespawnAgentAfterDelay(agent, agentLifetime));
            }

            yield return null;
        }
    }

    Vector3 GetValidSpawnPosition()
    {
        for (int attempt = 0; attempt < 30; attempt++)
        {
            Vector3 randomPos = transform.position + Random.insideUnitSphere * spawnRadius;
            randomPos.y = 100f;

            if (Physics.Raycast(randomPos, Vector3.down, out RaycastHit hit, 200f))
            {
                return hit.point;
            }
        }

        return Vector3.zero;
    }

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
