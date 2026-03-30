using UnityEngine;

public class Enemy : MonoBehaviour
{
    [Header("สถานะศัตรู")]
    public float maxHealth = 3f; // เลือดสูงสุด (ลูกกระจ๊อกอาจจะ 3, บอสอาจจะ 50)
    private float currentHealth;

    [Header("เอฟเฟกต์และของดรอป")]
    public GameObject deathFXPrefab;   // ลาก Prefab เอฟเฟกต์ระเบิด/เลือด มาใส่
    public GameObject itemDropPrefab;  // ลาก Prefab ไอเทม (FloatingItem) มาใส่ ถ้าว่างไว้ก็ไม่ดรอป

    void Start()
    {
        // เริ่มเกมมาให้เลือดเต็ม
        currentHealth = maxHealth;
    }

    // ฟังก์ชันนี้จะถูกเรียกจากกระสุน หรือ มีดของผู้เล่น
    public void TakeDamage(float damageAmount)
    {
        currentHealth -= damageAmount;
        Debug.Log(gameObject.name + " โดนโจมตี! เลือดเหลือ: " + currentHealth);
        
        // ถ้าเลือดหมด ให้ตาย
        if (currentHealth <= 0)
        {
            Die();
        }
    }

    void Die()
    {
        // 1. เล่นเอฟเฟกต์ตาย (ถ้ามี)
        if (deathFXPrefab != null)
        {
            Instantiate(deathFXPrefab, transform.position, Quaternion.identity);
        }

        // 2. ดรอปไอเทม (ถ้ามี)
        if (itemDropPrefab != null)
        {
            // ให้ของดรอปลอยสูงจากพื้นนิดนึง จะได้ไม่จมดิน
            Vector3 dropPosition = transform.position + new Vector3(0, 0.5f, 0);
            Instantiate(itemDropPrefab, dropPosition, Quaternion.identity);
        }

        // 3. ทำลายโมเดลศัตรูทิ้ง
        Destroy(gameObject);
    }
}