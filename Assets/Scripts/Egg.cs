using UnityEngine;

public class Egg : MonoBehaviour
{
    private PlayerController playerController;

    void Start()
    {
        // Find the player controller in the scene
        playerController = FindAnyObjectByType<PlayerController>();
        
        gameObject.tag = "Egg";
    }

    void OnTriggerStay2D(Collider2D collision)
    {
        // Check level completion when egg is in contact with a nest
        if (collision.CompareTag("Nest") && playerController != null)
        {
            playerController.CheckLevelCompletion();
        }
    }
}
