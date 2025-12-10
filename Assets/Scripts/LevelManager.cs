using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.InputSystem;

public class LevelManager : MonoBehaviour
{
    [SerializeField] private InputActionAsset inputActionAsset;
    [SerializeField] private Canvas completionCanvas;
    
    private InputActionMap uiActionMap;
    private InputAction submitAction;
    private InputAction restartAction;
    private bool isShowing = false;

    void Start()
    {
        // Auto-find canvas if not assigned
        if (completionCanvas == null)
        {
            completionCanvas = GetComponentInChildren<Canvas>();
            if (completionCanvas == null)
            {
                Debug.LogError("LevelManager: Canvas not found as child or assigned in inspector!");
            }
        }

        // Hide the canvas initially
        if (completionCanvas != null)
        {
            completionCanvas.gameObject.SetActive(false);
        }

        // Setup input actions
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
        // Only respond to submit if the screen is showing
        if (isShowing)
        {
            LoadNextScene();
        }
    }

    void OnRestartPerformed(InputAction.CallbackContext context)
    {
        RestartLevel();
    }

    public void ShowCompletionScreen()
    {
        if (completionCanvas != null)
        {
            completionCanvas.gameObject.SetActive(true);
            isShowing = true;
            Time.timeScale = 0f; // Pause the game
        }
        else
        {
            Debug.LogError("Completion Canvas not assigned to LevelManager");
        }
    }

    public void HideCompletionScreen()
    {
        if (completionCanvas != null)
        {
            completionCanvas.gameObject.SetActive(false);
            isShowing = false;
            Time.timeScale = 1f; // Resume the game
        }
    }

    void LoadNextScene()
    {
        Time.timeScale = 1f; // Resume time before loading
        int nextSceneIndex = SceneManager.GetActiveScene().buildIndex + 1;
        
        if (nextSceneIndex < SceneManager.sceneCountInBuildSettings)
        {
            SceneManager.LoadScene(nextSceneIndex);
        }
        else
        {
            Debug.LogWarning("No next scene available");
        }
    }

    void RestartLevel()
    {
        Time.timeScale = 1f; // Resume time before loading
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
}
