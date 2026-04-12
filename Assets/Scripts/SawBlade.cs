using UnityEngine;

public class SawBlade : MonoBehaviour
{
    [Header("ตั้งค่าการเคลื่อนที่")]
    public float moveSpeed = 3f;     
    public float moveDistance = 5f;  
    private Vector3 startPos;

    [Header("การทำดาเมจ (ปรับใหม่ตามคำขอ)")]
    public float damage = 1f;           // ดาเมจครั้งละ 1
    public float damageCooldown = 0.1f;   // โดนถี่ๆ ทุก 0.1 วินาที (1 วินาทีโดน 10 ครั้ง = 2 ดาเมจต่อวินาที)
    
    private float nextDamageTime = 0f;

    void Start()
    {
        startPos = transform.position;
        startPos.y = 1f; 
        transform.position = startPos;
    }

    void Update()
    {
        // ระบบเดินซ้ายขวา
        float newX = startPos.x + Mathf.Sin(Time.time * moveSpeed) * moveDistance;
        transform.position = new Vector3(newX, 1f, startPos.z);
    }

    private void OnCollisionStay(Collision collision)
    {
        // ตรวจจับดาเมจแบบถี่ๆ ตลอดเวลาที่ยังชนกันอยู่
        if (Time.time >= nextDamageTime)
        {
            DealDamage(collision.gameObject);
        }
    }

    void DealDamage(GameObject target)
    {
        bool hasDealtDamage = false;

        // 1. ดาเมจผู้เล่น
        PlayerHealth player = target.GetComponent<PlayerHealth>();
        if (player != null)
        {
            player.TakeDamage(damage);
            hasDealtDamage = true;
        }

        // 2. ดาเมจศัตรู
        Enemy enemy = target.GetComponent<Enemy>();
        if (enemy != null)
        {
            enemy.TakeDamage(damage);
            hasDealtDamage = true;
        }
        if (hasDealtDamage)
        {
            nextDamageTime = Time.time + damageCooldown; 
        }
    }
}