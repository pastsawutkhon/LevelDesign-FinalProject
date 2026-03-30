using UnityEngine;

public class DoorLock : MonoBehaviour
{
    [Header("การตั้งค่าแม่กุญแจ")]
    public KeyCode interactKey = KeyCode.E;
    
    [Header("กุญแจที่ต้องใช้ (เลือกได้ว่าจะให้รับดอกไหน)")]
    // ตัวแปรนี้จะไปโผล่ใน Inspector ให้คุณเลือกประเภทกุญแจได้เลย!
    public ControlPlayer.WeaponType requiredKey = ControlPlayer.WeaponType.Key_1; 
    
    [Header("เอฟเฟกต์ตอนปลดล็อค")]
    public GameObject unlockFXPrefab; 
    public Transform fxSpawnPoint;    

    private bool isPlayerInRange = false;
    private ControlPlayer playerScript;

    void Start()
    {
        if (fxSpawnPoint == null) 
        {
            fxSpawnPoint = transform; 
        }
    }

    void Update()
    {
        if (isPlayerInRange && Input.GetKeyDown(interactKey))
        {
            if (playerScript != null)
            {
                // ตรวจสอบว่าผู้เล่น "มีกุญแจตรงตามที่กำหนดไหม"
                bool hasTheRightKey = false;
                if (requiredKey == ControlPlayer.WeaponType.Key_1 && playerScript.hasKey_1) hasTheRightKey = true;
                if (requiredKey == ControlPlayer.WeaponType.Key_2 && playerScript.hasKey_2) hasTheRightKey = true;

                // เช็คว่า มีกุญแจที่ถูกดอก และ กำลังถือมันอยู่บนมือ
                if (hasTheRightKey && playerScript.currentWeapon == requiredKey)
                {
                    Debug.Log("ไขกุญแจสำเร็จ! ทำลายแม่กุญแจและกุญแจในมือ");

                    // 1. เล่นเอฟเฟกต์ตอนไข
                    if (unlockFXPrefab != null)
                    {
                        Instantiate(unlockFXPrefab, fxSpawnPoint.position, fxSpawnPoint.rotation);
                    }

                    // 2. หักกุญแจออกจากตัวผู้เล่นตามประเภทที่ถูกใช้
                    if (requiredKey == ControlPlayer.WeaponType.Key_1) playerScript.hasKey_1 = false;
                    if (requiredKey == ControlPlayer.WeaponType.Key_2) playerScript.hasKey_2 = false;
                    
                    // 3. สั่งให้กลับไปอยู่สถานะ "มือเปล่า"
                    playerScript.UnlockWeapon(ControlPlayer.WeaponType.None);

                    // 4. ทำลายแม่กุญแจที่ขวางประตูอยู่ทิ้ง
                    Destroy(gameObject); 
                }
                // ถ้ามีกุญแจดอกนี้แล้ว แต่ยังไม่ได้กดหยิบขึ้นมาถือ
                else if (hasTheRightKey && playerScript.currentWeapon != requiredKey)
                {
                    Debug.Log("คุณมีกุญแจแล้ว แต่ต้องหยิบขึ้นมาถือให้ถูกดอกก่อนถึงจะไขได้!");
                }
                // ถ้าไม่มีกุญแจดอกที่ประตูกำหนดเลย
                else
                {
                    Debug.Log("ประตูถูกล็อค! คุณต้องไปหา " + requiredKey.ToString() + " มาก่อน");
                }
            }
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            isPlayerInRange = true;
            playerScript = other.GetComponent<ControlPlayer>(); 
            Debug.Log("กด E เพื่อตรวจสอบที่ล็อค");
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