using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class LoadingScreenManager : MonoBehaviour
{
    public Slider progressBar;
    public Text loadingText;         // optional "Loading... X%" text
    public string SampleScene;

    void Start()
    {
        StartCoroutine(LoadSceneAsync());
    }

    IEnumerator LoadSceneAsync()
    {
        AsyncOperation operation = SceneManager.LoadSceneAsync("SampleScene");
        operation.allowSceneActivation = false;

        while (!operation.isDone)
        {
            float progress = Mathf.Clamp01(operation.progress / 0.9f);

            if (progressBar) progressBar.value = progress;
            if (loadingText) loadingText.text = $"Loading... {(int)(progress * 100)}%";

            // Activate scene when fully loaded
            if (operation.progress >= 0.9f)
            {
                yield return new WaitForSeconds(0.5f); // brief pause so it doesn't flash
                operation.allowSceneActivation = true;
            }

            yield return null;
        }
    }
}

