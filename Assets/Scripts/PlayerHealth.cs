using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class PlayerHealth : MonoBehaviour
{
    [Header("สถานะผู้เล่น")]
    public float maxHealth = 10f;
    public float currentHealth;
    public int lives = 3; 

    [Header("UI หลอดเลือด")]
    public Image healthBarFill; 
    public float healthBarLerpSpeed = 10f; // ตัวปรับความสมูธ (ยิ่งเยอะยิ่งไหลเร็ว)
    private float targetFillAmount = 1f;   // ตัวแปรเก็บเป้าหมายว่าเลือดควรไปหยุดที่ตรงไหน

    [Header("ระบบฟื้นฟูเลือด (Regen)")]
    public float regenDelay = 5f; 
    public float regenRate = 2f;  
    private float lastDamageTime;

    [Header("ระบบเกิดใหม่ (Respawn)")]
    public Transform respawnPoint; 
    public GameObject respawnFXPrefab; 
    public float respawnDelay = 1.5f; 
    public float flashDuration = 2f; 

    private ControlPlayer controlPlayer; 
    private bool isRespawning = false;
    private Renderer[] allRenderers; 

    void Start()
    {
        Time.timeScale = 1f; 
        currentHealth = maxHealth;
        controlPlayer = GetComponent<ControlPlayer>();
        allRenderers = GetComponentsInChildren<Renderer>();
        
        if (respawnPoint == null)
        {
            GameObject defaultSpawn = new GameObject("Default_SpawnPoint");
            defaultSpawn.transform.position = transform.position;
            respawnPoint = defaultSpawn.transform;
        }

        // เซ็ตให้หลอดเลือดและเป้าหมายเต็ม 100% ตอนเริ่มเกมทันที จะได้ไม่เห็นมันวิ่งขึ้นตอนเริ่ม
        targetFillAmount = 1f;
        if (healthBarFill != null) healthBarFill.fillAmount = 1f;
    }

    void Update()
    {
        // --------------------------------------------------------
        // 1. ระบบสมูธ UI (ให้มันทำงานทุกเฟรม ไม่ว่าจะตายหรือเกิดอยู่)
        // --------------------------------------------------------
        if (healthBarFill != null)
        {
            // ใช้ Mathf.Lerp เพื่อค่อยๆ ปรับค่า fillAmount ปัจจุบัน ให้ไหลไปหาเป้าหมาย (targetFillAmount)
            healthBarFill.fillAmount = Mathf.Lerp(healthBarFill.fillAmount, targetFillAmount, Time.deltaTime * healthBarLerpSpeed);
        }

        // ถ้าตายหรือเกิดใหม่ ข้ามระบบด้านล่างไปเลย
        if (isRespawning || lives <= 0) return;

        // --------------------------------------------------------
        // 2. ระบบรีเจนเลือด
        // --------------------------------------------------------
        if (currentHealth < maxHealth && Time.time >= lastDamageTime + regenDelay)
        {
            currentHealth += regenRate * Time.deltaTime; 
            if (currentHealth > maxHealth) currentHealth = maxHealth; 
            
            // อัปเดตเป้าหมายของ UI ตอนเลือดกำลังเด้ง
            UpdateHealthUI();
        }
    }

    public void TakeDamage(float damageAmount)
    {
        if (isRespawning || lives <= 0) return; 

        currentHealth -= damageAmount;
        lastDamageTime = Time.time; 
        
        Debug.Log("ผู้เล่นโดนดาเมจ! เลือดเหลือ: " + currentHealth);

        // อัปเดตเป้าหมายของ UI ตอนโดนตี
        UpdateHealthUI();

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    void UpdateHealthUI()
    {
        if (healthBarFill != null)
        {
            // เปลี่ยนจากการสั่งให้หดทันที เป็นการ "ตั้งเป้าหมาย" แทน
            targetFillAmount = currentHealth / maxHealth;
        }
    }

    void Die()
    {
        lives--; 
        Debug.Log("ผู้เล่นตาย! หัวใจเหลือ: " + lives);

        if (lives > 0)
        {
            StartCoroutine(RespawnRoutine());
        }
        else
        {
            GameOver();
        }
    }

    IEnumerator RespawnRoutine()
    {
        isRespawning = true;
        if (controlPlayer != null) controlPlayer.enabled = false;

        transform.position = respawnPoint.position;
        currentHealth = maxHealth; 
        lastDamageTime = Time.time; 
        
        // เซ็ตให้หลอดเลือดกลับมาเต็ม 100%
        UpdateHealthUI();

        if (respawnFXPrefab != null)
        {
            GameObject fx = Instantiate(respawnFXPrefab, transform.position, Quaternion.identity);
            
            Destroy(fx, 2f); 
        }

        StartCoroutine(FlashRoutine());

        yield return new WaitForSeconds(respawnDelay);

        if (controlPlayer != null) controlPlayer.enabled = true;
        isRespawning = false;
    }

    IEnumerator FlashRoutine()
    {
        float elapsed = 0f;
        bool isVisible = true;

        while (elapsed < flashDuration)
        {
            isVisible = !isVisible;
            SetRenderersVisibility(isVisible);
            yield return new WaitForSeconds(0.1f); 
            elapsed += 0.1f;
        }

        SetRenderersVisibility(true);
    }

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
        Debug.Log("Game Over!");
        if (controlPlayer != null) controlPlayer.enabled = false;
        Time.timeScale = 0f;
    }
}