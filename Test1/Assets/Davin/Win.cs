using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class WinGame : MonoBehaviour
{
    [Header("UI")]
    public GameObject winPanel;

    void Start()
    {
        if (winPanel != null)
            winPanel.SetActive(false);
    }

    public void TriggerWin()
    {
        winPanel.SetActive(true);
        Time.timeScale = 0f;
    }



    public void GoToMenu()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene("Menu");
    }
}