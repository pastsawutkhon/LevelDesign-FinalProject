using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using TMPro;

public class InventoryUI : MonoBehaviour
{
    [Header("อ้างอิงสคริปต์ผู้เล่น")]
    public ControlPlayer playerScript;

    [Header("ตั้งค่า UI")]
    public GameObject slotPrefab;   // ลาก SlotPrefab มาใส่
    public Transform slotContainer; // ลาก LoadoutPanel มาใส่

    [Header("ไอคอนอาวุธ (ใส่รูป 2D)")]
    public Sprite rifleIcon;
    public Sprite pistolIcon;
    public Sprite knifeIcon;
    public Sprite dynamiteIcon;
    public Sprite key1Icon;
    public Sprite key2Icon;
    public Sprite moneyIcon;
    public Sprite goldenCoinIcon;

    [Header("สีไฮไลท์ตอนเลือก (Valorant Style)")]
    public Color unselectedColor = new Color(1f, 1f, 1f, 0.4f); // สีจางๆ ตอนไม่ได้ถือ
    public Color selectedColor = new Color(1f, 1f, 1f, 1f);     // สีสว่าง 100% ตอนถืออยู่
    public Color bgSelectedColor = new Color(1f, 0.8f, 0f, 0.5f); // สีพื้นหลังตอนเลือก (เช่น สีเหลืองทอง)
    public Color bgUnselectedColor = new Color(0f, 0f, 0f, 0.5f); // สีพื้นหลังตอนปกติ (สีดำ)

    private List<GameObject> spawnedSlots = new List<GameObject>();

    void Update()
    {
        if (playerScript == null) return;
        UpdateLoadoutUI();
    }

    void UpdateLoadoutUI()
    {
        while (spawnedSlots.Count < playerScript.inventory.Count)
        {
            GameObject newSlot = Instantiate(slotPrefab, slotContainer);
            spawnedSlots.Add(newSlot);
        }
        
        while (spawnedSlots.Count > playerScript.inventory.Count)
        {
            Destroy(spawnedSlots[spawnedSlots.Count - 1]);
            spawnedSlots.RemoveAt(spawnedSlots.Count - 1);
        }

        for (int i = 0; i < playerScript.inventory.Count; i++)
        {
            ControlPlayer.WeaponType weapon = playerScript.inventory[i];
            GameObject slot = spawnedSlots[i];

            // 🌟 เพิ่มระบบป้องกัน Error: ค้นหา Object ก่อนว่ามีชื่อตรงไหม
            Transform iconTransform = slot.transform.Find("Icon");
            Transform numberTransform = slot.transform.Find("Number");

            if (iconTransform == null || numberTransform == null)
            {
                Debug.LogError("หา Object ย่อยชื่อ 'Icon' หรือ 'Number' ใน SlotPrefab ไม่เจอครับ! เข้าไปเช็คการสะกดชื่อใน Prefab ด่วน!");
                continue; // ข้ามการทำงานช่องนี้ไปก่อน เกมจะได้ไม่ค้าง
            }

            Image iconImage = iconTransform.GetComponent<Image>();
            TextMeshProUGUI numberText = numberTransform.GetComponent<TextMeshProUGUI>(); 
            Image backgroundImage = slot.GetComponent<Image>(); 

            // 🌟 เช็คอีกชั้นเผื่อลืมใส่ Component
            if (iconImage == null || numberText == null)
            {
                Debug.LogError("เจอชื่อ Object แล้ว แต่ตัว Icon ไม่มี Component Image หรือ ตัว Number ไม่มี Component TextMeshProUGUI ครับ");
                continue;
            }

            iconImage.sprite = GetIconForWeapon(weapon);
            numberText.text = (i + 1).ToString();

            // --- โค้ดส่วนบนของฟังก์ชัน UpdateLoadoutUI() ยังเหมือนเดิม ---

            iconImage.sprite = GetIconForWeapon(weapon);
            numberText.text = (i + 1).ToString();

            // 🌟 สร้างตัวแปรมารับค่า Scale ดั้งเดิมของ Prefab
            Vector3 originalScale = slotPrefab.transform.localScale;
            Vector3 targetScale;

            if (playerScript.currentWeapon == weapon)
            {
                iconImage.color = selectedColor;
                backgroundImage.color = bgSelectedColor;
                // 🌟 ขยาย 10% จากขนาดดั้งเดิม
                targetScale = originalScale * 1.1f; 
            }
            else
            {
                iconImage.color = unselectedColor;
                backgroundImage.color = bgUnselectedColor;
                // 🌟 กลับไปเป็นขนาดดั้งเดิม
                targetScale = originalScale; 
            }

            // 🌟 ให้มันค่อยๆ ขยายหรือหด (Lerp) ไปหาขนาดเป้าหมาย
            slot.transform.localScale = Vector3.Lerp(slot.transform.localScale, targetScale, Time.deltaTime * 10f);
        }
    }

    // ฟังก์ชันช่วยแปลชนิดอาวุธเป็นรูปภาพ
    Sprite GetIconForWeapon(ControlPlayer.WeaponType type)
    {
        switch (type)
        {
            case ControlPlayer.WeaponType.Rifle: return rifleIcon;
            case ControlPlayer.WeaponType.Pistol: return pistolIcon;
            case ControlPlayer.WeaponType.Knife: return knifeIcon;
            case ControlPlayer.WeaponType.Dynamite: return dynamiteIcon;
            case ControlPlayer.WeaponType.Key_1: return key1Icon;
            case ControlPlayer.WeaponType.Key_2: return key2Icon;
            case ControlPlayer.WeaponType.Money: return moneyIcon;
            case ControlPlayer.WeaponType.GoldenCoin: return goldenCoinIcon;
            default: return null;
        }
    }
}