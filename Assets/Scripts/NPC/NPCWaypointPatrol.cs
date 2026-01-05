using UnityEngine;
using UnityEngine.AI;

public class NPCWaypointPatrol : MonoBehaviour
{
    private NavMeshAgent agent;
    private Waypoint currentWaypoint;
    private Animator anim;
    private Waypoint[] cachedWaypoints;

    [Header("Patrol Settings")]
    public float reachDistance = 1f;   // Jarak untuk dianggap sampai waypoint
    public float waitTime = 1.2f;      // Waktu berhenti sebelum lanjut
    float actualWaitTime;
    private float waitTimer = 0f;

    [Header("Idle Look Around")]
    public float chanceToLookAround = 0.6f;
    public float lookAroundSpeed = 2f;
    public float maxLookAngle = 60f;

    private Quaternion lookTargetRotation;
    private bool isLookingAround = false;

    void Start()
    {
        anim = GetComponentInChildren<Animator>();
        agent = GetComponent<NavMeshAgent>();

        if (cachedWaypoints == null || cachedWaypoints.Length == 0)
        {
            cachedWaypoints = FindObjectsByType<Waypoint>(FindObjectsSortMode.None);
        }

        Waypoint[] allWaypoints = cachedWaypoints;

        if (allWaypoints.Length == 0)
        {
            Debug.LogError("[NPC] Tidak ada waypoint di scene!");
            enabled = false;
            return;
        }

        Collider[] hits = Physics.OverlapSphere(transform.position, 0.6f);
        foreach (var hit in hits)
        {
            if (hit.gameObject != gameObject && hit.CompareTag("NPC"))
            {
                Vector3 dir = transform.position - hit.transform.position;
                dir.y = 0;
                agent.Move(dir.normalized * Time.deltaTime * 0.3f);
            }
        }

        // Cari waypoint terdekat dari posisi NPC saat spawn
        currentWaypoint = GetNearestWaypoint(allWaypoints);

        MoveToWaypoint(currentWaypoint);
    }

    void Update()
    {
        if (agent.pathPending && agent.hasPath || !agent.enabled) return;

        // Jika sudah sampai waypoint
        if (!agent.pathPending &&
            agent.remainingDistance <= reachDistance &&
            agent.velocity.sqrMagnitude < 0.05f)
        {
            if (!isLookingAround && waitTimer == 0f)
            {
                float roll = Random.value;

                if (roll <= chanceToLookAround)
                {
                    StartLookAround();
                }
                else
                {
                    GoToNextNeighbor();
                    return;
                }
            }

            if (isLookingAround)
            {
                waitTimer += Time.deltaTime;
                LookAround();

                if (waitTimer >= waitTime)
                {
                    isLookingAround = false;
                    waitTimer = 0f;
                    GoToNextNeighbor();
                }
            }
        }

        if (anim != null)
        {
            float speed = agent.velocity.magnitude;
            anim.SetFloat("Speed", speed);
        }
    }

    // Pindah ke waypoint tetangga
    void GoToNextNeighbor()
    {
        if (currentWaypoint == null) return;

        if (currentWaypoint.neighbors.Count > 0)
        {
            // Pilih waypoint tetangga secara random
            Waypoint next = currentWaypoint.neighbors[
                Random.Range(0, currentWaypoint.neighbors.Count)
            ];

            currentWaypoint = next;
            MoveToWaypoint(next);
        }
        else
        {
            Debug.LogWarning("[NPC] Waypoint tanpa neighbor! Mencari waypoint terdekat...");

            Waypoint[] all = cachedWaypoints;
            currentWaypoint = GetNearestWaypoint(all);
            MoveToWaypoint(currentWaypoint);
        }
    }

    void MoveToWaypoint(Waypoint wp)
    {
        if (wp != null)
        {
            agent.SetDestination(wp.transform.position);
        }
    }

    // Cari waypoint terdekat saat awal spawn
    Waypoint GetNearestWaypoint(Waypoint[] all)
    {
        Waypoint nearest = null;
        float bestDist = Mathf.Infinity;

        foreach (var wp in all)
        {
            if (wp == null) continue; // 🔥 WAJIB

            float dist = Vector3.Distance(transform.position, wp.transform.position);
            if (dist < bestDist)
            {
                bestDist = dist;
                nearest = wp;
            }
        }
        return nearest;
    }

    void StartLookAround()
    {
        isLookingAround = true;
        waitTimer = 0f;
        actualWaitTime = Random.Range(waitTime * 0.6f, waitTime * 1.4f);

        float randomAngle = Random.Range(-maxLookAngle, maxLookAngle);
        lookTargetRotation = Quaternion.Euler(0f, transform.eulerAngles.y + randomAngle, 0f);
    }

    void LookAround()
    {
        transform.rotation = Quaternion.Slerp(
            transform.rotation,
            lookTargetRotation,
            Time.deltaTime * lookAroundSpeed
        );
    }
}