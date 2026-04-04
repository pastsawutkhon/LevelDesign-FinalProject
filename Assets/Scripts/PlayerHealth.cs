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
    public float healthBarLerpSpeed = 10f; 
    private float targetFillAmount = 1f;   

    // ---------------------------------------------------------
    // 🌟 ส่วนที่เพิ่มใหม่: UI หัวใจ
    // ---------------------------------------------------------
    [Header("UI หัวใจ (Lives)")]
    public GameObject[] heartIcons; // อาร์เรย์สำหรับเก็บรูปหัวใจ 3 ดวง

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

        targetFillAmount = 1f;
        if (healthBarFill != null) healthBarFill.fillAmount = 1f;

        // อัปเดตหัวใจให้แสดงครบ 3 ดวงตอนเริ่มเกม
        UpdateLivesUI(); 
    }

    void Update()
    {
        if (healthBarFill != null)
        {
            healthBarFill.fillAmount = Mathf.Lerp(healthBarFill.fillAmount, targetFillAmount, Time.deltaTime * healthBarLerpSpeed);
        }

        if (isRespawning || lives <= 0) return;

        if (currentHealth < maxHealth && Time.time >= lastDamageTime + regenDelay)
        {
            currentHealth += regenRate * Time.deltaTime; 
            if (currentHealth > maxHealth) currentHealth = maxHealth; 
            
            UpdateHealthUI();
        }
    }

    public void TakeDamage(float damageAmount)
    {
        if (isRespawning || lives <= 0) return; 

        currentHealth -= damageAmount;
        lastDamageTime = Time.time; 
        
        Debug.Log("ผู้เล่นโดนดาเมจ! เลือดเหลือ: " + currentHealth);
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
            targetFillAmount = currentHealth / maxHealth;
        }
    }

    // ---------------------------------------------------------
    // 🌟 ฟังก์ชันอัปเดตการแสดงผลหัวใจ
    // ---------------------------------------------------------
    void UpdateLivesUI()
    {
        // เช็คก่อนว่ามีการลากรูปหัวใจมาใส่ใน Inspector หรือยัง
        if (heartIcons == null || heartIcons.Length == 0) return;

        // วนลูปเช็คหัวใจแต่ละดวง
        for (int i = 0; i < heartIcons.Length; i++)
        {
            // ถ้าลำดับของหัวใจ น้อยกว่า จำนวนชีวิตที่เหลืออยู่ ให้แสดงหัวใจ (เปิด Object)
            if (i < lives)
            {
                heartIcons[i].SetActive(true);
            }
            // ถ้าชีวิตลดลงไปแล้ว ให้ซ่อนหัวใจดวงนั้น (ปิด Object)
            else
            {
                heartIcons[i].SetActive(false);
            }
        }
    }

    void Die()
    {
        lives--; // ลดชีวิตลง 1
        Debug.Log("ผู้เล่นตาย! หัวใจเหลือ: " + lives);

        // เรียกใช้อัปเดตหัวใจบนหน้าจอทันทีที่ตาย
        UpdateLivesUI();

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
        Rigidbody rb = GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
        }

        transform.position = respawnPoint.position;
        currentHealth = maxHealth; 
        lastDamageTime = Time.time; 
        
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