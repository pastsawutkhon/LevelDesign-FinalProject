using UnityEngine;
using UnityEngine.AI;
using System.Collections;

public class TacticalRangedEnemy : MonoBehaviour
{
    [Header("จุดลาดตระเวน (Waypoints)")]
    public Transform[] waypoints;
    public float waitTime = 2f;
    private int currentWaypointIndex = 0;
    private bool isWaiting = false;

    [Header("ระยะและเลเยอร์")]
    public float attackRange = 15f;      // ระยะยิงไกลสุด
    public float minSafeDistance = 5f;   // ระยะที่จะไม่เดินเข้าไปใกล้กว่านี้ (รักษาระยะห่าง)
    public LayerMask wallLayer;          // เลเยอร์กำแพงที่บังกระสุน (ห้ามยิงทะลุ)

    [Header("การเช็ควิถีกระสุน")]
    public float bulletRadius = 0.5f;    // ขนาดรัศมีกระสุนตามที่คุณกำหนด

    [Header("การยิง")]
    public GameObject bulletPrefab;
    public Transform firePoint;
    public float fireRate = 2f;
    private float nextFireTime;

    private NavMeshAgent agent;
    private EnemyFOV fov;
    private bool isSearchingForSpot = false;

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        fov = GetComponent<EnemyFOV>();

        if (agent != null)
        {
            // ปิดการหมุนอัตโนมัติเพื่อให้ LookAtPlayer คุมการหันหน้าเอง (กันหันหลังเดิน)
            agent.updateRotation = false; 
            // เพิ่มความเร่งเพื่อให้ AI ขยับหาช่องยิงได้คล่องตัว
            agent.acceleration = 30f; 
        }
    }

    void Update()
    {
        if (fov != null && fov.canSeePlayer)
        {
            // จ้องหน้าผู้เล่นตลอดเวลา
            LookAtPlayer();
            HandleTacticalMovement();
        }
        else
        {
            Patrol();
        }
    }

    void HandleTacticalMovement()
    {
        float distanceToPlayer = Vector3.Distance(transform.position, fov.playerTransform.position);
    
        // เช็ควิถีกระสุน (SphereCast 0.5)
        bool hasClearShot = CheckClearShot(firePoint.position, fov.playerTransform.position + Vector3.up);

        // --- 1. ระบบการยิง (ลำดับความสำคัญสูงสุด) ---
        // ยิงได้ทันทีถ้าทางสะดวก และอยู่ในระยะยิง (ไม่สนว่าจะใกล้แค่ไหน)
        if (hasClearShot && distanceToPlayer <= attackRange)
        {
            if (Time.time >= nextFireTime)
            {
                Shoot();
                nextFireTime = Time.time + fireRate;
            }
        }
    

        // --- 2. ระบบการเคลื่อนที่ (รักษาระยะห่าง) ---
        // ถ้าใกล้เกิน minSafeDistance (5 เมตร) ให้พยายามหาที่ยืนใหม่ที่ไกลกว่าเดิม
        if (distanceToPlayer < minSafeDistance || !hasClearShot)
        {
            agent.isStopped = false;
            if (!isSearchingForSpot)
            {
                StartCoroutine(FindNewShootingSpot());
            }
        }
        
        else
        {
            // ถ้าอยู่ในระยะที่เหมาะสม (เกิน 5 เมตร) และยิงได้แล้ว ให้หยุดยืนนิ่งๆ เพื่อยิง
            agent.isStopped = true;
        }
    }

    // ฟังก์ชันเช็ควิถีกระสุนแบบมีความหนา
    bool CheckClearShot(Vector3 start, Vector3 target)
    {
        Vector3 direction = (target - start).normalized;
        float distance = Vector3.Distance(start, target);

        // ยิงทรงกลมขนาด 0.5 ออกไปเช็ค ถ้าชน wallLayer แสดงว่ายิงติดขอบ
        if (Physics.SphereCast(start, bulletRadius, direction, out RaycastHit hit, distance, wallLayer))
        {
            return false; 
        }
        return true; 
    }

    IEnumerator FindNewShootingSpot()
    {
        isSearchingForSpot = true;
        Vector3 bestSpot = transform.position;
        bool foundSpot = false;

        // สุ่มหาจุดรอบๆ ตัวที่ยิงได้
        for (int i = 0; i < 15; i++)
        {
            Vector3 randomDir = Random.insideUnitSphere * 10f; 
            Vector3 testPos = transform.position + randomDir;

            NavMeshHit hit;
            if (NavMesh.SamplePosition(testPos, out hit, 10f, NavMesh.AllAreas))
            {
                float dist = Vector3.Distance(hit.position, fov.playerTransform.position);

                if (dist <= attackRange && dist >= minSafeDistance)
                {
                    if (CheckClearShot(hit.position + Vector3.up, fov.playerTransform.position + Vector3.up))
                    {
                        bestSpot = hit.position;
                        foundSpot = true;
                        break;
                    }
                }
            }
        }

        if (foundSpot)
        {
            agent.SetDestination(bestSpot);
        }
        else
        {
            // ถ้าหาจุดยิงไม่ได้ ให้เดินถอยออกจากผู้เล่นตรงๆ เพื่อรักษาระยะ
            Vector3 retreatDir = (transform.position - fov.playerTransform.position).normalized;
            agent.SetDestination(transform.position + retreatDir * 5f);
        }

        yield return new WaitForSeconds(0.5f); 
        isSearchingForSpot = false;
    }

    void Shoot()
    {
        if (bulletPrefab != null && firePoint != null)
        {
            Instantiate(bulletPrefab, firePoint.position, firePoint.rotation);
        }
    }

    void LookAtPlayer()
    {
        Vector3 direction = (fov.playerTransform.position - transform.position).normalized;
        direction.y = 0;
        if (direction != Vector3.zero)
        {
            Quaternion lookRotation = Quaternion.LookRotation(direction);
            transform.rotation = Quaternion.Slerp(transform.rotation, lookRotation, Time.deltaTime * 10f);
        }
    }

    void Patrol()
    {
        if (waypoints.Length == 0 || isWaiting) return;

        agent.isStopped = false;
        agent.speed = 2f; // ความเร็วเดินลาดตระเวน

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