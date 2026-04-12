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

    [Header("ระบบเช็คกำแพงบัง")]
    public LayerMask wallLayer; // 🌟 ติ๊กเลือก Layer กำแพงใน Inspector

    [Header("ระบบแรงกระแทก (Physics)")]
    public float boxExplosionForce = 20f;  
    public float BoxUpwardForce = 1f;      
    public float BoxLifeAfterHit = 2f;    

    [Header("เอฟเฟกต์")]
    public GameObject explosionFXPrefab;

    public AudioSource explosionSound; // เสียงตอนระเบิด (ลาก AudioSource มาใส่ตรงนี้)

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
        if (explosionSound != null)
        {
            AudioSource.PlayClipAtPoint(explosionSound.clip, transform.position);
        }

        if (explosionFXPrefab != null)
        {
            Instantiate(explosionFXPrefab, transform.position, Quaternion.identity);
        }

        Collider[] colliders = Physics.OverlapSphere(transform.position, outerRadius);
        
        foreach (Collider hit in colliders)
        {
            // 🌟 1. เช็คกำแพงบังก่อนเลย (ถ้ามีกำแพงกั้น ให้ข้าม Object นี้ไปเลย)
            if (!HasClearLineOfExplosion(transform.position, hit))
            {
                continue; // ข้ามไปเช็คชิ้นถัดไป
            }

            // ---------------------------------------------------------
            // ระบบคำนวณดาเมจแบบมี Sweet Spot (คงเดิม)
            // ---------------------------------------------------------
            float distance = Vector3.Distance(transform.position, hit.transform.position);
            float damageMultiplier = 1f;

            if (distance <= innerRadius)
            {
                damageMultiplier = 1f;
            }
            else
            {
                float falloffDistance = distance - innerRadius; 
                float maxFalloffRange = outerRadius - innerRadius; 
                float t = Mathf.Clamp01(falloffDistance / maxFalloffRange);
                damageMultiplier = Mathf.Lerp(1f, minDamagePercent, t);
            }

            float actualDamage = maxDamage * damageMultiplier;

            // --- ทำดาเมจศัตรู ---
            if (hit.CompareTag("Enemy"))
            {
                Enemy enemyScript = hit.GetComponent<Enemy>();
                if (enemyScript != null) enemyScript.TakeDamage(actualDamage);
            }
            
            // --- ทำดาเมจผู้เล่น ---
            if (hit.CompareTag("Player"))
            {
                PlayerHealth playerHealthScript = hit.GetComponent<PlayerHealth>();
                if (playerHealthScript != null) playerHealthScript.TakeDamage(actualDamage);
            }

            // --- ส่งแรงกระแทกใส่กล่อง ---
            if (hit.CompareTag("BreakBox"))
            {
                Rigidbody boxRB = hit.GetComponent<Rigidbody>();
                if (boxRB != null)
                {
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

    // 🌟 ฟังก์ชันเช็คว่าแรงระเบิดส่งไปถึงไหม โดยไม่ติดกำแพง
    bool HasClearLineOfExplosion(Vector3 explosionPos, Collider targetCollider)
    {
        // เช็คจากจุดระเบิดไปที่จุดศูนย์กลางของเป้าหมาย
        Vector3 targetPos = targetCollider.bounds.center;
        float distance = Vector3.Distance(explosionPos, targetPos);
        Vector3 direction = (targetPos - explosionPos).normalized;

        if (Physics.Raycast(explosionPos, direction, out RaycastHit hit, distance, wallLayer))
        {
            // ถ้าชนอะไรบางอย่างที่อยู่ใน wallLayer ก่อนถึงเป้าหมาย = โดนบัง
            return false;
        }
        return true;
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = new Color(1f, 0f, 0f, 0.8f);
        Gizmos.DrawWireSphere(transform.position, innerRadius);

        Gizmos.color = new Color(1f, 1f, 0f, 0.5f);
        Gizmos.DrawWireSphere(transform.position, outerRadius);
    }
}