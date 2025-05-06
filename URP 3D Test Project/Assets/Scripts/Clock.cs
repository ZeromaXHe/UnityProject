using System;
using UnityEngine;

public class Clock : MonoBehaviour
{
    private const float HoursToDegrees = -30f, MinutesToDegrees = -6f, SecondsToDegrees = -6f;

    [SerializeField] private Transform hoursPivot, minutesPivot, secondsPivot;

    // Start is called before the first frame update
    private void Start()
    {
    }

    // Update is called once per frame
    private void Update()
    {
        var time = DateTime.Now.TimeOfDay;
        hoursPivot.localRotation = Quaternion.Euler(0, 0, HoursToDegrees * (float)time.TotalHours);
        minutesPivot.localRotation = Quaternion.Euler(0, 0, MinutesToDegrees * (float)time.TotalMinutes);
        secondsPivot.localRotation = Quaternion.Euler(0, 0, SecondsToDegrees * (float)time.TotalSeconds);
    }
}