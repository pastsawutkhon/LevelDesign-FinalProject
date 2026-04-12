using UnityEngine;
using System.Collections;

[RequireComponent(typeof(CanvasGroup))]
public class TextFader : MonoBehaviour
{
    private CanvasGroup canvasGroup;

    public void StartFade(float duration)
    {
        canvasGroup = GetComponent<CanvasGroup>();
        StartCoroutine(FadeOutAtEnd(duration));
    }

    IEnumerator FadeOutAtEnd(float duration)
    {
        // รอจนเกือบหมดเวลา (เหลือ 0.5 วินาทีสุดท้ายค่อยจางหายลับไป)
        float waitTime = duration - 0.5f;
        yield return new WaitForSeconds(waitTime);

        float counter = 0;
        float startAlpha = canvasGroup.alpha; // เริ่มจางจากค่าความจางปัจจุบันในแถว

        while (counter < 0.5f)
        {
            counter += Time.deltaTime;
            canvasGroup.alpha = Mathf.Lerp(startAlpha, 0, counter / 0.5f);
            yield return null;
        }
    }
}