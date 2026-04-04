using UnityEngine;

public class ExitWay : MonoBehaviour
{
    [Header("การตั้งค่าทางออก")]
    public KeyCode interactKey = KeyCode.E;
    public ControlPlayer.WeaponType requiredItem = ControlPlayer.WeaponType.GoldenCoin;
    
    [Header("การเปลี่ยนสีและแสง")]
    public Color successColor = Color.green;
    [ColorUsage(true, true)] // ทำให้เลือกค่าความสว่างแบบ HDR ได้ใน Inspector
    public Color emissionColor = Color.green;

    [Header("เอฟเฟกต์")]
    public GameObject successFX;
    public Transform fxPoint;

    private bool isPlayerInRange = false;
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
            if (playerScript != null)
            {
                if (playerScript.currentWeapon == requiredItem)
                {
                    ActivateExit();
                }
                else
                {
                    Debug.Log("ต้องถือ " + requiredItem.ToString() + " ไว้ในมือก่อน!");
                }
            }
        }
    }

    void ActivateExit()
    {
        Debug.Log("ทางออกเปิดใช้งาน!");

        if (objectRenderer != null)
        {
            // 1. เปลี่ยนสีหลัก (Albedo)
            objectRenderer.material.color = successColor;

            // 2. เปิดใช้งานระบบ Emission ใน Material
            objectRenderer.material.EnableKeyword("_EMISSION");

            // 3. เซ็ตสี Emission (ใช้ชื่อ Property มาตรฐานของ Unity)
            objectRenderer.material.SetColor("_EmissionColor", emissionColor);
        }

        if (successFX != null)
        {
            Instantiate(successFX, fxPoint.position, fxPoint.rotation);
        }

        playerScript.RemoveWeapon(requiredItem);
    }

    // --- ส่วนของ Trigger เหมือนเดิม ---
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