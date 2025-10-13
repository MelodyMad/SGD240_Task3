using UnityEngine;
using UnityEngine.AI;

/// <summary>
/// This script controls the AI agents that roam around the NavMesh randomly. Using the variables the agent will move to random position within the defined radius at set time intervals. 
/// These destinations are chosen within the preferred height range and applied erosion as the agent moves. 
/// </summary>

// Ensure the GameObject has a NavMeshAgent component
[RequireComponent(typeof(NavMeshAgent))]

public class AIAgent : MonoBehaviour
{
    // Reference to the map generator for erosion and height checks
    [Header("References")]
    public ErosionMapGenerator mapGenerator; 

    [Header("Movement Settings")]
    [SerializeField] private float moveRadius = 50f; // Max distance for random movement
    [SerializeField] private float moveInterval = 5f; // Time between movement decisions
    [Range(0, 1)] [SerializeField] private float minHeight = 0.2f; // Minimum normalized height AI can move on
    [Range(0, 1)] [SerializeField] private float maxHeight = 0.8f; // Maximum normalized height AI can move on
    [SerializeField] private float speed = 3.5f; // NavMeshAgent speed

    [Header("Erosion Settings")]
    [SerializeField] private bool enableErosion = true; // Toggle erosion effect
    
    private NavMeshAgent agent; // Reference to NavMeshAgent
    private float moveTimer; // Timer for deciding next movement
    private bool isMoving; // Check if the agent is moving

    // When the game is started
    void Start()
    {
        // Referencing the NavMeshAgent
        agent = GetComponent<NavMeshAgent>();
        if (agent == null)
        {
            // Checking the existence of the NavMeshAgent
            Debug.LogError("Missing NavMeshAgent on AI Agent!");
            enabled = false;
            return;
        }

        // Find the map generator to assign it
        if (mapGenerator == null)
        {
            mapGenerator = Object.FindFirstObjectByType<ErosionMapGenerator>();
        }

        // Set the agent's speed
        agent.speed = speed;

        // Randomize initial move timer
        moveTimer = Random.Range(0, moveInterval);
    }

    // On every frame
    void Update()
    {
        if (mapGenerator == null || agent == null)
            return;

        // Update timer
        moveTimer -= Time.deltaTime;

        // Apply erosion at the agent's position if it is moving
        if (isMoving && enableErosion)
        {
            mapGenerator.ApplyErosionAtPosition(transform.position);
        }

        // Decide on a new random destination if timer has expired
        if (moveTimer <= 0f)
        {
            Vector3 newPos = GetRandomNavPosition();
            if (newPos != Vector3.zero)
            {
                agent.SetDestination(newPos);
                isMoving = true;
            }
            moveTimer = moveInterval;
        }

        // Check if agent the has reached destination and if they have stopped moving
        if (!agent.pathPending && agent.remainingDistance <= agent.stoppingDistance)
        {
            isMoving = false;
        }
    }

    // Get a random navigable position within moveRadius and within the preferred height ranges
    private Vector3 GetRandomNavPosition()
    {
        if (mapGenerator == null)
            return Vector3.zero;

        for (int attempt = 0; attempt < 10; attempt++)
        {
            // Pick a random point around the agent
            Vector3 randomDirection = Random.insideUnitSphere * moveRadius + transform.position;

            // Check if the random point is on the NavMesh
            if (NavMesh.SamplePosition(randomDirection, out NavMeshHit hit, moveRadius, NavMesh.AllAreas))
            {
                float height = mapGenerator.GetHeightAtPosition(hit.position);
                float normalizedHeight = height / mapGenerator.HeightMultiplier;

                // Only accept positions in the preferred height range
                if (normalizedHeight >= minHeight && normalizedHeight <= maxHeight)
                    return hit.position;
            }
        }

        // If no valid position found after attempts, retrace the steps
        return Vector3.zero;
    }

    // Allows other scripts to assign a map generator dynamically
    public void SetMapGenerator(ErosionMapGenerator generator)
    {
        mapGenerator = generator;
    }
}





