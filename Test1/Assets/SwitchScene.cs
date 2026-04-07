using UnityEngine;
using UnityEngine.SceneManagement;

using UnityEngine;
using UnityEngine.UIElements; // Required for UI Toolkit

public class MainMenuController : MonoBehaviour
{
    private void OnEnable()
    {
        // 1. Get the UIDocument component from this object
        VisualElement root = GetComponent<UIDocument>().rootVisualElement;

        // 2. Find the button by the name you set in UI Builder
        Button myButton = root.Q<Button>("Play");

        // 3. Register the click event
        if (myButton != null)
        {
            myButton.clicked += OnButtonClicked;
        }
    }

    private void OnButtonClicked()
    {
        SceneManager.LoadScene("SampleScene");
    }
}

