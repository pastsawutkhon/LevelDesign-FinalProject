using UnityEngine;
using System.Collections;

public class Dynamite : MonoBehaviour
{
    [Header("การโยน (ปรับได้ใน Inspector)")]
    public float throwForce = 15f;
    public float throwUpwardForce = 5f;

    [Header("การระเบิด (Damage Falloff)")]
    public float explosionTimer = 3f;
    
    [Tooltip("ระยะวงในที่ดาเมจจะโดนเต็ม 100% เสมอ")]
    public float innerRadius = 2f;    
    
    [Tooltip("ระยะวงนอกสุดที่ระเบิดจะไปถึง")]
    public float outerRadius = 6f;    
    
    [Tooltip("ดาเมจสูงสุดเมื่ออยู่ในระยะวงใน")]
    public float maxDamage = 10f;     
    
    [Tooltip("เปอร์เซ็นต์ดาเมจต่ำสุดเมื่ออยู่ขอบวงนอกสุด (0.2 = 20%)")]
    public float minDamagePercent = 0.2f; 

    [Header("ระบบแรงกระแทก (Physics)")]
    public float boxExplosionForce = 20f;  
    public float BoxUpwardForce = 1f;      
    public float BoxLifeAfterHit = 2f;    

    [Header("เอฟเฟกต์")]
    public GameObject explosionFXPrefab;

    private Rigidbody rb;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        
        if (rb != null)
        {
            Vector3 throwVector = transform.forward * throwForce + transform.up * throwUpwardForce;
            rb.AddForce(throwVector, ForceMode.Impulse);
        }

        StartCoroutine(ExplodeSequence());
    }

    IEnumerator ExplodeSequence()
    {
        yield return new WaitForSeconds(explosionTimer);
        Explode();
    }

    void Explode()
    {
        Debug.Log("ตู้ม! ไดนาไมต์ระเบิด");

        if (explosionFXPrefab != null)
        {
            Instantiate(explosionFXPrefab, transform.position, Quaternion.identity);
        }

        // ตรวจจับรัศมีทั้งหมด โดยใช้วงนอกสุด (outerRadius)
        Collider[] colliders = Physics.OverlapSphere(transform.position, outerRadius);
        
        foreach (Collider hit in colliders)
        {
            // ---------------------------------------------------------
            // ระบบคำนวณดาเมจแบบมี Sweet Spot (0-2m = 100%, 2-6m = 100%->20%)
            // ---------------------------------------------------------
            float distance = Vector3.Distance(transform.position, hit.transform.position);
            float damageMultiplier = 1f;

            // 1. ถ้าอยู่ในวงใน (Sweet Spot) ดาเมจคือ 100%
            if (distance <= innerRadius)
            {
                damageMultiplier = 1f;
            }
            // 2. ถ้าหลุดวงในมาแล้ว แต่อยู่ไม่เกินวงนอก
            else
            {
                // หาระยะที่เกินมาจากวงใน
                float falloffDistance = distance - innerRadius; 
                // หาระยะทางทั้งหมดของช่วงที่ดาเมจลดลง
                float maxFalloffRange = outerRadius - innerRadius; 
                
                // แปลงเป็นเปอร์เซ็นต์ว่าเดินมาไกลแค่ไหนในโซนลดดาเมจ (0 ถึง 1)
                float t = Mathf.Clamp01(falloffDistance / maxFalloffRange);
                
                // เกลี่ยตัวเลขจาก 100% ไปหา 20%
                damageMultiplier = Mathf.Lerp(1f, minDamagePercent, t);
            }

            float actualDamage = maxDamage * damageMultiplier;

            // --- ทำดาเมจศัตรู ---
            if (hit.CompareTag("Enemy"))
            {
                Enemy enemyScript = hit.GetComponent<Enemy>();
                if (enemyScript != null)
                {
                    enemyScript.TakeDamage(actualDamage);
                }
            }
            
            // --- ทำดาเมจผู้เล่น ---
            if (hit.CompareTag("Player"))
            {
                PlayerHealth playerHealthScript = hit.GetComponent<PlayerHealth>();
                if (playerHealthScript != null)
                {
                    playerHealthScript.TakeDamage(actualDamage);
                }
            }

            // --- ส่งแรงกระแทกใส่กล่อง ---
            if (hit.CompareTag("BreakBox"))
            {
                Rigidbody boxRB = hit.GetComponent<Rigidbody>();
                
                if (boxRB != null)
                {
                    // เปลี่ยนเป้าหมายแรงระเบิดให้ใช้วงกว้างสุด (outerRadius)
                    boxRB.AddExplosionForce(boxExplosionForce, transform.position, outerRadius, BoxUpwardForce, ForceMode.Impulse);
                    Destroy(hit.gameObject, BoxLifeAfterHit); 
                }
                else
                {
                    Destroy(hit.gameObject); 
                }
            }
        }

        Destroy(gameObject);
    }

    // วาดเส้น 2 ชั้นใน Scene เพื่อให้ง่ายต่อการปรับค่า
    void OnDrawGizmosSelected()
    {
        // วงใน (Sweet Spot) สีแดง
        Gizmos.color = new Color(1f, 0f, 0f, 0.8f);
        Gizmos.DrawWireSphere(transform.position, innerRadius);

        // วงนอกสุด (Falloff Limit) สีเหลือง
        Gizmos.color = new Color(1f, 1f, 0f, 0.5f);
        Gizmos.DrawWireSphere(transform.position, outerRadius);
    }
}