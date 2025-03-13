using UnityEngine;
using UnityEngine.UI;

public class HealthBar : MonoBehaviour
{
    public Slider slider;

    void Start()
    {
        // Placeholder value, actual health should be set from RobotController
        SetMaxHealth(100f);
    }

    public void SetMaxHealth(float health)
    {
        if (slider != null)
        {
            slider.maxValue = health;
            slider.value = health;
        }
        else
        {
            Debug.LogError("HealthBar: Slider component is missing!");
        }
    }

    public void UpdateHealth(float health)
    {
        if (slider != null)
        {
            slider.value = health;
        }
        else
        {
            Debug.LogError("HealthBar: Slider component is missing!");
        }
    }

    public float getHealth()
    {
        return slider.value;
    }
}

