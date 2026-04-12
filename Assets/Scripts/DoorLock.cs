using UnityEngine;

public class DoorLock : MonoBehaviour
{
    [Header("การตั้งค่าแม่กุญแจ")]
    public KeyCode interactKey = KeyCode.E;
    
    [Header("กุญแจที่ต้องใช้")]
    public ControlPlayer.WeaponType requiredKey = ControlPlayer.WeaponType.Key_1; 
    
    [Header("เอฟเฟกต์ตอนปลดล็อค")]
    public GameObject unlockFXPrefab; 
    public Transform fxSpawnPoint;    

    private bool isPlayerInRange = false;
    private ControlPlayer playerScript;

    void Start()
    {
        if (fxSpawnPoint == null) fxSpawnPoint = transform; 
    }

    void Update()
    {
        if (isPlayerInRange && Input.GetKeyDown(interactKey))
        {
            if (playerScript != null)
            {
                // 🌟 เปลี่ยนมาเช็คว่า "ของที่ถืออยู่ในมือตอนนี้ ใช่กุญแจที่ถูกต้องไหม?"
                if (playerScript.currentWeapon == requiredKey)
                {
                    Debug.Log("ไขกุญแจสำเร็จ! ลบกุญแจออกและสลับเป็นมือเปล่า");
                    UIManager.instance.ShowNotification("Door unlocked!");

                    // 1. เล่นเอฟเฟกต์ (ถ้ามี)
                    if (unlockFXPrefab != null)
                    {
                        Instantiate(unlockFXPrefab, fxSpawnPoint.position, fxSpawnPoint.rotation);
                    }

                    // 2. เรียกคำสั่งลบกุญแจ (ฟังก์ชันนี้จะสลับเป็นมือเปล่า WeaponType.None ให้อัตโนมัติด้วย)
                    playerScript.RemoveWeapon(requiredKey);

                    // 3. ทำลายแม่กุญแจทิ้ง
                    Destroy(gameObject); 
                }
                else
                {
                    // ถ้าในมือถือปืน หรือถือไอเทมอย่างอื่นอยู่
                    Debug.Log("คุณไม่ได้ถือกุญแจ " + requiredKey.ToString() + " ไว้ในมือ!");
                    UIManager.instance.ShowNotification("Use " + requiredKey.ToString() + " to unlock!");
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