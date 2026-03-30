using UnityEngine;
using System.Collections;

public class Dynamite : MonoBehaviour
{
    [Header("การโยน (ปรับได้ใน Inspector)")]
    public float throwForce = 15f;
    public float throwUpwardForce = 5f;

    [Header("การระเบิด")]
    public float explosionTimer = 3f;
    public float explosionRadius = 5f;
    public float explosionDamage = 10f;
    
    [Header("ระบบแรงกระแทก (Physics)")]
    public float boxExplosionForce = 20f;  // ความแรงในการกระแทกกล่องให้กระเด็น
    public float BoxUpwardForce = 1f;      // แรงยกด้านบนตอนกระเด็น (ทำให้ลอยสูงขึ้น)
    public float BoxLifeAfterHit = 2f;    // กล่องจะกระเด็นอยู่นานกี่วิก่อนจะหายไป (Destroy)

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
            // --- ทำดาเมจศัตรู (tag Enemy) ---
            if (hit.CompareTag("Enemy"))
            {
                Enemy enemyScript = hit.GetComponent<Enemy>();
                if (enemyScript != null)
                {
                    enemyScript.TakeDamage(explosionDamage);
                }
            }
            
            // --- ทำดาเมจผู้เล่น (tag Player) ---
            if (hit.CompareTag("Player"))
            {
                PlayerHealth playerHealthScript = hit.GetComponent<PlayerHealth>();
                if (playerHealthScript != null)
                {
                    playerHealthScript.TakeDamage(explosionDamage);
                }
            }

            // ----------------------------------------------------------------------------------
            // !!! แก้ไขตรงจุดนี้เพื่อทำให้กระเด็น !!!
            // --- ส่งแรงกระแทกใส่กล่อง (tag BreakBox) ---
            // ----------------------------------------------------------------------------------
            if (hit.CompareTag("BreakBox"))
            {
                // ดึง Rigidbody ของกล่องมา (ต้องมี Rigidbody แปะอยู่ที่กล่อง!)
                Rigidbody boxRB = hit.GetComponent<Rigidbody>();
                
                if (boxRB != null)
                {
                    Debug.Log("ระเบิดโดน BreakBox ส่งแรงกระแทก: " + hit.gameObject.name);
                    
                    // สั่งให้ Rigidbody รับแรงระเบิดจากจุดศูนย์กลาง (transform.position)
                    // ด้วยความแรง boxExplosionForce และยกสูงขึ้น BoxUpwardForce
                    boxRB.AddExplosionForce(boxExplosionForce, transform.position, explosionRadius, BoxUpwardForce, ForceMode.Impulse);
                    
                    // สั่งให้ทำลายกล่องทิ้งหลังจากหน่วงเวลาไป x วินาที (เพื่อให้เห็นตอนกระเด็นก่อน)
                    Destroy(hit.gameObject, BoxLifeAfterHit); 
                }
                else
                {
                    // สำรองไว้ เผื่อลืมใส่ Rigidbody ให้กล่อง ก็ให้ทำลายทิ้งทันทีเหมือนเดิม
                    Debug.LogWarning("เจอ BreakBox แต่ไม่มี Rigidbody เลยส่งแรงกระแทกไม่ได้!");
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