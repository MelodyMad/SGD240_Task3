using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
public class AIAgent : MonoBehaviour
{
    [Header("References")]
    public MapGeneratorWithErosion mapGenerator;

    [Header("Movement Settings")]
    [SerializeField] private float moveRadius = 30f;
    [SerializeField] private float moveInterval = 3f;
    [SerializeField] private float preferredMinHeight = 0.2f;
    [SerializeField] private float preferredMaxHeight = 0.8f;
    [SerializeField] private float agentSpeed = 3.5f;

    [Header("Erosion Settings")]
    [SerializeField] private bool enableErosion = true;
    [SerializeField] private float erosionInterval = 0.1f;

    private NavMeshAgent agent;
    private float moveTimer;
    private float erosionTimer;
    private bool isMoving;

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();

        if (agent == null)
        {
            Debug.LogError("Missing NavMeshAgent on AI Agent!");
            enabled = false;
            return;
        }

        if (mapGenerator == null)
            mapGenerator = FindObjectOfType<MapGeneratorWithErosion>();

        agent.speed = agentSpeed;
        moveTimer = Random.Range(0, moveInterval); // stagger movements
    }

    void Update()
    {
        if (mapGenerator == null || agent == null)
            return;

        moveTimer -= Time.deltaTime;
        erosionTimer -= Time.deltaTime;

        // Only erode when moving
        if (isMoving && mapGenerator != null)
        {
            mapGenerator.ApplyErosionAtPosition(transform.position);
        }

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

        // Stop erosion if agent stops moving
        if (!agent.pathPending && agent.remainingDistance <= agent.stoppingDistance)
        {
            isMoving = false;
        }
    }

    private Vector3 GetRandomNavPosition()
    {
        if (mapGenerator == null)
            return Vector3.zero;

        for (int attempt = 0; attempt < 10; attempt++)
        {
            Vector3 randomDirection = Random.insideUnitSphere * moveRadius;
            randomDirection += transform.position;

            if (NavMesh.SamplePosition(randomDirection, out NavMeshHit hit, moveRadius, NavMesh.AllAreas))
            {
                float height = mapGenerator.GetHeightAtPosition(hit.position);
                float normalizedHeight = height / mapGenerator.HeightMultiplier;

                // Only accept positions in preferred height range
                if (normalizedHeight >= preferredMinHeight && normalizedHeight <= preferredMaxHeight)
                    return hit.position;
            }
        }

        return Vector3.zero; // fallback
    }

    public void SetMapGenerator(MapGeneratorWithErosion generator)
    {
        mapGenerator = generator;
    }

}




