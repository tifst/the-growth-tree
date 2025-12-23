using System.Collections.Generic;
using UnityEngine;

public class Waypoint : MonoBehaviour
{
    public List<Waypoint> neighbors = new List<Waypoint>();

    void OnDrawGizmos()
    {
        Gizmos.color = Color.yellow;

        foreach (var n in neighbors)
        {
            if (n != null)
                Gizmos.DrawLine(transform.position, n.transform.position);
        }
    }

    // Dikosongkan (manual saja)
    public void AutoConnect(Waypoint[] all)
    {
        // Tidak perlu apa-apa
    }
}
