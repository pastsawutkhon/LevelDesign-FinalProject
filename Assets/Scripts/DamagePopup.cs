using UnityEngine;
using TMPro;

public class DamagePopup : MonoBehaviour
{
    private TextMeshProUGUI textMesh;
    private float disappearTimer;
    private Color textColor;
    private const float DISAPPEAR_TIMER_MAX = 1f;

    public void Setup(float damageAmount)
    {
        textMesh = GetComponentInChildren<TextMeshProUGUI>();
        textMesh.SetText(damageAmount.ToString("F0")); // แสดงเลขจำนวนเต็ม
        textColor = textMesh.color;
        disappearTimer = DISAPPEAR_TIMER_MAX;
        
        // สุ่มตำแหน่งกระจายตัวเล็กน้อย
        transform.localPosition += new Vector3(Random.Range(-0.5f, 0.5f), Random.Range(-0.5f, 0.5f), 0);
    }

    private void Update()
    {
        // ลอยขึ้นข้างบน
        float moveYSpeed = 1.5f;
        transform.position += new Vector3(0, moveYSpeed) * Time.deltaTime;

        // ค่อยๆ จางหาย
        disappearTimer -= Time.deltaTime;
        if (disappearTimer < 0)
        {
            float fadeSpeed = 3f;
            textColor.a -= fadeSpeed * Time.deltaTime;
            textMesh.color = textColor;
            if (textColor.a <= 0)
            {
                Destroy(gameObject);
            }
        }
    }

    private void LateUpdate()
    {
        // ให้ตัวเลขหันหน้าเข้าหากล้องตลอดเวลา (Billboard Effect)
        if (Camera.main != null)
            transform.LookAt(transform.position + Camera.main.transform.forward);
    }
}