using UnityEngine;

public class EnemyBullet : MonoBehaviour
{
    [Header("การตั้งค่ากระสุน")]
    public float speed = 20f;
    public float lifeTime = 3f;
    public float damage = 1f;

    [Header("เอฟเฟกต์")]
    public GameObject hitFXPrefab;

    private Rigidbody rb;
    private bool isHit = false; // 🌟 ตัวล็อกป้องกันดาเมจซ้ำ

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        rb.linearVelocity = transform.forward * speed;
        Destroy(gameObject, lifeTime);
    }

    void OnCollisionEnter(Collision collision)
    {
        // 🌟 ถ้ากระสุนนัดนี้เคยทำดาเมจไปแล้ว ให้หยุดการทำงานทันที
        if (isHit) return;

        if (collision.gameObject.CompareTag("Player"))
        {
            isHit = true; // 🌟 ล็อกทันทีที่สัมผัสครั้งแรก

            PlayerHealth healthScript = collision.gameObject.GetComponent<PlayerHealth>();
            if (healthScript != null)
            {
                healthScript.TakeDamage(damage);
            }

            SpawnHitEffect(collision);
            Destroy(gameObject); // ทำลายทิ้ง
        }
        else if (collision.gameObject.CompareTag("Enemy"))
        {
            // ปล่อยให้ทะลุพวกเดียวกันไป ไม่ต้องเซ็ต isHit
        }
        else
        {
            isHit = true; // ชนกำแพง/พื้น ก็ให้ล็อกไว้กันเอฟเฟกต์เด้งซ้ำ
            SpawnHitEffect(collision);
            Destroy(gameObject);
        }
    }

    void SpawnHitEffect(Collision collision)
    {
        if (hitFXPrefab != null)
        {
            ContactPoint contact = collision.contacts[0];
            Instantiate(hitFXPrefab, contact.point, Quaternion.LookRotation(contact.normal));
        }
    }
}