using UnityEngine;
using UnityEngine.AI;

public class NPCQuestController : MonoBehaviour
{
    public string CurrentQuestID { get; private set; }
    public NavMeshAgent agent;
    public Animator animator;

    private QuestData quest;
    private Transform questPoint;
    private QuestQueueManager manager;
    private Transform returnPoint;

    private bool isLeaving = false;
    private bool hasReachedQuestPoint = false;
    public bool IsReadyForInteraction => hasReachedQuestPoint && !isLeaving;

    public Transform player;
    public float rotateSpeed = 8f;

    public void Setup(
        QuestData q,
        Transform questPoint,
        Transform spawnPoint,
        QuestQueueManager m,
        bool alreadyAtPoint
    )
    {
        CurrentQuestID = q.questID;

        if (!player)
            player = GameObject.FindWithTag("Player")?.transform;

        quest = q;
        this.questPoint = questPoint;
        manager = m;
        returnPoint = spawnPoint;

        GetComponent<QuestTrigger>().questToGive = quest;

        agent.updateRotation = true;
        agent.isStopped = false;
        agent.stoppingDistance = 0.3f;

        SetAtQuest(false);
        SetWalking(true);

        if (!alreadyAtPoint)
            agent.SetDestination(questPoint.position);
        else
            ArriveAtQuestPoint();
    }

    // ================= NAVMESH =================
    void Update()
    {
        if (!isLeaving && !hasReachedQuestPoint)
        {
            if (!agent.pathPending && agent.remainingDistance <= agent.stoppingDistance)
            {
                ArriveAtQuestPoint();
            }
        }

        if (isLeaving && !agent.pathPending && agent.remainingDistance <= agent.stoppingDistance)
        {
            manager.OnNPCGone();
            Destroy(gameObject);
        }
    }

    void ArriveAtQuestPoint()
    {
        hasReachedQuestPoint = true;

        agent.isStopped = true;
        agent.updateRotation = false;
        agent.ResetPath();

        SetWalking(false);
        SetAtQuest(true);
    }

    // ================= ROTATE TO PLAYER =================
    void LateUpdate()
    {
        if (isLeaving) return;
        if (!hasReachedQuestPoint) return;
        if (!player) return;

        Vector3 dir = player.position - transform.position;
        dir.y = 0f;

        if (dir.sqrMagnitude < 0.01f) return;

        Quaternion targetRot = Quaternion.LookRotation(dir);

        transform.rotation = Quaternion.RotateTowards(
            transform.rotation,
            targetRot,
            rotateSpeed * 360f * Time.deltaTime
        );
    }

    // ================= EXIT =================
    public void LeaveAndDestroy()
    {
        isLeaving = true;

        agent.updateRotation = true;
        agent.isStopped = false;

        SetAtQuest(false);
        SetWalking(true);

        agent.SetDestination(returnPoint.position);
    }

    public void AssignNextQuest(QuestData nextQuest)
    {
        quest = nextQuest;
        CurrentQuestID = nextQuest.questID;

        GetComponent<QuestTrigger>().questToGive = nextQuest;

        // RESET STATE
        hasReachedQuestPoint = false;
        isLeaving = false;

        agent.isStopped = false;
        agent.updateRotation = true;
        agent.SetDestination(questPoint.position);

        SetWalking(true);
        SetAtQuest(false);
        GuideManager.Instance.AddTarget(transform);
    }

    // ================= ANIM =================
    void SetWalking(bool value)
    {
        animator.SetBool("IsWalking", value);
    }

    void SetAtQuest(bool value)
    {
        animator.SetBool("IsAtQuest", value);
    }

    public Transform GetReturnPoint()
    {
        return returnPoint;
    }
}