using UnityEngine;
using UnityEngine.UI;
using System;

public class CanvasSetting : MonoBehaviour
{
    public Text DaysText;
    public static Action newDays;
    [SerializeField] private static int days;
    private void Start()
    {
        DaysText.text = "Δενό: " + days.ToString();
        OnNewDays(UpdateDaysText);
    }
    public static void OnNewDays(Action days)
    {
        newDays += days;
    }
    public static void TheNewDaysEvent()
    {
        newDays?.Invoke();
    }
    public void UpdateDaysText()
    {
        days += 1;
        
        if (DaysText != null)
        {
            DaysText.text = "Δενό: " + days.ToString();
        }
    }
}
