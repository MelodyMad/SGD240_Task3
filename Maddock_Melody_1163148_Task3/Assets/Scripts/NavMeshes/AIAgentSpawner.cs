using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AIAgentSpawner : MonoBehaviour
{
    [Header("References")]
    public GameObject agentPrefab;
    public MapGeneratorWithErosion mapGenerator;

    [Header("Spawn Settings")]
    public int numberOfAgents = 10;
    public float spawnRadius = 50f;

    [Header("Agent Lifetime")]
    public float agentLifetime = 10f; // how long agents exist before despawning

    private List<GameObject> spawnedAgents = new List<GameObject>();

    void Start()
    {
        if (agentPrefab == null || mapGenerator == null)
        {
            Debug.LogError("Spawner missing references!");
            return;
        }

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
                    ai.mapGenerator = mapGenerator;
                }

                spawnedAgents.Add(agent);

                // Start despawn timer
                StartCoroutine(DespawnAgentAfterDelay(agent, agentLifetime));
            }

            yield return null; // small delay to avoid hiccups
        }
    }

    Vector3 GetValidSpawnPosition()
    {
        for (int attempt = 0; attempt < 30; attempt++)
        {
            Vector3 randomPos = transform.position + Random.insideUnitSphere * spawnRadius;
            randomPos.y = 100f; // high up for raycast

            if (Physics.Raycast(randomPos, Vector3.down, out RaycastHit hit, 200f))
            {
                // Just return the surface point — let AIAgent handle height logic
                return hit.point;
            }
        }

        return Vector3.zero; // fallback if no valid position found
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



