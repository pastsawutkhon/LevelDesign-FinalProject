using UnityEngine;

public class NPCTrader : MonoBehaviour
{
    [Header("การตั้งค่าซื้อขาย")]
    public KeyCode interactKey = KeyCode.E;
    public bool tradeOnlyOnce = true; 
    private bool hasTraded = false; 

    [Header("ของที่จะดรอป")]
    public GameObject itemDropPrefab; 
    public Transform dropPoint;

    [Header("เอฟเฟกต์")]
    public GameObject tradeFXPrefab; 

    private bool isPlayerInRange = false;
    private ControlPlayer playerScript;

    void Update()
    {
        if (isPlayerInRange && (!hasTraded || !tradeOnlyOnce) && Input.GetKeyDown(interactKey))
        {
            if (playerScript != null)
            {
                // ตรวจสอบว่าในกระเป๋ามี Money หรือไม่
                bool hasMoneyInBag = playerScript.inventory.Contains(ControlPlayer.WeaponType.Money);

                // เช็คว่ามีเงิน และถือเงินไว้ในมือ
                if (hasMoneyInBag && playerScript.currentWeapon == ControlPlayer.WeaponType.Money)
                {
                    TradeItem();
                }
                else if (hasMoneyInBag && playerScript.currentWeapon != ControlPlayer.WeaponType.Money)
                {
                    Debug.Log("NPC: ถือเงินออกมาให้ฉันเห็นก่อนสิ!");
                }
                else
                {
                    Debug.Log("NPC: นายไม่มีเงินนี่นา...");
                }
            }
        }
    }

    void TradeItem()
    {
        Debug.Log("NPC: แลกเปลี่ยนสำเร็จ!");
        hasTraded = true;

        if (tradeFXPrefab != null)
        {
            Instantiate(tradeFXPrefab, transform.position, Quaternion.identity);
        }

        if (itemDropPrefab != null)
        {
            Vector3 spawnPosition = dropPoint != null ? dropPoint.position : transform.position + (transform.forward * 1.5f);
            spawnPosition.y += 0.5f; 
            Instantiate(itemDropPrefab, spawnPosition, Quaternion.identity);
        }

        // 🌟 จุดสำคัญ: สั่งลบเงินออกจากลิสต์กระเป๋าเก็บของ
        playerScript.RemoveWeapon(ControlPlayer.WeaponType.Money);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            if (tradeOnlyOnce && hasTraded) return;
            isPlayerInRange = true;
            playerScript = other.GetComponent<ControlPlayer>();
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