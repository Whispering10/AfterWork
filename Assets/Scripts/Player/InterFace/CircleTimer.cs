using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class CircleTimer : MonoBehaviour
{
    [SerializeField] private Image timerImage;

    private float totalTime;
    private float currentTime;
    private bool isRunning = false;

    public event EventHandler<EventArgs> timerEnded;


    private void Awake()
    {
        enabled = false;
    }
    public void Init()
    {
        timerImage = GetComponent<Image>();
        timerImage.type = Image.Type.Filled;
        timerImage.fillMethod = Image.FillMethod.Radial360;
        timerImage.fillOrigin = (int)Image.Origin360.Top;

        gameObject.SetActive(false);

        enabled = true;
    }

    private void Update()
    {
        if (!isRunning) return;

        currentTime -= Time.deltaTime / Time.timeScale;
        UpdateVisual();

        if (currentTime <= 0)
        {
            StopTimer();
        }
    }

    private void UpdateVisual()
    {
        float fillAmount = currentTime / totalTime;
        timerImage.fillAmount = fillAmount;
        timerImage.color = Color.Lerp(Color.red, Color.green, fillAmount);
    }

    public void StartTimer(float time)
    {
        totalTime = time;
        currentTime = totalTime;
        isRunning = true;
        gameObject.SetActive(true);
    }

    public void StopTimer()
    {
        gameObject.SetActive(false);
        isRunning = false;

        timerEnded?.Invoke(this, new EventArgs());
    }
}
