using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using System.Collections;

public class SimpleLoader : MonoBehaviour
{
    public GameObject loadingScreen;
    public Slider progressBar;

    public void LoadLevel(int sceneIndex)
    {
        StartCoroutine(LoadAsync(sceneIndex));
    }

    IEnumerator LoadAsync(int sceneIndex)
    {
        AsyncOperation op = SceneManager.LoadSceneAsync(sceneIndex);
        loadingScreen.SetActive(true);

        while (!op.isDone)
        {
            // Progress goes from 0 to 0.9 before completion
            progressBar.value = Mathf.Clamp01(op.progress / 0.9f);
            yield return null;
        }
    }
}
