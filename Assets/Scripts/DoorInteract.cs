using UnityEngine;

public class DoorInteract : MonoBehaviour
{
    [Header("ชิ้นส่วนประตู (ลาก Hinge มาใส่ตรงนี้)")]
    public Transform doorHinge; 

    [Header("ตัวล็อกประตู (ปล่อยว่างได้ถ้าไม่ต้องการล็อก)")]
    public GameObject padlock; // ถ้าลาก Object มาใส่ = ล็อก, ถ้าไม่ใส่ = เปิดได้ปกติ

    [Header("การตั้งค่าประตู")]
    public KeyCode interactKey = KeyCode.E; 
    public float openAngle = 90f;           
    public float openSpeed = 5f;            

    private bool isOpen = false;
    private bool isPlayerInRange = false;
    private Quaternion closedRotation;
    private Quaternion openRotation;

    void Start()
    {
        if (doorHinge != null)
        {
            closedRotation = doorHinge.rotation;
            openRotation = Quaternion.Euler(doorHinge.eulerAngles + new Vector3(0, openAngle, 0));
        }
    }

    void Update()
    {
        // ถ้าผู้เล่นอยู่ในระยะ และกดปุ่ม E
        if (isPlayerInRange && Input.GetKeyDown(interactKey))
        {
            // --- ระบบเช็คล็อกประตู ---
            // ถ้ามีการใส่ตัวล็อกไว้ และตัวล็อกนั้นยังไม่ถูกทำลาย (ยังไม่เป็น null)
            if (padlock != null)
            {
                Debug.Log("ประตูถูกล็อก! ต้องทำลายสิ่งกีดขวาง/ตัวล็อกก่อน");
                return; // สั่งหยุดการทำงานตรงนี้ ประตูจะไม่เปิดเด็ดขาด
            }

            // ถ้าหลุดเงื่อนไขด้านบนมาได้ (ไม่ได้ใส่ล็อกแต่แรก หรือล็อกโดนทำลายไปแล้ว)
            isOpen = !isOpen; 
        }

        // ระบบหมุนประตู (เปิด/ปิด)
        if (doorHinge != null)
        {
            Quaternion targetRotation = isOpen ? openRotation : closedRotation;
            doorHinge.rotation = Quaternion.Slerp(doorHinge.rotation, targetRotation, Time.deltaTime * openSpeed);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            isPlayerInRange = true;
            
            // เปลี่ยนข้อความ UI ตามสถานะว่าล็อกอยู่หรือไม่
            if (padlock != null)
            {
                Debug.Log("ประตูถูกล็อกอยู่");
                UIManager.instance.ShowNotification("Door is locked!");
            }
            else
            {
                Debug.Log("กด E เพื่อ " + (isOpen ? "ปิด" : "เปิด") + " ประตู");
                UIManager.instance.ShowNotification("Press E to " + (isOpen ? "close" : "open") + " door");
            } 
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            isPlayerInRange = false;
        }
    }
}