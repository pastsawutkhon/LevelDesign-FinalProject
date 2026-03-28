using UnityEngine;
using System.Collections; // จำเป็นต้องใช้สำหรับ Coroutine (หน่วงเวลา)

public class LootBox : MonoBehaviour
{
    [Header("โมเดลกล่อง (โมเดลที่จะให้หายไป)")]
    public GameObject chestModel; 

    [Header("เอฟเฟกต์ตอนเปิด (Particle System Prefab)")]
    public GameObject openEffectPrefab;
    public Transform effectSpawnPoint; // จุดที่เอฟเฟกต์จะเกิด (ถ้าไม่ใส่จะเกิดที่ตัวกล่อง)

    [Header("ไอเทมที่จะเด้งออกมา (FloatingItem Prefab)")]
    public GameObject floatingItemPrefab;
    public Transform itemSpawnPoint; // จุดที่ไอเทมจะเด้งออกมา (ควรอยู่เหนือกล่อง)

    [Header("การตั้งค่า (Settings)")]
    public KeyCode interactKey = KeyCode.E; // ปุ่มที่ใช้กดเปิด
    public float delayBeforeSpawn = 0.5f; // เวลาหน่วงก่อนของจะเด้ง (เพื่อให้ตรงกับเอฟเฟกต์)

    private bool isPlayerInRange = false;
    private bool isOpened = false;

    void Start()
    {
        // ถ้าไม่ใส่จุดเกิดของเอฟเฟกต์ ให้ใช้ตัวกล่องเป็นจุดเกิดแทน
        if (effectSpawnPoint == null) effectSpawnPoint = transform;
    }

    void Update()
    {
        // ถ้าผู้เล่นอยู่ในระยะ ไม่เคยเปิด และกดปุ่ม E
        if (isPlayerInRange && !isOpened && Input.GetKeyDown(interactKey))
        {
            StartCoroutine(OpenBoxSequence());
        }
    }

    // Coroutine จัดการลำดับการเปิดกล่อง
    IEnumerator OpenBoxSequence()
    {
        isOpened = true;
        Debug.Log("เปิดกล่อง!");

        // 1. เรียกเอฟเฟกต์ตอนเปิด (Particle)
        if (openEffectPrefab != null)
        {
            // สร้างเอฟเฟกต์ และทำลายมันโดยอัตโนมัติเมื่อเล่นจบ (ถ้าตัว Prefab ตั้งค่าไว้)
            // หรือใช้ Destroy ใน Prefab
            Instantiate(openEffectPrefab, effectSpawnPoint.position, effectSpawnPoint.rotation);
        }

        // 2. ทำให้โมเดลกล่องหายไป
        if (chestModel != null)
        {
            chestModel.SetActive(false); // ปิดการใช้งานโมเดล
        }

        // 3. หน่วงเวลาแป๊บนึง (เพื่อให้ตรงกับจังหวะเอฟเฟกต์)
        yield return new WaitForSeconds(delayBeforeSpawn);

        // 4. เรียก Prefab Item เด้งออกมา
        if (floatingItemPrefab != null && itemSpawnPoint != null)
        {
            // สร้างไอเทมที่จุดที่กำหนด
            // ไอเทมตัวนี้จะมีสคริปต์ FloatingItem ทำงานโดยอัตโนมัติ (ลอยและรอให้เก็บ)
            Instantiate(floatingItemPrefab, itemSpawnPoint.position, Quaternion.identity);
        }

        // 5. ทำลายตัวกล่อง (Object ตัวนี้) ออกจาก Scene ไปเลย
        Destroy(gameObject, 0.1f); // ทำลาย Object Parent ของกล่อง เพื่อความสะอาด
    }

    // ตรวจสอบเมื่อผู้เล่นเดินเข้ามาในระยะ
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player") && !isOpened)
        {
            isPlayerInRange = true;
            Debug.Log("กด E เพื่อเปิดกล่อง");
        }
    }

    // ตรวจสอบเมื่อผู้เล่นเดินออกจากระยะ
    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            isPlayerInRange = false;
        }
    }
}