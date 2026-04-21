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
    public GameObject door;

    [Header("ระบบจบเกม")]
    public float fadeSpeed = 1f;
    public Animator doorAnimator; // ลาก Animator ของประตูมาใส่ตรงนี้
    public AudioClip completeSound; // เสียงตอนจบเกม (ลาก AudioClip มาใส่ตรงนี้)
    private bool isPlayerInRange = false;
    private bool isActivated = false; // 🌟 สถานะว่าใส่ของไปหรือยัง
    private ControlPlayer playerScript;
    private Renderer objectRenderer;
    void Start()
    {
        objectRenderer = GetComponent<Renderer>();
        if (fxPoint == null) fxPoint = transform;
        doorAnimator.enabled = false; // ปิด Animator ไว้ก่อน เดี๋ยวค่อยเปิดตอนใช้งาน
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
                    UIManager.instance.ShowNotification("Hold the " + requiredItem.ToString() + " in your hand!");
                }
            }
            // ขั้นตอนที่ 2: ถ้าใส่ของไปแล้ว (ทางออกเปิดแล้ว) กดอีกทีเพื่อออก
            else
            {
                UIManager.instance.ShowNotification("Press E to Exit!");
                StartCoroutine(FinishGameSequence());
            }
        }
    }

    void ActivateExit()
    {
        isActivated = true; // 🌟 เปลี่ยนสถานะเป็นเปิดใช้งานแล้ว
        doorAnimator.enabled = true; // เปิด Animator ให้ประตูเริ่มทำงาน (เปิดประตู)
        AudioManager.instance.PlaySFX(completeSound); // เล่นเสียงตอนจบเกม
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
            UIManager.instance.ShowNotification("Press E to Interact");
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