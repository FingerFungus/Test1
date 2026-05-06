using UnityEngine;
using UnityEngine.UI;

public class HealthBar : MonoBehaviour
{
    public void SetMaxHealth(int health)
    {
        slider.maxValue = health;
        slider.value = health;
    }
    public Slider slider;

    public void SetHealth(int health)
    {
        slider.value = health;
    }
}
