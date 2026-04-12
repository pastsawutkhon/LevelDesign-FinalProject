using UnityEngine;
using UnityEngine.AI;
using System.Collections;

public class EnemyMovement : MonoBehaviour
{
    [Header("จุดลาดตระเวน (Waypoints)")]
    public Transform[] waypoints;
    public float waitTime = 2f;
    
    [Header("ความเร็ว")]
    public float patrolSpeed = 2f;
    public float chaseSpeed = 5f;

    private NavMeshAgent agent;
    private EnemyFOV fov;
    private MeleeEnemyAttack meleeAttackScript; // 1. ประกาศตัวแปรอ้างอิงสคริปต์โจมตี
    private int currentWaypointIndex = 0;
    private bool isWaiting = false;

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        fov = GetComponent<EnemyFOV>();
        // 2. ดึงคอมโพเนนต์มาเก็บไว้
        meleeAttackScript = GetComponent<MeleeEnemyAttack>();
        
        if (waypoints.Length > 0)
            agent.SetDestination(waypoints[currentWaypointIndex].position);
    }

    void Update()
    {
        // 🌟 3. ใส่บรรทัดนี้ที่จุดเริ่มต้นของ Update 🌟
        // ถ้ากำลังโจมตีอยู่ จะไม่รัน Code การเดิน/หันหน้า ด้านล่างต่อเลย
        if (meleeAttackScript != null && meleeAttackScript.isAttacking) 
        {
            return; 
        }

        if (fov.canSeePlayer)
        {
            // --- โหมดไล่ล่า ---
            StopAllCoroutines(); 
            isWaiting = false;
            agent.speed = chaseSpeed;
            agent.stoppingDistance = 2f;
            agent.SetDestination(fov.playerTransform.position);
        }
        else
        {
            // --- โหมดลาดตระเวน ---
            agent.speed = patrolSpeed;
            agent.stoppingDistance = 0f;
            Patrol();
        }
    }

    void Patrol()
    {
        if (waypoints.Length == 0 || isWaiting) return;

        if (!agent.pathPending && agent.remainingDistance < 0.5f)
        {
            StartCoroutine(WaitAndMoveToNextPoint());
        }
    }

    IEnumerator WaitAndMoveToNextPoint()
    {
        isWaiting = true;
        yield return new WaitForSeconds(waitTime);

        currentWaypointIndex = (currentWaypointIndex + 1) % waypoints.Length;
        agent.SetDestination(waypoints[currentWaypointIndex].position);
        
        isWaiting = false;
    }
}