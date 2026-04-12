using UnityEngine;

public class AudioManager : MonoBehaviour
{
    // 🌟 Singleton Instance เพื่อให้เข้าถึงจากสคริปต์อื่นได้ง่าย
    public static AudioManager instance;

    public AudioSource sfxSource;

    void Awake()
    {
        // ตรวจสอบว่ามี AudioManager อยู่แล้วหรือยัง
        if (instance == null)
        {
            instance = this;
            // 🌟 สั่งให้ Object นี้คงอยู่ตลอดไปแม้เปลี่ยน Scene
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            // ถ้ามีอยู่แล้วให้ทำลายตัวที่สร้างมาใหม่ทิ้ง เพื่อไม่ให้มีซ้ำ
            Destroy(gameObject);
        }
    }

    // ฟังก์ชันสำหรับสั่งเล่นเสียงจากที่ไหนก็ได้
    public void PlaySFX(AudioClip clip)
    {
        if (clip != null && sfxSource != null)
        {
            sfxSource.PlayOneShot(clip);
        }
    }
}