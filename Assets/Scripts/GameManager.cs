using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.InputSystem;

public class GameManager : MonoBehaviour
{
    [SerializeField] private InputActionAsset inputActionAsset;
    [SerializeField] private bool isCongratulatonsScreen = false; // Set to true for Congratulations scene
    
    private InputActionMap uiActionMap;
    private InputAction submitAction;
    private InputAction restartAction;

    void Start()
    {
        if (inputActionAsset == null)
        {
            inputActionAsset = Resources.Load<InputActionAsset>("InputSystem_Actions");
        }

        if (inputActionAsset != null)
        {
            uiActionMap = inputActionAsset.FindActionMap("UI");
            submitAction = uiActionMap.FindAction("Submit");
            restartAction = uiActionMap.FindAction("Restart");

            if (submitAction != null)
            {
                submitAction.performed += OnSubmitPerformed;
            }
            
            if (restartAction != null)
            {
                restartAction.performed += OnRestartPerformed;
            }
            
            uiActionMap.Enable();
        }
        else
        {
            Debug.LogWarning("InputSystem_Actions asset not found");
        }
    }

    void OnDestroy()
    {
        if (submitAction != null)
        {
            submitAction.performed -= OnSubmitPerformed;
        }
        if (restartAction != null)
        {
            restartAction.performed -= OnRestartPerformed;
        }
        if (uiActionMap != null)
        {
            uiActionMap.Disable();
        }
    }

    void OnSubmitPerformed(InputAction.CallbackContext context)
    {
        LoadNextScene();
    }

    void OnRestartPerformed(InputAction.CallbackContext context)
    {
        if (isCongratulatonsScreen)
        {
            // Return to main menu from Congratulations screen
            Time.timeScale = 1f;
            SceneManager.LoadScene(0);
        }
    }

    void LoadNextScene()
    {
        int nextSceneIndex = SceneManager.GetActiveScene().buildIndex + 1;
        
        if (nextSceneIndex < SceneManager.sceneCountInBuildSettings)
        {
            Time.timeScale = 1f; // Resume time if paused
            SceneManager.LoadScene(nextSceneIndex);
        }
        else
        {
            Debug.LogWarning("No next scene available");
        }
    }
}
