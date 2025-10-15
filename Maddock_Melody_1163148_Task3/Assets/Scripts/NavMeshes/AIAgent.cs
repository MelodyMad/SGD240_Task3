using UnityEngine;
using UnityEngine.AI;

/// <summary>
/// Controls AI agents that roam around randomly on the NavMesh,
/// using map height and erosion logic from any map generator implementing IMapGenerator.
/// </summary>
[RequireComponent(typeof(NavMeshAgent))]
public class AIAgent : MonoBehaviour
{
    [Header("References")]
    public MonoBehaviour mapGeneratorScript; // Drag your map generator here
    private IMapGenerator mapGenerator;      // Interface reference for flexibility

    [Header("Movement Settings")]
    [SerializeField] private float moveRadius = 50f;
    [SerializeField] private float moveInterval = 5f;
    [Range(0, 1)][SerializeField] private float minHeight = 0.2f;
    [Range(0, 1)][SerializeField] private float maxHeight = 0.8f;
    [SerializeField] private float speed = 3.5f;

    [Header("Erosion Settings")]
    [SerializeField] private bool enableErosion = true;

    private NavMeshAgent agent;
    private float moveTimer;
    private bool isMoving;

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();

        // Make sure the assigned script implements the interface
        mapGenerator = mapGeneratorScript as IMapGenerator;
        if (mapGenerator == null)
        {
            Debug.LogError("Assigned map generator does not implement IMapGenerator!");
            enabled = false;
            return;
        }

        agent.speed = speed;
        moveTimer = Random.Range(0, moveInterval);

        SetRandomDestination();
    }

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

    private Vector3 GetRandomNavPosition()
    {
        for (int attempt = 0; attempt < 10; attempt++)
        {
            Vector3 randomDirection = transform.position + Random.insideUnitSphere * moveRadius;

            if (NavMesh.SamplePosition(randomDirection, out NavMeshHit hit, moveRadius, NavMesh.AllAreas))
            {
                float height = mapGenerator.GetHeightAtPosition(hit.position);
                float normalizedHeight = mapGenerator.GetNormalizedHeightAtPosition(hit.position);

                if (normalizedHeight >= minHeight && normalizedHeight <= maxHeight)
                    return mapGenerator.ClampToMap(hit.position);
            }
        }

        return Vector3.zero;
    }

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
