using UnityEngine;

public class ActiveThis : MonoBehaviour
{
    [Header("1. วัตถุที่จะโดนทำลาย (เงื่อนไข)")]
    public GameObject targetToDestroy;

    [Header("2. วัตถุที่จะให้ปรากฏออกมา (ผลลัพธ์)")]
    public GameObject objectToActive;

    [Header("การตั้งค่าเพิ่มเติม")]
    public float delayBeforeActive = 0f; // เผื่ออยากให้หน่วงเวลานิดนึงค่อยโผล่

    private bool triggered = false;

    void Update()
    {
        // เช็คว่ายังไม่ได้ทำงาน และ วัตถุเป้าหมายหายไปแล้ว (กลายเป็น null)
        if (!triggered && targetToDestroy == null)
        {
            triggered = true;
            Invoke("ActivateNow", delayBeforeActive);
        }
    }

    void ActivateNow()
    {
        if (objectToActive != null)
        {
            objectToActive.SetActive(true);
            Debug.Log("<color=green>Watcher:</color> " + objectToActive.name + " ถูกเปิดใช้งานแล้ว!");
        }

        // เมื่อทำงานเสร็จแล้ว ให้ทำลายตัวสคริปต์ Watcher ทิ้งไปเลยเพื่อไม่ให้หนักเครื่อง
        Destroy(gameObject);
    }
}