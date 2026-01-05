using System.Collections.Generic;
using UnityEngine;

public class Waypoint : MonoBehaviour
{
    public List<Waypoint> neighbors = new List<Waypoint>();

    [Header("Auto Connect")]
    public float autoConnectRadius = 6f;
    public LayerMask obstacleMask; // 🔥 layer penghalang (Building, Wall, etc)

    void OnDrawGizmos()
    {
        Gizmos.color = Color.yellow;
        foreach (var n in neighbors)
        {
            if (n != null)
                Gizmos.DrawLine(transform.position, n.transform.position);
        }

        Gizmos.color = Color.white;
        Gizmos.DrawSphere(transform.position, 0.2f);
    }

    public void AutoConnect(Waypoint[] all)
    {
        neighbors.Clear();

        foreach (var wp in all)
        {
            if (wp == this) continue;

            float dist = Vector3.Distance(transform.position, wp.transform.position);
            if (dist > autoConnectRadius) continue;

            // 🔥 cek penghalang pakai layer
            if (Physics.Linecast(
                transform.position,
                wp.transform.position,
                out RaycastHit hit,
                obstacleMask
            ))
            {
                // ada tembok / bangunan → jangan connect
                continue;
            }

            neighbors.Add(wp);
        }
    }
}