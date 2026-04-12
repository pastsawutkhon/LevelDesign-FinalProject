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

    [Header("UI หัวใจ (Lives)")]
    public GameObject[] heartIcons;

    [Header("Damage Popup")]
    public GameObject damagePopupPrefab; // ลาก Prefab เลขดาเมจมาใส่ตรงนี้

    [Header("ระบบเกิดใหม่/Regen")]
    public float regenDelay = 5f; 
    public float regenRate = 2f;  
    public Transform respawnPoint;
    private ControlPlayer controlPlayer; // อ้างอิงสคริปต์ควบคุมผู้เล่น
    private float lastDamageTime;
    private bool isRespawning = false;

    void Start()
    {
        controlPlayer = GetComponent<ControlPlayer>();
        currentHealth = maxHealth;
        UpdateLivesUI();
    }

    void Update()
    {
        if (healthBarFill != null)
            healthBarFill.fillAmount = Mathf.Lerp(healthBarFill.fillAmount, targetFillAmount, Time.deltaTime * healthBarLerpSpeed);

        if (!isRespawning && currentHealth < maxHealth && Time.time >= lastDamageTime + regenDelay)
        {
            currentHealth = Mathf.Min(currentHealth + regenRate * Time.deltaTime, maxHealth);
            UpdateHealthUI();
        }
    }

    public void TakeDamage(float damageAmount)
    {
        if (isRespawning || lives <= 0) return;

        currentHealth -= damageAmount;
        lastDamageTime = Time.time;
        UpdateHealthUI();

        // สร้างเลขดาเมจลอยขึ้นที่ตัวผู้เล่น
        if (damagePopupPrefab != null)
        {
            GameObject popup = Instantiate(damagePopupPrefab, transform.position + Vector3.up * 2f, Quaternion.identity);
            popup.GetComponent<DamagePopup>().Setup(damageAmount);
        }

        if (currentHealth <= 0) Die();
    }

    void UpdateHealthUI() { if (healthBarFill != null) targetFillAmount = currentHealth / maxHealth; }
    
    void UpdateLivesUI()
    {
        for (int i = 0; i < heartIcons.Length; i++)
            heartIcons[i].SetActive(i < lives);
    }

    void Die()
    {
        lives--;
        UpdateLivesUI();
        if (lives > 0) StartCoroutine(RespawnRoutine());
        else Debug.Log("Game Over");
    }

    IEnumerator RespawnRoutine()
    {
        isRespawning = true;
        transform.position = respawnPoint.position;
        currentHealth = maxHealth;
        UpdateHealthUI();
        yield return new WaitForSeconds(1f);
        isRespawning = false;
    }
    void GameOver()
{
    Debug.Log("Game Over!");
    if (controlPlayer != null) controlPlayer.enabled = false;
    
    // 🌟 สั่งให้ GameFlowManager แสดงหน้าจอจบเกมและคะแนน
    if (GameFlowManager.instance != null)
    {
        GameFlowManager.instance.ShowGameOver();
    }
    else
    {
        // กรณีลืมวาง GameFlowManager ใน Scene ให้หยุดเวลาไว้ก่อนกันพลาด
        Time.timeScale = 0f;
    }
}
}