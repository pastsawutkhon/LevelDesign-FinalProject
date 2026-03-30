using UnityEngine;

public class Enemy : MonoBehaviour
{
    [Header("สถานะศัตรู")]
    public float maxHealth = 3f; // เลือดสูงสุด
    private float currentHealth;

    [Header("เอฟเฟกต์และของดรอป")]
    public GameObject deathFXPrefab;   // เอฟเฟกต์ตอนตาย
    public GameObject itemDropPrefab;  // ไอเทมที่จะดรอป
    
    [Tooltip("จุดที่จะให้ไอเทมดรอป (ถ้าปล่อยว่าง จะดรอปที่ตัวศัตรู)")]
    public Transform dropPoint;        // ลาก GameObject เปล่าๆ มาใส่เพื่อกำหนดจุดดรอป

    void Start()
    {
        // เริ่มเกมมาให้เลือดเต็ม
        currentHealth = maxHealth;
    }

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
        // 1. เล่นเอฟเฟกต์ตาย
        if (deathFXPrefab != null)
        {
            // ให้เอฟเฟกต์เกิดตรงจุดกึ่งกลางของตัวศัตรู (ปรับให้สูงขึ้นมานิดนึง)
            Vector3 fxPos = transform.position + new Vector3(0, 1f, 0);
            Instantiate(deathFXPrefab, fxPos, Quaternion.identity);
        }

        // 2. ดรอปไอเทม
        if (itemDropPrefab != null)
        {
            // ตรวจสอบว่ามีการใส่ Drop Point ไว้หรือไม่
            Vector3 spawnPosition;
            if (dropPoint != null)
            {
                // ถ้าใส่ไว้ ให้ใช้ตำแหน่งของ Drop Point นั้นเลย
                spawnPosition = dropPoint.position;
            }
            else
            {
                // ถ้าไม่ได้ใส่ ให้ใช้ตำแหน่งตัวศัตรู แล้วยกสูงขึ้น 0.5 หน่วยกันจมดิน
                spawnPosition = transform.position + new Vector3(0, 0.5f, 0);
            }

            Instantiate(itemDropPrefab, spawnPosition, Quaternion.identity);
        }

        // 3. ทำลายโมเดลศัตรูทิ้ง
        Destroy(gameObject);
    }
}