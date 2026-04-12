using UnityEngine;
using System.Collections;

public class MenuManager : MonoBehaviour
{
    public static MenuManager instance;

    [Header("UI Panels")]
    public GameObject mainMenuUI;
    public GameObject inGameCanvas;

    [Header("Cameras")]
    public GameObject transitionCamera; // 🌟 กล้องตัวนี้จะวางแช่ไว้หน้าเมนูและใช้เลื่อนด้วย
    public GameObject playerCamera;     // 🌟 กล้องจริงที่ล็อกตัวผู้เล่น (มีสคริปต์ Follow)

    [Header("Animation Points")]
    public Transform playerStartPoint;  // จุดปลายทางที่กล้องควรเลื่อนไปถึง (สร้างไว้เป็นลูกของ Player)

    [Header("Settings")]
    public float transitionDuration = 2.0f;
    public AnimationCurve transitionCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);

    public ControlPlayer playerScript;
    public AudioClip ButtonClickSound; // เสียงตอนกดปุ่ม (ลาก AudioSource มาใส่ตรงนี้)

    void Awake() => instance = this;

    void Start()
    {
        ShowMainMenu();
    }

    public void ShowMainMenu()
    {
        mainMenuUI.SetActive(true);
        inGameCanvas.SetActive(false);
        
        // เริ่มเกมมาให้เปิดกล้อง Transition (ที่วางมุมเมนูไว้) และปิดกล้องเล่นจริง
        transitionCamera.SetActive(true);
        playerCamera.SetActive(false);
        
        // ผู้เล่นจะยังหันตามเมาส์ได้แต่เดินไม่ได้
        if (playerScript != null) playerScript.canMove = false; 
        
        Cursor.visible = true;
        //Cursor.lockState = CursorLockMode.None;
    }

    public void PressPlay()
    {
        if(AudioManager.instance != null)
        {
            AudioManager.instance.PlaySFX(ButtonClickSound);
        }
        mainMenuUI.SetActive(false);
        // เริ่มการเลื่อนจากจุดปัจจุบันของ transitionCamera ไปยัง playerStartPoint
        StartCoroutine(AnimateCameraTransition());
    }

    IEnumerator AnimateCameraTransition()
    {
        float elapsed = 0;
        // เก็บตำแหน่งและหมุนเริ่มต้น (ซึ่งวางไว้ที่มุมเมนู)
        Vector3 startPos = transitionCamera.transform.position;
        Quaternion startRot = transitionCamera.transform.rotation;

        while (elapsed < transitionDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / transitionDuration;
            float curveT = transitionCurve.Evaluate(t);

            // เลื่อนกล้องจากมุมเมนู เข้าหาจุดเริ่มเล่น
            transitionCamera.transform.position = Vector3.Lerp(startPos, playerStartPoint.position, curveT);
            transitionCamera.transform.rotation = Quaternion.Slerp(startRot, playerStartPoint.rotation, curveT);

            yield return null;
        }

        // เมื่อเลื่อนเสร็จ: ปิดกล้องเลื่อน และเปิดกล้องจริงที่ล็อกตัวผู้เล่น
        transitionCamera.SetActive(false);
        playerCamera.SetActive(true);

        // เข้าสู่โหมดการเล่น
        inGameCanvas.SetActive(true);
        if (playerScript != null) playerScript.canMove = true; 
        
        // ใช้โหมด Confined เพื่อให้เมาส์ไม่หายและไม่หลุดขอบ
        Cursor.visible = true; 
        //Cursor.lockState = CursorLockMode.Confined;
        
        if (TimeManager.instance != null) TimeManager.instance.StartTimer();
    }
}