using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

public class GameFlowManager : MonoBehaviour
{
    public static GameFlowManager instance;

    public GameObject pauseMenuUI;
    public GameObject gameOverUI;
    public TextMeshProUGUI finalScoreText;
    
    private bool isPaused = false;

    void Awake() => instance = this;

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (isPaused) Resume();
            else Pause();
        }
    }

    public void Pause()
    {
        pauseMenuUI.SetActive(true);
        Time.timeScale = 0f; 
        isPaused = true;
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;
    }

    public void Resume()
    {
        pauseMenuUI.SetActive(false);
        Time.timeScale = 1f;
        isPaused = false;
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;
    }

    public void ShowGameOver()
    {
        gameOverUI.SetActive(true);
        finalScoreText.text = "Final Score: " + TimeManager.instance.CalculateFinalScore();
        Time.timeScale = 0f;
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;
    }

    public void ExitToMainMenu()
    {
        Time.timeScale = 1f;
        // โหลด Scene ปัจจุบันใหม่เพื่อกลับไปหน้าเมนูและรีเซ็ตค่าทั้งหมด
        SceneManager.LoadScene(SceneManager.GetActiveScene().name); 
    }

    public void ExitGame()
    {
        Application.Quit();
        Debug.Log("Game Exited");
    }
}