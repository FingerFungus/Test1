using UnityEngine;
using UnityEngine.SceneManagement;

public class PlayerKillOnFloorKill : MonoBehaviour
{
    [Header("Layer that kills the player")]
    public string killLayerName = "floorkill";

    private int killLayer;

    void Start()
    {
        killLayer = LayerMask.NameToLayer(killLayerName);

        if (killLayer == -1)
            Debug.LogError("Layer '" + killLayerName + "' does not exist!");
    }

    void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.layer == killLayer)
        {
            RestartLevel();
        }
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.gameObject.layer == killLayer)
        {
            RestartLevel();
        }
    }

    void RestartLevel()
    {
        Debug.Log("Player touched floorkill — restarting level.");
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }
}
