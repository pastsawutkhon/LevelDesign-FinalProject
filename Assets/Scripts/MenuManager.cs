using UnityEngine;

public class MenuManager : MonoBehaviour
{
    public static MenuManager instance;

    [Header("UI Panels")]
    public GameObject mainMenuUI;
    public GameObject inGameCanvas; // 🌟 ลาก Canvas ที่ใช้ตอนเล่นมาใส่ที่นี่

    [Header("Cameras")]
    public GameObject menuCamera;  
    public GameObject playerCamera; 

    public ControlPlayer playerScript;

    void Awake() => instance = this;

    void Start()
    {
        ShowMainMenu();
    }

    public void ShowMainMenu()
    {
        mainMenuUI.SetActive(true);
        inGameCanvas.SetActive(false); // 🌟 ปิด UI ตอนเล่นขณะอยู่หน้าเมนู
        
        menuCamera.SetActive(true);
        playerCamera.SetActive(false);
        
        playerScript.canMove = false; 
        
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;
    }

    public void PressPlay()
    {
        mainMenuUI.SetActive(false);
        inGameCanvas.SetActive(true); // 🌟 เปิด UI ตอนเล่นเมื่อกดเริ่มเกม
        
        menuCamera.SetActive(false);
        playerCamera.SetActive(true);
        
        playerScript.canMove = true;
        TimeManager.instance.StartTimer();
    }
}