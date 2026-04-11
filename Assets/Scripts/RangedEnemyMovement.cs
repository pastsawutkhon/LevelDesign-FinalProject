using UnityEngine;
using UnityEngine.AI;
using System.Collections;

public class RangedEnemyMovement : MonoBehaviour
{
    [Header("จุดลาดตระเวน (Waypoints)")]
    public Transform[] waypoints;
    public float waitTime = 2f;
    
    [Header("ความเร็ว")]
    public float patrolSpeed = 2f;
    public float chaseSpeed = 3.5f;

    [Header("ระยะโจมตีไกล")]
    public float stopRange = 8f;        // ระยะที่จะหยุดเดินเพื่อยืนยิง
    public float attackRange = 10f;     // ระยะสูงสุดที่ยิงถึง
    public LayerMask obstructionMask;   // เลเยอร์กำแพง (Default)

    [Header("การโจมตี")]
    public GameObject bulletPrefab;      // กระสุน
    public Transform firePoint;          // จุดที่กระสุนออกจากปืน
    public float fireRate = 1.5f;        // ยิงทุกๆกี่วินาที
    private float nextFireTime;

    private NavMeshAgent agent;
    private EnemyFOV fov;
    private int currentWaypointIndex = 0;
    private bool isWaiting = false;

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        fov = GetComponent<EnemyFOV>();
        agent.stoppingDistance = stopRange; // ตั้งระยะหยุดให้ไกลขึ้น
    }

    void Update()
    {
        if (fov.canSeePlayer)
        {
            HandleRangedCombat();
        }
        else
        {
            ResumePatrol();
        }
    }

    void HandleRangedCombat()
    {
        StopAllCoroutines();
        isWaiting = false;
        agent.speed = chaseSpeed;

        float distanceToPlayer = Vector3.Distance(transform.position, fov.playerTransform.position);
        
        // 1. หันหน้าหาผู้เล่นตลอดเวลาที่เจอตัว
        LookAtPlayer();

        // 2. เช็คว่ามีอะไรบังหน้าไหม (Line of Sight)
        bool hasClearShot = !Physics.Linecast(firePoint.position, fov.playerTransform.position + Vector3.up, obstructionMask);

        if (distanceToPlayer <= attackRange && hasClearShot)
        {
            // --- อยู่ในระยะและไม่มีอะไรบัง -> หยุดและยิง ---
            agent.isStopped = true;

            if (Time.time >= nextFireTime)
            {
                Attack();
                nextFireTime = Time.time + fireRate;
            }
        }
        else
        {
            // --- ถ้ายิงไม่ได้ หรือยังอยู่ไกลเกินไป -> เดินเข้าไปหา ---
            agent.isStopped = false;
            agent.SetDestination(fov.playerTransform.position);
        }
    }

    void Attack()
    {
        Debug.Log("Ranged Enemy: ยิง!");
        if (bulletPrefab != null && firePoint != null)
        {
            Instantiate(bulletPrefab, firePoint.position, firePoint.rotation);
        }
    }

    void LookAtPlayer()
    {
        Vector3 direction = (fov.playerTransform.position - transform.position).normalized;
        direction.y = 0;
        Quaternion lookRotation = Quaternion.LookRotation(direction);
        transform.rotation = Quaternion.Slerp(transform.rotation, lookRotation, Time.deltaTime * 5f);
    }

    void ResumePatrol()
    {
        agent.isStopped = false;
        agent.speed = patrolSpeed;
        agent.stoppingDistance = 0.1f; // กลับไปเดินถึงจุด Waypoint จริงๆ
        Patrol();
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