using UnityEngine;
using TMPro;

public class TimeManager : MonoBehaviour
{
    public static TimeManager instance;

    public TextMeshProUGUI timerText;
    public float timeRemaining = 600f; //
    private bool timerIsRunning = false; //
    
    [HideInInspector] public int totalEnemyScore = 0; 

    void Awake() => instance = this;

    public void AddEnemyScore(int amount)
    {
        totalEnemyScore += amount;
    }

    public void StartTimer() => timerIsRunning = true; //

    void Update()
    {
        if (timerIsRunning && timeRemaining > 0) //
        {
            timeRemaining -= Time.deltaTime; //
            DisplayTime(timeRemaining); //
        }
        else if (timerIsRunning && timeRemaining <= 0) //
        {
            timeRemaining = 0; //
            timerIsRunning = false; //
            DisplayTime(timeRemaining); //
            GameOver(); //
        }
    }

    void DisplayTime(float timeToDisplay)
    {
        float minutes = Mathf.FloorToInt(timeToDisplay / 60); //
        float seconds = Mathf.FloorToInt(timeToDisplay % 60); //
        timerText.text = string.Format("{0:0}:{1:00}", minutes, seconds); //
    }

    // 🌟 สูตรคำนวณคะแนนใหม่ตามที่คุณต้องการ
    public int CalculateFinalScore(int livesLeft, bool isVictory)
    {
        // ถ้าตายจน Lives เหลือ 0 ให้คะแนนเป็น 0 ทันที
        if (!isVictory) return 0;

        // สูตร: (คะแนนศัตรูสะสม) + (วินาทีที่เหลือ) + (ชีวิตที่เหลือ * 100)
        int enemyScore = totalEnemyScore;
        int timeLeftScore = Mathf.RoundToInt(timeRemaining) * 3;
        int livesBonus = livesLeft * 100;

        int finalTotal = enemyScore + timeLeftScore + livesBonus;

        return Mathf.Max(0, finalTotal); // ป้องกันคะแนนติดลบ
    }
    void GameOver() { if (GameFlowManager.instance != null) GameFlowManager.instance.EndGame(false); }
}