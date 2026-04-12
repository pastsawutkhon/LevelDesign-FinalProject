using UnityEngine;
using UnityEngine.SceneManagement; // แก้ Error CS0246 เรียบร้อย
using TMPro;

public class GameFlowManager : MonoBehaviour
{
    public static GameFlowManager instance;

    [Header("UI Panels")]
    public GameObject pauseMenuUI;
    public GameObject gameOverUI;

    [Header("Status Texts")]
    public TextMeshProUGUI statusText;      // สำหรับแสดง "MISSION COMPLETE" หรือ "GAME OVER"
    public TextMeshProUGUI finalScoreText;   // สำหรับแสดงคะแนนรวมสุดท้าย
    public AudioClip ButtonClickSound; // เสียงตอนกดปุ่ม (ลาก AudioSource มาใส่ตรงนี้)

    private bool isPaused = false;

    void Awake()
    {
        if (instance == null) instance = this;
    }

    void Update()
    {
        // ระบบกดปุ่ม Esc เพื่อพักเกม
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (isPaused) Resume();
            else Pause();
        }
    }

    // --- ระบบ Pause ---
    public void Pause()
    {
        if(AudioManager.instance != null)
        {
            AudioManager.instance.PlaySFX(ButtonClickSound);
        }
        pauseMenuUI.SetActive(true);
        Time.timeScale = 0f; // หยุดเวลาในเกมทั้งหมด
        isPaused = true;
        Cursor.visible = true;
        //Cursor.lockState = CursorLockMode.None;
    }

    public void Resume()
    {
        if(AudioManager.instance != null)
        {
            AudioManager.instance.PlaySFX(ButtonClickSound);
        }
        pauseMenuUI.SetActive(false);
        Time.timeScale = 1f; // ให้เวลาเดินปกติ
        isPaused = false;
        
        // กลับไปใช้โหมด Confined ตามที่คุณต้องการ (เมาส์ไม่หาย)
        Cursor.visible = true;
        //Cursor.lockState = CursorLockMode.Confined;
    }

    // --- ระบบจบเกม (เรียกใช้เมื่อเข้าเส้นชัย หรือ ตายจน Lives = 0) ---
    public void EndGame(bool isVictory)
    {
        // 1. เปิดหน้าจอ GameOver UI และหยุดเวลา
        gameOverUI.SetActive(true);
        Time.timeScale = 0f; 
        
        Cursor.visible = true;

        // 🌟 2. ดึงข้อมูลคะแนนแยกส่วน และเช็คเงื่อนไขการแพ้/ชนะ
        // ถ้าแพ้ (isVictory = false) ให้เซ็ตทุกอย่างเป็น 0 ทันที
        int enemyScore = isVictory ? TimeManager.instance.totalEnemyScore : 0; 
        int timeLeft = isVictory ? (Mathf.RoundToInt(TimeManager.instance.timeRemaining) * 3) : 0; 
        
        PlayerHealth playerHealth = FindFirstObjectByType<PlayerHealth>();
        int livesLeft = isVictory ? ((playerHealth != null) ? playerHealth.lives : 0) : 0;

        // 3. คำนวณคะแนนรวมสุทธิ (ถ้าแพ้ CalculateFinalScore จะคืนค่า 0 ให้อยู่แล้ว)
        int finalTotal = TimeManager.instance.CalculateFinalScore(livesLeft, isVictory);

        // 4. สั่งรัน Animation
        GameOverAnimation scoreAnim = gameOverUI.GetComponent<GameOverAnimation>();
        if (scoreAnim != null)
        {
            if (statusText != null)
                statusText.text = isVictory ? "MISSION COMPLETE" : "GAME OVER";

            // ส่งค่าที่เป็น 0 ทั้งหมดไปให้ Animation ในกรณีที่แพ้
            scoreAnim.StartGameOverSequence(enemyScore, timeLeft, livesLeft, finalTotal);
        }
    }

    // --- ระบบปุ่มกด ---
    public void ExitToMainMenu()
    {
            if(AudioManager.instance != null)
            {
                AudioManager.instance.PlaySFX(ButtonClickSound);
            }
        Time.timeScale = 1f;
        // โหลด Scene ลำดับที่ 0 (หรือชื่อ Scene ปัจจุบัน) เพื่อรีเซ็ตกลับไปหน้า Menu
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    public void ExitGame()
    {
            if(AudioManager.instance != null)
            {
                AudioManager.instance.PlaySFX(ButtonClickSound);
            }
        Debug.Log("Exiting Game...");
        Application.Quit();
    }
}