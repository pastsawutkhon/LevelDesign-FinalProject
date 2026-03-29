using UnityEngine;

public class DoorLock : MonoBehaviour
{
    [Header("การตั้งค่าแม่กุญแจ")]
    public KeyCode interactKey = KeyCode.E;
    
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
                // เช็คว่ามีกุญแจ และถืออยู่บนมือ
                if (playerScript.hasKey_1 && playerScript.currentWeapon == ControlPlayer.WeaponType.Key_1)
                {
                    Debug.Log("ไขกุญแจสำเร็จ! ทำลายแม่กุญแจและกุญแจในมือ");

                    // 1. เล่นเอฟเฟกต์ตอนไข
                    if (unlockFXPrefab != null)
                    {
                        Instantiate(unlockFXPrefab, fxSpawnPoint.position, fxSpawnPoint.rotation);
                    }

                    // 2. หักกุญแจทิ้งออกจากตัวผู้เล่น
                    playerScript.hasKey_1 = false; // ปรับสถานะเป็นไม่มีกุญแจ
                    
                    // 3. สั่งให้กลับไปอยู่สถานะ "มือเปล่า" (โมเดลกุญแจในมือจะถูกซ่อนทันที)
                    playerScript.UnlockWeapon(ControlPlayer.WeaponType.None);

                    // 4. ทำลายแม่กุญแจที่ขวางประตูอยู่ทิ้ง
                    Destroy(gameObject); 
                }
                else if (playerScript.hasKey_1 && playerScript.currentWeapon != ControlPlayer.WeaponType.Key_1)
                {
                    Debug.Log("คุณมีกุญแจนะ แต่ต้องหยิบขึ้นมาถือ (กด 5) ก่อนถึงจะไขได้!");
                }
                else
                {
                    Debug.Log("ประตูถูกล็อค! คุณต้องไปหา Key_1 มาก่อน");
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
            Debug.Log("กด E เพื่อใช้กุญแจไขล็อค");
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