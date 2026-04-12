using UnityEngine;
using TMPro;
using System.Collections;

public class GameOverAnimation : MonoBehaviour
{
    [Header("1-4 Fade Groups")]
    public CanvasGroup blackOverlay;      
    public CanvasGroup scoreboardCG;      
    public CanvasGroup statusCanvasGroup; 
    public CanvasGroup totalScoreCG;      

    [Header("5, 7, 9 Score CanvasGroups")]
    public CanvasGroup enemyScoreCG;      
    public CanvasGroup timeScoreCG;       
    public CanvasGroup livesScoreCG;      

    [Header("11-12 Buttons Group")]
    // 🌟 ลาก GameObject ที่รวมปุ่มทั้งสองอันและมี Canvas Group มาใส่ตรงนี้
    public CanvasGroup buttonsGroupCG; 

    [Header("Text Elements")]
    public TextMeshProUGUI enemyScoreText;
    public TextMeshProUGUI timeScoreText;
    public TextMeshProUGUI livesScoreText;
    public TextMeshProUGUI totalScoreText;

    [Header("Settings")]
    public float fadeSpeed = 2f;
    public float countDuration = 0.5f;
    public float buttonDelay = 1f; // ระยะเวลาหน่วงก่อนปุ่มโผล่มา

    public void StartGameOverSequence(int enemy, int time, int lives, int total)
    {
        // Step 0: รีเซ็ตค่าและซ่อนทุกอย่างรวมถึงปุ่ม
        blackOverlay.alpha = 0;
        scoreboardCG.alpha = 0;
        statusCanvasGroup.alpha = 0;
        totalScoreCG.alpha = 0;
        enemyScoreCG.alpha = 0;
        timeScoreCG.alpha = 0;
        livesScoreCG.alpha = 0;
        buttonsGroupCG.alpha = 0; // ซ่อนปุ่มไว้ก่อน

        enemyScoreText.text = "Enemy Score: 0";
        timeScoreText.text = "Time Left: 0";
        livesScoreText.text = "Lives Bonus: 0";
        totalScoreText.text = "0"; 

        StartCoroutine(PlaySequence(enemy, time, lives, total));
    }

    IEnumerator PlaySequence(int enemy, int time, int lives, int total)
    {
        // --- ขั้นตอนที่ 1 ถึง 10 (เหมือนเดิม) ---
        yield return StartCoroutine(FadeCanvas(blackOverlay, 1f));
        yield return StartCoroutine(FadeCanvas(scoreboardCG, 1f));
        yield return StartCoroutine(FadeCanvas(statusCanvasGroup, 1f));
        yield return StartCoroutine(FadeCanvas(totalScoreCG, 1f));

        int currentTotal = 0;

        // Enemy Score Sequence
        yield return StartCoroutine(FadeCanvas(enemyScoreCG, 1f));
        yield return StartCoroutine(CountNumber(0, enemy, enemyScoreText, "Enemy Score: "));
        yield return StartCoroutine(CountNumber(currentTotal, currentTotal + enemy, totalScoreText, ""));
        currentTotal += enemy;

        // Time Score Sequence
        yield return StartCoroutine(FadeCanvas(timeScoreCG, 1f));
        yield return StartCoroutine(CountNumber(0, time, timeScoreText, "Time Left: "));
        yield return StartCoroutine(CountNumber(currentTotal, currentTotal + time, totalScoreText, ""));
        currentTotal += time;

        // Lives Score Sequence
        yield return StartCoroutine(FadeCanvas(livesScoreCG, 1f));
        int livesPoints = lives * 100;
        yield return StartCoroutine(CountNumber(0, livesPoints, livesScoreText, "Lives Bonus: "));
        yield return StartCoroutine(CountNumber(currentTotal, currentTotal + livesPoints, totalScoreText, ""));

        // --- 🌟 ขั้นตอนใหม่: ปุ่มแสดงผล ---
        
        // 11. Delay หลังจากสรุปคะแนนเสร็จ
        yield return new WaitForSecondsRealtime(buttonDelay);

        // 12. Fade In ปุ่ม (Exit, Main menu) เข้ามาพร้อมกัน
        yield return StartCoroutine(FadeCanvas(buttonsGroupCG, 1f));
    }

    // ฟังก์ชันช่วย Fade
    IEnumerator FadeCanvas(CanvasGroup cg, float targetAlpha)
    {
        while (!Mathf.Approximately(cg.alpha, targetAlpha))
        {
            cg.alpha = Mathf.MoveTowards(cg.alpha, targetAlpha, Time.unscaledDeltaTime * fadeSpeed);
            yield return null;
        }
        cg.alpha = targetAlpha;
    }

    // ฟังก์ชันรันตัวเลข
    IEnumerator CountNumber(int start, int end, TextMeshProUGUI textTarget, string prefix)
    {
        float elapsed = 0;
        while (elapsed < countDuration)
        {
            elapsed += Time.unscaledDeltaTime;
            int current = (int)Mathf.Lerp(start, end, elapsed / countDuration);
            textTarget.text = prefix + current.ToString();
            yield return null;
        }
        textTarget.text = prefix + end.ToString();
    }
}