using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class PlayerHealth : MonoBehaviour
{
    [Header("สถานะผู้เล่น")]
    public float maxHealth = 20f;
    public float currentHealth;
    public int lives = 3; 

    [Header("ระบบ Auto-Regen")]
    public float regenDelay = 5f;    // 🌟 (หายไป) เวลาที่ต้องรอก่อนรีเลือด
    public float regenRate = 2f;     // 🌟 (หายไป) อัตราการเพิ่มเลือดต่อวินาที
    private float lastDamageTime;    // 🌟 (หายไป) เก็บเวลาล่าสุดที่โดนดาเมจ

    [Header("เอฟเฟกต์การเกิดใหม่")]
    public GameObject respawnFX;      
    public float fxDestroyTime = 2f;  
    public GameObject playerModelObject; 
    public float respawnDelay = 1.5f; 

    [Header("UI และอื่นๆ")]
    public Image healthBarFill; 
    public float healthBarLerpSpeed = 10f; // 🌟 (หายไป) ความเร็วการลดของเลือด
    private float targetFillAmount = 1f;   // 🌟 (หายไป) ค่าเป้าหมายของหลอดเลือด
    public GameObject[] heartIcons;
    public GameObject damagePopupPrefab;

    private Vector3 initialStartPosition; 
    private ControlPlayer controlPlayer;
    private Rigidbody rb;
    private bool isInvulnerable = false;

    void Awake()
    {
        initialStartPosition = transform.position;
    }

    void Start()
    {
        controlPlayer = GetComponent<ControlPlayer>();
        rb = GetComponent<Rigidbody>();
        currentHealth = maxHealth;
        UpdateLivesUI();
    }

    void Update()
    {
        // 🌟 1. ระบบหลอดเลือดนุ่มนวล (หายไป)
        if (healthBarFill != null)
            healthBarFill.fillAmount = Mathf.Lerp(healthBarFill.fillAmount, targetFillAmount, Time.deltaTime * healthBarLerpSpeed);

        // 🌟 2. ระบบ Auto-Regen (หายไป)
        if (currentHealth < maxHealth && Time.time >= lastDamageTime + regenDelay)
        {
            currentHealth = Mathf.Min(currentHealth + regenRate * Time.deltaTime, maxHealth);
            UpdateHealthUI();
        }
    }

    public void TakeDamage(float damageAmount)
    {
        if (isInvulnerable || lives <= 0) return;

        currentHealth -= damageAmount;
        lastDamageTime = Time.time; // 🌟 (หายไป) บันทึกเวลาเพื่อเริ่มนับ Delay ใหม่
        UpdateHealthUI();

        if (damagePopupPrefab != null)
        {
            GameObject popup = Instantiate(damagePopupPrefab, transform.position + Vector3.up * 2f, Quaternion.identity);
            popup.GetComponent<DamagePopup>().Setup(damageAmount);
        }

        if (currentHealth <= 0) Die();
    }

    void Die()
    {
        lives--;
        UpdateLivesUI();

        if (lives > 0)
        {
            StartCoroutine(OriginalRespawnRoutine());
        }
        else
        {
            GameOver(); 
        }
    }

    IEnumerator OriginalRespawnRoutine()
    {
        isInvulnerable = true;

        if (controlPlayer != null) controlPlayer.canMove = false;
        
        if (rb != null)
        {
            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
        }

        transform.position = initialStartPosition;
        Physics.SyncTransforms();

        currentHealth = maxHealth;
        UpdateHealthUI();

        if (respawnFX != null) 
        {
            GameObject fx = Instantiate(respawnFX, initialStartPosition, Quaternion.identity);
            Destroy(fx, fxDestroyTime); 
        }

        float elapsed = 0;
        while (elapsed < respawnDelay)
        {
            if (playerModelObject != null) 
                playerModelObject.SetActive(!playerModelObject.activeSelf);
            
            // ใช้ 0.15f เพื่อให้จังหวะกระพริบสวยงามขึ้น
            yield return new WaitForSecondsRealtime(0.15f); 
            elapsed += 0.15f;
        }

        if (playerModelObject != null) playerModelObject.SetActive(true);
        if (controlPlayer != null) controlPlayer.canMove = true;
        isInvulnerable = false;
    }

    // 🌟 แก้ไข UpdateHealthUI ให้รองรับระบบ Lerp
    void UpdateHealthUI() { if (healthBarFill != null) targetFillAmount = currentHealth / maxHealth; }
    
    void UpdateLivesUI() { for (int i = 0; i < heartIcons.Length; i++) if(heartIcons[i] != null) heartIcons[i].SetActive(i < lives); }
    
    void GameOver() { if (GameFlowManager.instance != null) GameFlowManager.instance.EndGame(false); }
}