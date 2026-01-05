using UnityEngine;
using UnityEngine.AI;

public class NPCSleepController : MonoBehaviour
{
    public float sleepDistance = 30f;

    private Transform player;
    private NavMeshAgent agent;
    private Animator anim;
    private Renderer[] renderers;

    private bool isSleeping = false;
    public bool IsSleeping => isSleeping;

    void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
        anim = GetComponentInChildren<Animator>();
        renderers = GetComponentsInChildren<Renderer>();
    }

    void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player")?.transform;
    }

    void Update()
    {
        if (player == null) return;

        float dist = Vector3.Distance(transform.position, player.position);
        bool visible = IsVisible();

        if ((dist > sleepDistance || !visible) && !isSleeping)
            Sleep();

        else if ((dist <= sleepDistance && visible) && isSleeping)
            Wake();
    }

    bool IsVisible()
    {
        foreach (var r in renderers)
        {
            if (r.isVisible)
                return true;
        }
        return false;
    }

    void Sleep()
    {
        isSleeping = true;

        if (agent != null)
        {
            agent.isStopped = true;
            agent.enabled = false;
        }

        if (anim != null)
            anim.enabled = false;
    }

    void Wake()
    {
        isSleeping = false;

        if (agent != null)
            agent.enabled = true;

        if (anim != null)
            anim.enabled = true;
    }
}