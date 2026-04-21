using UnityEngine;
using UnityEngine.AI;
using System.Collections;

public class MeleeEnemyAttack : MonoBehaviour
{
    [Header("การตั้งค่าโจมตี")]
    public float attackDamage = 10f;
    public float attackCooldown = 1.5f; // ระยะห่างระหว่างแต่ละการโจมตี
    public float damageDelay = 0.2f;    // รอหลังจากเล่นแอนิเมชันกี่วินาทีถึงจะคำนวณดาเมจ
    public float stopDuration = 0.5f;   // ระยะเวลาที่หยุดนิ่ง (ไม่เดิน ไม่หมุน)

    [Header("อ้างอิง")]
    public Animator anim;
    public bool isAttacking = false;
    private NavMeshAgent agent;
    private EnemyFOV fov;
    private float nextAttackTime;
    public AudioSource attackSound; // เสียงตอนโจมตี (ลาก AudioSource มาใส่ตรงนี้)

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        fov = GetComponent<EnemyFOV>();
        if (anim == null) anim = GetComponentInChildren<Animator>();
    }

    void Update()
    {
        // ถ้าเจอ Player และอยู่ในระยะที่ศัตรูหยุดเดิน (Stopping Distance)
        if (fov.canSeePlayer && !isAttacking)
        {
            float distanceToPlayer = Vector3.Distance(transform.position, fov.playerTransform.position);
            
            // ตรวจสอบว่าอยู่ในระยะโจมตีและ Cooldown เสร็จหรือยัง
            if (distanceToPlayer <= agent.stoppingDistance && Time.time >= nextAttackTime)
            {
                StartCoroutine(PerformMeleeAttack());
            }
        }
    }

    IEnumerator PerformMeleeAttack()
    {
        isAttacking = true;
        nextAttackTime = Time.time + attackCooldown;

        // 1. หยุดเดินและหยุดหมุน
        agent.isStopped = true;
        // ปิดสคริปต์หันหน้าชั่วคราว (ถ้ามีสคริปต์หันแยก ให้ปิดที่นี่)
        // แต่ใน Update ของผมจะใช้ isAttacking กันไว้

        // 2. เล่นแอนิเมชันโจมตี (ตั้งชื่อ Parameter ใน Animator ว่า "Attack")
        if (anim != null) anim.SetTrigger("Attack");

        // 3. รอ 0.2 วินาทีตามจังหวะแอนิเมชันก่อนทำดาเมจ
        yield return new WaitForSeconds(damageDelay);
        attackSound.Play(); // เล่นเสียงตอนโจมตี

        // 4. คำนวณดาเมจแบบรูปกรวย (ใช้ค่าจาก FOV)
        CheckMeleeHit();

        // 5. รอให้ครบ 0.5 วินาที (จากจุดเริ่ม) ก่อนกลับไปทำงานต่อ
        // เราใช้เวลาไปแล้ว 0.2 วิ (damageDelay) ดังนั้นรอเพิ่มอีก 0.3 วิ
        yield return new WaitForSeconds(stopDuration - damageDelay);

        agent.isStopped = false;
        isAttacking = false;
    }

    void CheckMeleeHit()
    {
        Collider[] hitColliders = Physics.OverlapSphere(transform.position, fov.viewRadius, fov.targetMask);

        if (hitColliders.Length == 0) Debug.Log("ไม่เจอ Player ในระยะวงกลมเลย");

        foreach (var hitCollider in hitColliders)
        {
            Vector3 directionToTarget = (hitCollider.transform.position - transform.position).normalized;
            float angleToTarget = Vector3.Angle(transform.forward, directionToTarget);

            // วาดเส้น Debug ในหน้า Scene เพื่อดูว่าตอนมันตี มันเช็คไปที่ไหน
            Debug.DrawRay(transform.position + Vector3.up, directionToTarget * fov.viewRadius, Color.yellow, 1f);

            if (angleToTarget <= fov.viewAngle / 2)
            {
                // ลองคอมเมนต์บรรทัด Linecast ออกก่อนเพื่อเช็คว่าติดกำแพงทิพย์ไหม
                // if (!Physics.Linecast(...)) 
             
                PlayerHealth playerHealth = hitCollider.GetComponent<PlayerHealth>();
                if (playerHealth != null)
                {
                 playerHealth.TakeDamage(attackDamage);
                 Debug.Log("โดนตบแล้ว!");
                }
            }
            else 
            {
                Debug.Log("Player อยู่นอกกรวย! องศาที่เช็คได้คือ: " + angleToTarget);
            }
        }
    }
}