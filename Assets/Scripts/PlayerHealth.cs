using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class PlayerHealth : MonoBehaviour
{
    [Header("สถานะผู้เล่น")]
    public float maxHealth = 20f;
    public float currentHealth;
    public int lives = 3; 

    [Header("เอฟเฟกต์การเกิดใหม่")]
    public GameObject respawnFX;      
    public float fxDestroyTime = 2f;  
    
    // 🌟 เปลี่ยนจาก MeshRenderer เป็น GameObject (ตัวโมเดลหลักที่มีลูกๆ อยู่ข้างใน)
    public GameObject playerModelObject; 
    public float respawnDelay = 1.5f; 

    [Header("UI และอื่นๆ")]
    public Image healthBarFill; 
    public GameObject[] heartIcons;
    public GameObject damagePopupPrefab;

    private Vector3 initialStartPosition; 
    private ControlPlayer controlPlayer;
    private Rigidbody rb; // เพิ่ม Rigidbody เพื่อ Reset แรง
    private bool isInvulnerable = false;

    void Awake()
    {
        initialStartPosition = transform.position;
    }

    void Start()
    {
        controlPlayer = GetComponent<ControlPlayer>();
        rb = GetComponent<Rigidbody>(); // อ้างอิง Rigidbody
        currentHealth = maxHealth;
        UpdateLivesUI();
    }

    public void TakeDamage(float damageAmount)
    {
        if (isInvulnerable || lives <= 0) return;

        currentHealth -= damageAmount;
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
        
        // 🌟 แก้บัค 1: หยุดแรงฟิสิกส์ทั้งหมดก่อนย้ายที่
        if (rb != null)
        {
            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
        }

        // ย้ายตำแหน่งกลับจุดเริ่ม
        transform.position = initialStartPosition;
        
        // 🌟 บังคับให้ระบบ Physics อัปเดตตำแหน่งทันที (แก้ปัญหาไม่วาร์ป)
        Physics.SyncTransforms();

        currentHealth = maxHealth;
        UpdateHealthUI();

        if (respawnFX != null) 
        {
            GameObject fx = Instantiate(respawnFX, initialStartPosition, Quaternion.identity);
            Destroy(fx, fxDestroyTime); 
        }

        // 🌟 แก้บัค 2: สั่งกระพริบทั้ง GameObject (เพื่อให้ Child ทั้งหมดหายไปด้วย)
        float elapsed = 0;
        while (elapsed < respawnDelay)
        {
            if (playerModelObject != null) 
                playerModelObject.SetActive(!playerModelObject.activeSelf);
            
            yield return new WaitForSeconds(0.1f);
            elapsed += 0.1f;
        }

        // กลับมาสถานะปกติ
        if (playerModelObject != null) playerModelObject.SetActive(true);
        if (controlPlayer != null) controlPlayer.canMove = true;
        isInvulnerable = false;
    }

    void UpdateHealthUI() { if (healthBarFill != null) healthBarFill.fillAmount = currentHealth / maxHealth; }
    void UpdateLivesUI() { for (int i = 0; i < heartIcons.Length; i++) if(heartIcons[i] != null) heartIcons[i].SetActive(i < lives); }
    void GameOver() { if (GameFlowManager.instance != null) GameFlowManager.instance.EndGame(false); }
}