using UnityEngine;
using UnityEngine.AI;

public class NPCWaypointPatrol : MonoBehaviour
{
    private NavMeshAgent agent;
    private Waypoint currentWaypoint;

    [Header("Patrol Settings")]
    public float reachDistance = 1f;   // Jarak untuk dianggap sampai waypoint
    public float waitTime = 1.2f;      // Waktu berhenti sebelum lanjut

    private float waitTimer = 0f;

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();

        Waypoint[] allWaypoints = FindObjectsByType<Waypoint>(FindObjectsSortMode.None);

        if (allWaypoints.Length == 0)
        {
            Debug.LogError("[NPC] Tidak ada waypoint di scene!");
            enabled = false;
            return;
        }

        // Cari waypoint terdekat dari posisi NPC saat spawn
        currentWaypoint = GetNearestWaypoint(allWaypoints);

        MoveToWaypoint(currentWaypoint);
    }

    void Update()
    {
        if (agent.pathPending) return;

        // Jika sudah sampai waypoint
        if (agent.remainingDistance <= reachDistance)
        {
            waitTimer += Time.deltaTime;

            if (waitTimer >= waitTime)
            {
                GoToNextNeighbor();
                waitTimer = 0f;
            }
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

            Waypoint[] all = FindObjectsByType<Waypoint>(FindObjectsSortMode.None);
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
            float dist = Vector3.Distance(transform.position, wp.transform.position);
            if (dist < bestDist)
            {
                bestDist = dist;
                nearest = wp;
            }
        }

        return nearest;
    }
}
