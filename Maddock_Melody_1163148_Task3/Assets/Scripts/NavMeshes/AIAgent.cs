using UnityEngine;
using UnityEngine.AI;

public class AIAgent : MonoBehaviour
{
    private NavMeshAgent agent;
    [SerializeField] private float wanderRadius = 20f;
    [SerializeField] private float wanderDelay = 2f;
    [SerializeField] private MapGeneratorWithErosion erosionScript; // ? Changed from GroundErosion

    private float timer;

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        timer = wanderDelay;
    }

    void Update()
    {
        // If agent is not yet on a NavMesh, skip
        if (!agent.isOnNavMesh)
            return;

        timer += Time.deltaTime;

        if (timer >= wanderDelay)
        {
            Vector3 newPos = RandomNavSphere(transform.position, wanderRadius, -1);
            agent.SetDestination(newPos);
            timer = 0;
        }

        // Optional: continuously erode as the AI walks around
        if (erosionScript != null)
        {
            erosionScript.ApplyErosionAtPosition(transform.position);
        }
    }

    public static Vector3 RandomNavSphere(Vector3 origin, float dist, int layermask)
    {
        Vector3 randDirection = Random.insideUnitSphere * dist;
        randDirection += origin;
        NavMeshHit navHit;
        NavMesh.SamplePosition(randDirection, out navHit, dist, layermask);
        return navHit.position;
    }
}

