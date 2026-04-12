using UnityEngine;
using System.Collections;
using UnityEngine.UI; // สำหรับเรียกใช้งาน UI

public class ExitWay : MonoBehaviour
{
    [Header("การตั้งค่าทางออก")]
    public KeyCode interactKey = KeyCode.E;
    public ControlPlayer.WeaponType requiredItem = ControlPlayer.WeaponType.GoldenCoin;
    
    [Header("การเปลี่ยนสีและแสง")]
    public Color successColor = Color.green;
    [ColorUsage(true, true)] 
    public Color emissionColor = Color.green;

    [Header("เอฟเฟกต์")]
    public GameObject successFX;
    public Transform fxPoint;

    [Header("ระบบจบเกม")]
    public CanvasGroup fadeCanvasGroup; // ลาก FadeImage ที่มี Canvas Group มาใส่
    public float fadeSpeed = 1f;

    private bool isPlayerInRange = false;
    private bool isActivated = false; // 🌟 สถานะว่าใส่ของไปหรือยัง
    private ControlPlayer playerScript;
    private Renderer objectRenderer;

    void Start()
    {
        objectRenderer = GetComponent<Renderer>();
        if (fxPoint == null) fxPoint = transform;
    }

    void Update()
    {
        if (isPlayerInRange && Input.GetKeyDown(interactKey))
        {
            // ขั้นตอนที่ 1: ถ้ายังไม่ได้ใส่ของ และถือของที่ถูกต้องอยู่
            if (!isActivated)
            {
                if (playerScript != null && playerScript.currentWeapon == requiredItem)
                {
                    ActivateExit();
                }
                else
                {
                    Debug.Log("ต้องถือ " + requiredItem.ToString() + " ไว้ในมือก่อน!");
                }
            }
            // ขั้นตอนที่ 2: ถ้าใส่ของไปแล้ว (ทางออกเปิดแล้ว) กดอีกทีเพื่อออก
            else
            {
                StartCoroutine(FinishGameSequence());
            }
        }
    }

    void ActivateExit()
    {
        isActivated = true; // 🌟 เปลี่ยนสถานะเป็นเปิดใช้งานแล้ว
        Debug.Log("ทางออกเปิดใช้งาน! กด E อีกครั้งเพื่อหนีออกไป");

        if (objectRenderer != null)
        {
            objectRenderer.material.color = successColor;
            objectRenderer.material.EnableKeyword("_EMISSION");
            objectRenderer.material.SetColor("_EmissionColor", emissionColor);
        }

        if (successFX != null)
        {
            Instantiate(successFX, fxPoint.position, fxPoint.rotation);
        }

        playerScript.RemoveWeapon(requiredItem);
    }

    // 🌟 Coroutine สำหรับค่อยๆ จอดำแล้วขึ้น Scoreboard
    IEnumerator FinishGameSequence()
    {
        if (GameFlowManager.instance != null)
        {
            GameFlowManager.instance.EndGame(true);
        }
        yield return null;
    }

    // --- ส่วนของ Trigger ---
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            isPlayerInRange = true;
            playerScript = other.GetComponent<ControlPlayer>();
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            isPlayerInRange = false;
            playerScript = null;
        }
    }
}