using UnityEngine;

public class Bullet : MonoBehaviour
{
    [Header("การตั้งค่ากระสุน")]
    public float speed = 20f;
    public float lifeTime = 3f;
    public float damage = 1f; // ปรับดาเมจของกระสุนได้ใน Inspector

    [Header("เอฟเฟกต์")]
    public GameObject hitFXPrefab; // ลาก Prefab เอฟเฟกต์ตอนยิงโดนมาใส่ (เช่น เลือดกระเด็น, ประกายไฟ)

    public Rigidbody rb;
    public AudioClip hitSound; // เสียงตอนกระสุนโดนเป้าหมาย (ลาก AudioClip มาใส่ตรงนี้)

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        // พุ่งไปข้างหน้าตามแกนที่ตั้งไว้ (จากไฟล์เดิมของคุณใช้ transform.up)
        rb.linearVelocity = transform.up * speed;

        Destroy(gameObject, lifeTime);
    }

    void OnCollisionEnter(Collision collision)
    {
        // เช็คว่าชนกับศัตรูหรือไม่
        if (collision.gameObject.CompareTag("Enemy"))
        {
            // 1. เล่นเอฟเฟกต์ตรงจุดที่กระสุนกระทบเป้าหมายพอดี
            if (hitFXPrefab != null)
            {
                Instantiate(hitFXPrefab, collision.contacts[0].point, Quaternion.identity);
            }
            if (hitSound != null)
            {
                AudioSource.PlayClipAtPoint(hitSound, collision.contacts[0].point);
            }

            // 2. ทำดาเมจใส่ศัตรู
            Enemy enemyScript = collision.gameObject.GetComponent<Enemy>();
            if (enemyScript != null)
            {
                enemyScript.TakeDamage(damage);
            }

            // 3. ทำลายกระสุนทิ้งทันทีเมื่อยิงโดน
            Destroy(gameObject);
        }
        else 
        {
            // ถ้าชนอย่างอื่นที่ไม่ใช่ศัตรู (เช่น กำแพง) ให้กระสุนหยุดและตกลงพื้น
            rb.linearVelocity = Vector3.zero; 
            rb.useGravity = true; 
        }
    }
}