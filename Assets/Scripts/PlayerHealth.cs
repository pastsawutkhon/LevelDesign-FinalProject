using UnityEngine;
using System.Collections;

public class PlayerHealth : MonoBehaviour
{
    [Header("สถานะผู้เล่น")]
    public float maxHealth = 10f;
    public float currentHealth;
    public int lives = 3; // หัวใจ 3 ดวง

    [Header("ระบบฟื้นฟูเลือด (Regen)")]
    public float regenDelay = 5f; // เวลาที่ต้องรอก่อนเลือดจะเริ่มเด้ง
    public float regenRate = 2f;  // ความเร็วในการเด้ง (หน่วยต่อวินาที)
    private float lastDamageTime;

    [Header("ระบบเกิดใหม่ (Respawn)")]
    public Transform respawnPoint; // ลาก Object จุดเกิดมาใส่
    public GameObject respawnFXPrefab; // เอฟเฟกต์ตอนเกิด
    public float respawnDelay = 1.5f; // เวลาที่ขยับไม่ได้หลังเกิด (วินาที)
    public float flashDuration = 2f; // เวลากระพริบ (อมตะชั่วคราว)

    private ControlPlayer controlPlayer; // เอาไว้ปิดการควบคุมตอนรอเกิด
    private bool isRespawning = false;
    private Renderer[] allRenderers; // เก็บโมเดลทั้งหมดเพื่อสั่งกระพริบ

    void Start()
    {
        // เผื่อเริ่มฉากใหม่แล้วเวลาค้าง ให้รีเซ็ตเวลาเดินปกติ
        Time.timeScale = 1f; 

        currentHealth = maxHealth;
        controlPlayer = GetComponent<ControlPlayer>();
        
        // ดึง MeshRenderer ทั้งหมดในตัว Player (รวมถึงอาวุธ) เพื่อใช้ทำกระพริบ
        allRenderers = GetComponentsInChildren<Renderer>();
        
        // ถ้าลืมใส่จุดเกิด ให้เอาตำแหน่งที่ยืนอยู่ตอนเริ่มเกมเป็นจุดเกิดเลย
        if (respawnPoint == null)
        {
            GameObject defaultSpawn = new GameObject("Default_SpawnPoint");
            defaultSpawn.transform.position = transform.position;
            respawnPoint = defaultSpawn.transform;
            Debug.LogWarning("คุณไม่ได้ใส่ Respawn Point ระบบเลยสร้างจุดเกิดให้ตรงตำแหน่งเริ่มต้น");
        }
    }

    void Update()
    {
        // ถ้ากำลังเกิดใหม่ หรือตายสนิทแล้ว ไม่ต้องทำอะไร
        if (isRespawning || lives <= 0) return;

        // --- ระบบรีเจนเลือด ---
        // ถ้าเลือดไม่เต็ม และเวลาผ่านไปเกิน Delay ที่ตั้งไว้หลังจากโดนตีครั้งล่าสุด
        if (currentHealth < maxHealth && Time.time >= lastDamageTime + regenDelay)
        {
            currentHealth += regenRate * Time.deltaTime; // เลือดค่อยๆ เพิ่ม
            if (currentHealth > maxHealth) currentHealth = maxHealth; // ไม่ให้เลือดทะลุหลอด
        }
    }

    public void TakeDamage(float damageAmount)
    {
        // ถ้ากำลังเกิดใหม่ (อมตะกระพริบอยู่) หรือตายแล้ว จะไม่รับดาเมจ
        if (isRespawning || lives <= 0) return; 

        currentHealth -= damageAmount;
        lastDamageTime = Time.time; // อัปเดตเวลาโดนตี เพื่อรีเซ็ตเวลารีเจนเลือด
        
        Debug.Log("ผู้เล่นโดนดาเมจ! เลือดเหลือ: " + currentHealth);

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    void Die()
    {
        lives--; // ลดหัวใจ 1 ดวง
        Debug.Log("ผู้เล่นตาย! หัวใจเหลือ: " + lives);

        if (lives > 0)
        {
            // ถ้าหัวใจยังเหลือ ให้เริ่มกระบวนการเกิดใหม่
            StartCoroutine(RespawnRoutine());
        }
        else
        {
            // ถ้าหัวใจหมด (0)
            GameOver();
        }
    }

    // กระบวนการเกิดใหม่ (หน่วงเวลา)
    IEnumerator RespawnRoutine()
    {
        isRespawning = true;

        // 1. ปิดสคริปต์ควบคุม (ทำให้เดิน/ยิง/สลับอาวุธไม่ได้)
        if (controlPlayer != null) controlPlayer.enabled = false;

        // 2. ย้ายตัวละครกลับไปจุดเกิด และรีเซ็ตเลือด
        transform.position = respawnPoint.position;
        currentHealth = maxHealth; 
        lastDamageTime = Time.time; // รีเซ็ตเวลาดาเมจด้วย

        // 3. เล่นเอฟเฟกต์ตอนเกิด
        if (respawnFXPrefab != null)
        {
            Instantiate(respawnFXPrefab, transform.position, Quaternion.identity);
        }

        // 4. สั่งให้โมเดลกระพริบ
        StartCoroutine(FlashRoutine());

        // 5. รอเวลาดีเลย์ (ห้ามเดิน)
        yield return new WaitForSeconds(respawnDelay);

        // 6. หมดดีเลย์ เปิดให้กลับมาบังคับได้ปกติ
        if (controlPlayer != null) controlPlayer.enabled = true;
        isRespawning = false;
    }

    // กระบวนการกระพริบโมเดล
    IEnumerator FlashRoutine()
    {
        float elapsed = 0f;
        bool isVisible = true;

        // สลับเปิด-ปิดโมเดลไปเรื่อยๆ จนกว่าจะหมดเวลา flashDuration
        while (elapsed < flashDuration)
        {
            isVisible = !isVisible;
            SetRenderersVisibility(isVisible);
            
            yield return new WaitForSeconds(0.1f); // ความเร็วในการกระพริบ
            elapsed += 0.1f;
        }

        // ตอนจบกระพริบ บังคับให้โมเดลเปิดโชว์ตามปกติ
        SetRenderersVisibility(true);
    }

    // ฟังก์ชันสั่งเปิด/ปิด การมองเห็นโมเดล
    void SetRenderersVisibility(bool state)
    {
        if (allRenderers != null)
        {
            foreach (Renderer r in allRenderers)
            {
                if (r != null) r.enabled = state;
            }
        }
    }

    void GameOver()
    {
        Debug.Log("Game Over! หัวใจหมดแล้ว");
        
        // ปิดการควบคุม
        if (controlPlayer != null) controlPlayer.enabled = false;
        
        // หยุดเวลาของเกมทั้งหมด (กระสุนหยุดพุ่ง, ศัตรูหยุดเดิน)
        Time.timeScale = 0f;

        // -----------------------------------------------------------------
        // TODO: เรียกหน้าต่าง UI Game Over ขึ้นมาตรงนี้ (ถ้าคุณทำ UI ไว้แล้ว)
        // -----------------------------------------------------------------
    }
}