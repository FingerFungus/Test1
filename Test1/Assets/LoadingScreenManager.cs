using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class LoadingScreen : MonoBehaviour
{
    public GameObject loadingPanel; // Reference to your UI panel
    //public Slider progressBar;      // Reference to a UI Slider
    //public Text progressText;       // Optional: Reference to a UI Text

    public void LoadLevel(int sceneIndex)
    {
        StartCoroutine(LoadAsync(sceneIndex));
    }

    IEnumerator LoadAsync(int sceneIndex)
    {
        AsyncOperation operation = SceneManager.LoadSceneAsync(sceneIndex);
        
        loadingPanel.SetActive(true);

        while (!operation.isDone)
        {
            // progress is 0 to 0.9 (loading) and 1.0 (activation)
            float progress = Mathf.Clamp01(operation.progress / 0.9f);
            
            if (progressBar != null) progressBar.value = progress;
            if (progressText != null) progressText.text = (progress * 100f).ToString("F0") + "%";

            yield return null;
        }
    }
}
