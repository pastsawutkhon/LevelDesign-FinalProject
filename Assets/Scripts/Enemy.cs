using UnityEngine;

public class Enemy : MonoBehaviour
{
    [Header("สถานะศัตรู")]
    public float maxHealth = 3f;
    public int killScore = 100; // คะแนนที่ได้เมื่อฆ่าศัตรูตัวนี้
    private float currentHealth;

    [Header("เอฟเฟกต์และของดรอป")]
    public GameObject deathFXPrefab;
    public GameObject itemDropPrefab;
    public Transform dropPoint;

    [Header("Damage Popup")]
    public GameObject damagePopupPrefab; // ลาก Prefab เลขดาเมจมาใส่ตรงนี้

    void Start()
    {
        currentHealth = maxHealth;
    }

    public void TakeDamage(float damageAmount)
    {
        currentHealth -= damageAmount;
        
        // สร้างเลขดาเมจลอยขึ้น
        if (damagePopupPrefab != null)
        {
            GameObject popup = Instantiate(damagePopupPrefab, transform.position + Vector3.up * 2f, Quaternion.identity);
            popup.GetComponent<DamagePopup>().Setup(damageAmount);
        }

        if (currentHealth <= 0) Die();
    }

    void Die()
    {
        if (deathFXPrefab != null)
        {
            Instantiate(deathFXPrefab, transform.position + Vector3.up, Quaternion.identity);
        }

        if (itemDropPrefab != null)
        {
            Vector3 spawnPos = dropPoint != null ? dropPoint.position : transform.position + Vector3.up * 0.5f;
            Instantiate(itemDropPrefab, spawnPos, Quaternion.identity);
        }

        if (TimeManager.instance != null)
        {
            TimeManager.instance.AddEnemyScore(killScore);
        }
        Destroy(gameObject);
    }
}