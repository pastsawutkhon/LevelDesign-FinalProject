using UnityEngine;
using TMPro;
using System.Collections;

public class UIManager : MonoBehaviour
{
    public static UIManager instance;

    [Header("UI Settings")]
    public GameObject textPrefab;       // Prefab ข้อความ (ต้องมี Canvas Group)
    public Transform container;         // NotificationContainer (ตัวที่มี Vertical Layout Group)
    public float displayDuration = 3f;  // ระยะเวลาที่ข้อความแต่ละอันจะอยู่
    public int maxMessages = 5;         // จำนวนแถวสูงสุด

    void Awake()
    {
        if (instance == null) instance = this;
        else Destroy(gameObject);
    }

    public void ShowNotification(string message)
    {
        // 1. จัดการจำนวนข้อความ: ถ้าเกินให้ดึงออกไปนอกแถวแล้วทำลายทิ้ง
        while (container.childCount >= maxMessages)
        {
            Transform firstChild = container.GetChild(0);
            firstChild.SetParent(null); // ดึงออกจากระบบ Layout เพื่อไม่ให้กินที่
            Destroy(firstChild.gameObject);
        }

        // 2. สร้างข้อความใหม่ (จะไปอยู่ล่างสุดของ Vertical Layout Group)
        GameObject newTextObj = Instantiate(textPrefab, container);
        
        TextMeshProUGUI textComp = newTextObj.GetComponent<TextMeshProUGUI>();
        if (textComp != null) textComp.text = message;

        // 3. เริ่มระบบ Fade Out เฉพาะตัวของมันเองเมื่อครบเวลา
        TextFader fader = newTextObj.GetComponent<TextFader>();
        if (fader != null) fader.StartFade(displayDuration);

        // 4. 🌟 อัปเดตค่าความจาง (Alpha) ของทุกแถวตามลำดับความเก่า/ใหม่
        UpdateMessagesAlpha();

        // 5. ทำลาย Object เมื่อครบเวลา
        Destroy(newTextObj, displayDuration);
    }

    // ฟังก์ชันคำนวณความจางแบบ Gradient
    void UpdateMessagesAlpha()
    {
        int totalItems = container.childCount;

        for (int i = 0; i < totalItems; i++)
        {
            CanvasGroup group = container.GetChild(i).GetComponent<CanvasGroup>();
            if (group != null)
            {
                // i = 0 คือบนสุด (เก่าสุด), totalItems-1 คือล่างสุด (ใหม่สุด)
                int indexFromBottom = totalItems - 1 - i; 

                // คำนวณ Alpha: ใหม่สุด = 1.0, ถัดไป = 0.8, 0.6, 0.4, 0.2
                float targetAlpha = 1f - (indexFromBottom * 0.2f);
                
                // ตั้งค่า Alpha ต่ำสุดที่ 0.2 (20%) เพื่อให้อ่านออก
                group.alpha = Mathf.Max(0.2f, targetAlpha);
            }
        }
    }
}