using UnityEngine;
using System.Collections; // จำเป็นต้องใช้สำหรับ Coroutine

public class NPCTrader : MonoBehaviour
{
    [Header("การตั้งค่าซื้อขาย")]
    public KeyCode interactKey = KeyCode.E;
    public bool tradeOnlyOnce = true; 
    private bool hasTraded = false; 

    [Header("ของที่จะดรอป")]
    public GameObject itemDropPrefab; 
    public Transform dropPoint;
    public float itemDropDelay = 1.0f; // 🌟 ระยะเวลาหน่วงก่อนไอเทมจะดรอป (วินาที)

    [Header("เอฟเฟกต์")]
    public GameObject tradeFXPrefab; 
    public Transform fxPoint; // 🌟 จุดที่จะให้ Effect แสดง (ถ้าไม่ใส่จะเกิดที่ตัว NPC)

    private bool isPlayerInRange = false;
    private ControlPlayer playerScript;

    void Update()
    {
        if (isPlayerInRange && (!hasTraded || !tradeOnlyOnce) && Input.GetKeyDown(interactKey))
        {
            if (playerScript != null)
            {
                bool hasMoneyInBag = playerScript.inventory.Contains(ControlPlayer.WeaponType.Money);

                if (hasMoneyInBag && playerScript.currentWeapon == ControlPlayer.WeaponType.Money)
                {
                    TradeItem();
                }
                else if (hasMoneyInBag && playerScript.currentWeapon != ControlPlayer.WeaponType.Money)
                {
                    UIManager.instance.ShowNotification("Hold the money in your hand!");
                }
                else
                {
                    UIManager.instance.ShowNotification("You don't have any money!");
                }
            }
        }
    }

    void TradeItem()
    {
        Debug.Log("NPC: เริ่มการแลกเปลี่ยน...");
        UIManager.instance.ShowNotification("Trade successful!");
        hasTraded = true;

        // 1. สร้างเอฟเฟกต์ทันทีในตำแหน่งที่กำหนด
        if (tradeFXPrefab != null)
        {
            Vector3 fxPos = fxPoint != null ? fxPoint.position : transform.position;
            Instantiate(tradeFXPrefab, fxPos, Quaternion.identity);
        }

        // 2. ลบเงินออกจากตัวผู้เล่นทันที
        playerScript.RemoveWeapon(ControlPlayer.WeaponType.Money);

        // 3. 🌟 เริ่มกระบวนการรอเวลา (Delay) ก่อนดรอปไอเทม
        StartCoroutine(DropItemWithDelay());
    }

    // 🌟 Coroutine สำหรับรอเวลาดรอปของ
    IEnumerator DropItemWithDelay()
    {
        // รอตามเวลาที่ตั้งไว้ใน Inspector
        yield return new WaitForSeconds(itemDropDelay);

        if (itemDropPrefab != null)
        {
            Vector3 spawnPosition = dropPoint != null ? dropPoint.position : transform.position + (transform.forward * 1.5f);
            spawnPosition.y += 0.5f; 
            Instantiate(itemDropPrefab, spawnPosition, Quaternion.identity);
            
            Debug.Log("NPC: ส่งมอบไอเทมเรียบร้อย");
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            if (tradeOnlyOnce && hasTraded) return;
            isPlayerInRange = true;
            playerScript = other.GetComponent<ControlPlayer>();
            UIManager.instance.ShowNotification("Press E to Interact");
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            isPlayerInRange = false;
            playerScript = null;
        }
    }
}