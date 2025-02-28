using UnityEngine;
using UnityEngine.Rendering.Universal;
using System;

public class DaysSettingToTime : MonoBehaviour
{
    private Light2D sun;
    [SerializeField] private float secondsInFullDay = 120f;
    [Range(0, 1)]
    [SerializeField] private float currentTimeOfDay = 0; //можно подумать над static, но это страшная вещь
    [HideInInspector]
    [SerializeField] private float timeMultiplier = 1f;
    private float sunInitialIntensity;
    private void Start()
    {
        sun = GetComponent<Light2D>();
        sunInitialIntensity = sun.intensity;
    }
    private void Update()
    {
        UpdateSun();
        currentTimeOfDay += (Time.deltaTime / (secondsInFullDay*60)) * timeMultiplier;
        if (currentTimeOfDay >= 1)
        { 
            currentTimeOfDay = 0;
            CanvasSetting.TheNewDaysEvent();
        }
    }

    void UpdateSun()
    {
        float intensityMultiplier = 1;
        if (currentTimeOfDay <= 0.3f || currentTimeOfDay >= 1f)
        {
            intensityMultiplier = 0;
        }
        else if (currentTimeOfDay <= 0.45f)
        {
            intensityMultiplier = Mathf.Clamp01((currentTimeOfDay - 0.3f) * (1 / 0.15f));   
        }
        else if (currentTimeOfDay >= 0.55f)
        {
            intensityMultiplier = Mathf.Clamp01(1 - ((currentTimeOfDay - 0.55f) * (1 / 0.45f)));  
        }

        sun.intensity = sunInitialIntensity * intensityMultiplier;  
    }
}
