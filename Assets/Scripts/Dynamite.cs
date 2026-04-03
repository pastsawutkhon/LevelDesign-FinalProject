using UnityEngine;
using System.Collections;

public class Dynamite : MonoBehaviour
{
    [Header("การโยน (ปรับได้ใน Inspector)")]
    public float throwForce = 15f;
    public float throwUpwardForce = 5f;

    [Header("การระเบิด")]
    public float explosionTimer = 3f;
    public float explosionRadius = 5f;    // รัศมี 5 เมตร
    public float explosionDamage = 10f;   // ดาเมจสูงสุด (ตรงกลาง)
    
    [Tooltip("เปอร์เซ็นต์ดาเมจที่ขอบระเบิด (0.2 = 20%)")]
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

        Collider[] colliders = Physics.OverlapSphere(transform.position, explosionRadius);
        foreach (Collider hit in colliders)
        {
            // ---------------------------------------------------------
            // ระบบคำนวณดาเมจตามระยะทาง (100% -> 20%)
            // ---------------------------------------------------------
            float distance = Vector3.Distance(transform.position, hit.transform.position);
            
            // หาว่าเป้าหมายอยู่ห่างออกไปกี่เปอร์เซ็นต์ของรัศมี (0 = ตรงกลาง, 1 = ขอบ 5 เมตร)
            float distancePercent = Mathf.Clamp01(distance / explosionRadius);
            
            // ใช้ Lerp ไล่ระดับตัวคูณดาเมจ จาก 1f (100%) ไปจนถึง 0.2f (20%)
            float damageMultiplier = Mathf.Lerp(1f, minDamagePercent, distancePercent);
            
            // ดาเมจสุทธิที่จะทำใส่เป้าหมาย
            float actualDamage = explosionDamage * damageMultiplier;

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

            // --- ส่งแรงกระแทกใส่กล่อง (ขอบเขตระเบิดโดนปุ๊บกระเด็นพังเลยเหมือนเดิม) ---
            if (hit.CompareTag("BreakBox"))
            {
                Rigidbody boxRB = hit.GetComponent<Rigidbody>();
                
                if (boxRB != null)
                {
                    boxRB.AddExplosionForce(boxExplosionForce, transform.position, explosionRadius, BoxUpwardForce, ForceMode.Impulse);
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

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, explosionRadius);
    }
}