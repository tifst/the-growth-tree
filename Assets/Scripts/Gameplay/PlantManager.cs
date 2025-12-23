using UnityEngine;

public class PlantManager : MonoBehaviour
{
    private PlantPlot[] plots;
    private Transform player;

    void Start()
    {
        plots = Object.FindObjectsByType<PlantPlot>(FindObjectsSortMode.None);
        player = GameObject.FindGameObjectWithTag("Player")?.transform;

        Debug.Log($"🌾 Ditemukan {plots.Length} petak tanah di scene.");
    }

    void Update()
    {
        if (player == null || plots == null || plots.Length == 0)
            return;

        // cari plot terdekat yang playerInRange = true
        PlantPlot nearest = null;
        float minDist = Mathf.Infinity;

        foreach (var plot in plots)
        {
            if (!plot.playerInRange) continue;

            float dist = Vector3.Distance(player.position, plot.transform.position);
            if (dist < minDist)
            {
                minDist = dist;
                nearest = plot;
            }
        }

        // highlight hanya plot terdekat
        foreach (var plot in plots)
            plot.SetHighlight(plot == nearest);

        // TIDAK ADA PlantTree() lagi DI SINI
    }
}