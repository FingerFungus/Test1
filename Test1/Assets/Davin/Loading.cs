using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class LoadingScreen : MonoBehaviour
{
    public GameObject loadingPanel;
    public Slider progressBar;

    public void LoadLevel(int sceneIndex)
    {
        StartCoroutine(LoadAsynchronously(sceneIndex));
    }

    IEnumerator LoadAsynchronously(int sceneIndex)
    {
        // Start the background loading operation
        AsyncOperation operation = SceneManager.LoadSceneAsync(sceneIndex);

        // Show the loading UI
        loadingPanel.SetActive(true);

        while (!operation.isDone)
        {
            // operation.progress ranges from 0 to 0.9. 
            // Normalize it to 0-1 for the slider.
            float progress = Mathf.Clamp01(operation.progress / 0.9f);
            
            progressBar.value = progress;

            yield return null;
        }
    }
}
