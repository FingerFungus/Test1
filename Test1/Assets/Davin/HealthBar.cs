using UnityEngine;
using UnityEngine.UI;

public class HealthBar : MonoBehaviour
{
    public Slider slider;
    public HitPoints playerHitPoints;

    void Start()
    {
        // Set slider range to match max health
        slider.minValue = 0;
        slider.maxValue = playerHitPoints.maxHealth;
        slider.value    = playerHitPoints.maxHealth;

        // Subscribe to the event — auto-updates whenever damage/heal occurs
        playerHitPoints.onHealthChanged.AddListener(UpdateBar);
    }

    void UpdateBar(int newHealth)
    {
        slider.value = newHealth;
    }

    void OnDestroy()
    {
        // Always clean up listeners to avoid memory leaks
        playerHitPoints.onHealthChanged.RemoveListener(UpdateBar);
    }
}