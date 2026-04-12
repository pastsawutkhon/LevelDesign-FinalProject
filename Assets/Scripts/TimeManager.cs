using UnityEngine;
using TMPro;

public class TimeManager : MonoBehaviour
{
    public static TimeManager instance;

    public TextMeshProUGUI timerText;
    public float timeRemaining = 300f; // 5 นาที
    private bool timerIsRunning = false;
    
    [HideInInspector] public int enemyKilled = 0; 

    void Awake() => instance = this;

    public void StartTimer() => timerIsRunning = true;

    void Update()
    {
        if (timerIsRunning && timeRemaining > 0)
        {
            timeRemaining -= Time.deltaTime;
            DisplayTime(timeRemaining);
        }
    }

    void DisplayTime(float timeToDisplay)
    {
        float minutes = Mathf.FloorToInt(timeToDisplay / 60);
        float seconds = Mathf.FloorToInt(timeToDisplay % 60);
        timerText.text = string.Format("{0:0}:{1:00}", minutes, seconds);
    }

    public int CalculateFinalScore()
    {
        // ยิ่งจบเร็ว (เวลาเหลือเยอะ) คะแนนยิ่งสูง
        int score = (enemyKilled * 100) + Mathf.RoundToInt(timeRemaining * 10);
        return Mathf.Max(0, score);
    }
}