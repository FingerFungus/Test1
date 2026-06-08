using UnityEngine;
using UnityEngine.SceneManagement;

public class PlayerDeath : MonoBehaviour
{
    [Header("Settings")]
    public float deathDelay = 1f; // time before scene reloads

    private HitPoints hitPoints;

    void Start()
    {
        hitPoints = GetComponent<HitPoints>();
        hitPoints.onDeath.AddListener(OnDead);
    }

    void OnDead()
    {
        Debug.Log("Player died!");

        // Disable player input/movement here if you have a player controller
        // e.g. GetComponent<PlayerController>().enabled = false;

        Invoke(nameof(LoadMenuScene), deathDelay);
    }

    void LoadMenuScene()
    {
        SceneManager.LoadScene("Menu");
    }

    void OnDestroy()
    {
        hitPoints.onDeath.RemoveListener(OnDead);
    }
}