using UnityEngine;

public class FloatingItem : MonoBehaviour
{
    [Header("ประเภทของไอเทมที่จะได้รับ")]
    // ตัวแปรนี้จะไปขึ้นหน้า Inspector ให้คุณเลือกว่าไอเทมนี้คือปืนอะไร
    public ControlPlayer.WeaponType weaponToUnlock = ControlPlayer.WeaponType.Pistol; 

    [Header("การเคลื่อนไหว (Animation)")]
    public float rotationSpeed = 90f; 
    public float bobSpeed = 2f;       
    public float bobHeight = 0.5f;    

    [Header("การตั้งค่าปุ่ม (Interaction)")]
    public KeyCode interactKey = KeyCode.E; 

    private Vector3 startPosition;
    private bool isPlayerInRange = false;

    void Start()
    {
        startPosition = transform.position;
    }

    void Update()
    {
        transform.Rotate(Vector3.up * rotationSpeed * Time.deltaTime, Space.World);

        float newY = startPosition.y + (Mathf.Sin(Time.time * bobSpeed) * bobHeight);
        transform.position = new Vector3(transform.position.x, newY, transform.position.z);

        if (isPlayerInRange && Input.GetKeyDown(interactKey))
        {
            PickUpItem();
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            isPlayerInRange = true;
            Debug.Log("กด E เพื่อเก็บ: " + weaponToUnlock.ToString());
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            isPlayerInRange = false;
        }
    }

    private void PickUpItem()
    {
        Debug.Log("เก็บไอเทมสำเร็จ!");

        // 1. หาตัวละครผู้เล่นด้วย Tag "Player"
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        
        if (player != null)
        {
            // 2. ดึงสคริปต์ ControlPlayer จากผู้เล่นมา
            ControlPlayer playerScript = player.GetComponent<ControlPlayer>();
            
            if (playerScript != null)
            {
                // 3. สั่งให้ฟังก์ชัน UnlockWeapon ทำงาน พร้อมส่งชนิดของปืนไปด้วย
                playerScript.UnlockWeapon(weaponToUnlock);
            }
        }

        // ทำลายโมเดลไอเทมที่พื้นทิ้ง หลังจากเก็บเข้าตัวแล้ว
        Destroy(gameObject);
    }
}