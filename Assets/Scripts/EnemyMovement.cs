using UnityEngine;
using UnityEngine.AI;
using System.Collections;

public class EnemyMovement : MonoBehaviour
{
    [Header("จุดลาดตระเวน (Waypoints)")]
    public Transform[] waypoints;        // ลากตำแหน่ง x, y, z มาใส่เป็นลิสต์
    public float waitTime = 2f;          // หยุดรอกี่วิก่อนเดินกลับ
    
    [Header("ความเร็ว")]
    public float patrolSpeed = 2f;
    public float chaseSpeed = 5f;

    private NavMeshAgent agent;
    private EnemyFOV fov;
    private int currentWaypointIndex = 0;
    private bool isWaiting = false;

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        fov = GetComponent<EnemyFOV>();
        
        if (waypoints.Length > 0)
            agent.SetDestination(waypoints[currentWaypointIndex].position);
    }

    void Update()
    {
        if (fov.canSeePlayer)
        {
            // --- โหมดไล่ล่า ---
            StopAllCoroutines(); 
            isWaiting = false;
            agent.speed = chaseSpeed;
            agent.SetDestination(fov.playerTransform.position);
        }
        else
        {
            // --- โหมดลาดตระเวน ---
            agent.speed = patrolSpeed;
            Patrol();
        }
    }

    void Patrol()
    {
        if (waypoints.Length == 0 || isWaiting) return;

        // เช็คว่าเดินไปถึงจุดหมายหรือยัง
        if (!agent.pathPending && agent.remainingDistance < 0.5f)
        {
            StartCoroutine(WaitAndMoveToNextPoint());
        }
    }

    IEnumerator WaitAndMoveToNextPoint()
    {
        isWaiting = true;
        yield return new WaitForSeconds(waitTime);

        // เปลี่ยนไปยังจุดถัดไป (วนกลับมาจุดแรกถ้าถึงจุดสุดท้าย)
        currentWaypointIndex = (currentWaypointIndex + 1) % waypoints.Length;
        agent.SetDestination(waypoints[currentWaypointIndex].position);
        
        isWaiting = false;
    }
}