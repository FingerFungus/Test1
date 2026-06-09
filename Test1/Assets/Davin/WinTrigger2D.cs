using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class WinTrigger2D : MonoBehaviour
{
    [Tooltip("Reference to the WinGame component. If left empty the script will try to find one in the scene.")]
    public WinGame winGame;

    [Tooltip("Tag used to identify the player.")]
    public string playerTag = "Player";

    [Tooltip("If true the trigger only fires once.")]
    public bool oneUse = true;

    private bool triggered;

    private void Awake()
    {
        if (winGame == null)
        {
            winGame = Object.FindFirstObjectByType<WinGame>();
        }
    }

    private void Reset()
    {
        var col = GetComponent<Collider2D>();
        if (col != null)
            col.isTrigger = true;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (triggered && oneUse) return;
        if (!other.CompareTag(playerTag)) return;

        if (winGame != null)
        {
            winGame.TriggerWin();
            triggered = true;
        }
        else
        {
            Debug.LogWarning("WinTrigger2D: No WinGame reference found in scene.");
        }
    }
}
