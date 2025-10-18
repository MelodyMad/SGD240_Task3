using UnityEngine;
using UnityEngine.AI;

/// <summary>
/// <summary>
/// Controls AI agents that roam randomly within a defined radius on the NavMesh.
/// Integrates with any terrain generator implementing IMapGenerator to sample height, apply erosion, and constrain movement within map bounds.
/// </summary>

[RequireComponent(typeof(NavMeshAgent))]
public class AIAgent : MonoBehaviour
{
    [Header("References")]
    public MonoBehaviour mapGeneratorScript; // Assigned map generator script in the Inspector
    private IMapGenerator mapGenerator; // Interface reference for flexible map generator use

    [Header("Movement Settings")]
    [SerializeField] private float moveRadius = 50f; // Max distance the agent can move from its current position
    [SerializeField] private float moveInterval = 5f; // Time between picking new destinations
    [Range(0, 1)][SerializeField] private float minHeight = 0.2f; // Minimum allowed terrain height
    [Range(0, 1)][SerializeField] private float maxHeight = 0.8f; // Maximum allowed terrain height
    [SerializeField] private float speed = 3.5f; // Agent movement speed

    [Header("Erosion Settings")]
    [SerializeField] private bool enableErosion = true; // Option for the agent to cause erosion while moving

    private NavMeshAgent agent; 
    private float moveTimer;
    private bool isMoving;

    // When the game starts
    void Start()
    {
        agent = GetComponent<NavMeshAgent>();

        // Validate that the assigned script implements IMapGenerator
        mapGenerator = mapGeneratorScript as IMapGenerator;
        if (mapGenerator == null)
        {
            Debug.LogError("Assigned map generator does not implement IMapGenerator!");
            enabled = false;
            return;
        }

        agent.speed = speed; // Stagger movement timing between agents
        moveTimer = Random.Range(0, moveInterval); // Start by moving somewhere random
        SetRandomDestination();
    }

    // Updated every frame
    void Update()
    {
        if (mapGenerator == null || agent == null)
            return;

        moveTimer -= Time.deltaTime;

        // Only apply erosion if the agent is actually moving
        if (enableErosion && agent.velocity.sqrMagnitude > 0.01f)
        {
            mapGenerator.ApplyErosionAtPosition(transform.position);
        }
        // When timer expires, pick a new destination
        if (moveTimer <= 0f)
        {
            Vector3 newPos = GetRandomNavPosition();
            if (newPos != Vector3.zero)
            {
                agent.SetDestination(newPos);
            }
            moveTimer = moveInterval;
        }
    }

    // Finds a valid random position on the NavMesh within the move radius. Only returns positions within the specified height range.
    private Vector3 GetRandomNavPosition()
    {
        for (int attempt = 0; attempt < 10; attempt++)
        {
            Vector3 randomDirection = transform.position + Random.insideUnitSphere * moveRadius;

            if (NavMesh.SamplePosition(randomDirection, out NavMeshHit hit, moveRadius, NavMesh.AllAreas))
            {
                float height = mapGenerator.GetHeightAtPosition(hit.position);
                float normalizedHeight = mapGenerator.GetNormalizedHeightAtPosition(hit.position);
                // Only move to terrain within desired height limits
                if (normalizedHeight >= minHeight && normalizedHeight <= maxHeight)
                {
                    return mapGenerator.ClampToMap(hit.position);
                }
            }
        }

        return Vector3.zero; // Return zero if no valid spot found
    }

    // Sets an initial random destination for the agent when spawned.
    private void SetRandomDestination()
    {
        Vector3 randomPoint = transform.position + Random.insideUnitSphere * moveRadius;
        if (NavMesh.SamplePosition(randomPoint, out NavMeshHit hit, moveRadius, NavMesh.AllAreas))
        {
            Vector3 clampedPos = mapGenerator.ClampToMap(hit.position);
            agent.SetDestination(clampedPos);
        }
    }
}
