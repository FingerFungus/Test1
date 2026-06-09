using UnityEngine;

public class WinTrigger : MonoBehaviour
{
    private WinGame winGame;

    void Start()
    {
        winGame = FindObjectOfType<WinGame>();
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            winGame.TriggerWin();
        }
    }
}