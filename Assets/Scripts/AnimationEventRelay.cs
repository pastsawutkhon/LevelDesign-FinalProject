using UnityEngine;

public class AnimationEventRelay : MonoBehaviour
{
    private ControlPlayer playerScript;

    void Start()
    {
        // คำสั่งนี้จะวิ่งขึ้นไปหาตัวแม่ เพื่อดึงสคริปต์ ControlPlayer มาเก็บไว้
        playerScript = GetComponentInParent<ControlPlayer>();
        
        if (playerScript == null)
        {
            Debug.LogError("หา ControlPlayer ที่ตัวแม่ไม่เจอ! เช็คดูว่าตัวแม่มีสคริปต์นี้แปะอยู่ไหม");
        }
    }

    // ฟังก์ชันนี้แหละที่เราจะให้ Animation Event เรียกใช้
    public void TriggerKnifeDamage()
    {
        if (playerScript != null)
        {
            // สั่งให้สคริปต์ตัวแม่ รันฟังก์ชันทำดาเมจที่เราเขียนไว้
            playerScript.ExecuteKnifeDamage();
        }
    }
}