using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.InputSystem;
using System.Collections;
using System.Collections.Generic;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(Animator))]
[RequireComponent(typeof(SpriteRenderer))]
public class PlayerController : MonoBehaviour
{
    [SerializeField] private float gridSize = 1f;
    [SerializeField] private InputActionAsset inputActionAsset;
    [SerializeField] private float moveDuration = 0.2f;
    
    private Vector2 moveDirection = Vector2.zero;
    private bool canMove = true;
    private Rigidbody2D rb;
    private InputActionMap playerActionMap;
    private InputAction moveAction;
    private Coroutine currentMoveCoroutine;
    private Animator animator;
    private SpriteRenderer spriteRenderer;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        
        if (inputActionAsset == null)
        {
            inputActionAsset = Resources.Load<InputActionAsset>("InputSystem_Actions");
        }

        if (inputActionAsset != null)
        {
            playerActionMap = inputActionAsset.FindActionMap("Player");
            moveAction = playerActionMap.FindAction("Move");

            if (moveAction != null)
            {
                playerActionMap.Enable();
                moveAction.performed += OnMovePerformed;
            }
        }
    }

    void OnDestroy()
    {
        if (moveAction != null)
        {
            moveAction.performed -= OnMovePerformed;
        }
        if (playerActionMap != null)
        {
            playerActionMap.Disable();
        }
    }

    void OnMovePerformed(InputAction.CallbackContext context)
    {
        Vector2 input = context.ReadValue<Vector2>();
        
        if (!canMove || input == Vector2.zero)
            return;

        // Determine direction based on strongest input
        if (Mathf.Abs(input.y) > Mathf.Abs(input.x))
        {
            // Vertical movement takes priority
            moveDirection = input.y > 0 ? Vector2.up : Vector2.down;
        }
        else
        {
            // Horizontal movement
            moveDirection = input.x > 0 ? Vector2.right : Vector2.left;
            
            // Flip sprite based on horizontal direction
            spriteRenderer.flipX = moveDirection.x < 0;

        }

        AttemptMove(moveDirection);
    }

    void AttemptMove(Vector2 direction)
    {
        Vector2 targetPos = (Vector2)transform.position + direction * gridSize;
        
        // Check if able move to target position
        if (!IsWallAtPosition(targetPos))
        {
            // Check if there's an egg at the target position
            Collider2D eggCollider = Physics2D.OverlapPoint(targetPos, LayerMask.GetMask("Default"));
            
            if (eggCollider != null && eggCollider.CompareTag("Egg"))
            {
                // Try to push the egg
                if (!TryPushEgg(eggCollider.gameObject, direction))
                {
                    return; // Can't push the egg, don't move
                }
            }

            // Move the player
            canMove = false;
            animator.SetBool("isMoving", true);
            MoveToGridPosition(targetPos);
        }
    }

    bool IsWallAtPosition(Vector2 position)
    {
        // Create a small circle to check for walls at the position
        Collider2D wallCollider = Physics2D.OverlapCircle(position, 0.1f, LayerMask.GetMask("Wall"));
        return wallCollider != null;
    }

    bool TryPushEgg(GameObject egg, Vector2 direction)
    {
        Vector2 eggTargetPos = (Vector2)egg.transform.position + direction * gridSize;
        
        // Check if the egg can move to the target position
        if (!IsWallAtPosition(eggTargetPos))
        {
            // Check if there's another egg at the target position
            Collider2D anotherEgg = Physics2D.OverlapPoint(eggTargetPos, LayerMask.GetMask("Default"));
            if (anotherEgg != null && anotherEgg.CompareTag("Egg") && anotherEgg.gameObject != egg)
            {
                return false; // Can't push into another egg
            }

            // Move the egg smoothly using the player controller's coroutine system
            StartCoroutine(SmoothMoveCoroutine(egg.transform, eggTargetPos));
            return true;
        }
        
        return false; // Can't push the egg into a wall
    }

    void MoveToGridPosition(Vector2 position, Transform target = null)
    {
        if (target == null)
            target = transform;

        // Stop any existing coroutine on the target
        if (target == transform && currentMoveCoroutine != null)
        {
            StopCoroutine(currentMoveCoroutine);
        }

        // For player movement, track the coroutine
        if (target == transform)
        {
            currentMoveCoroutine = StartCoroutine(SmoothMoveCoroutine(target, position));
        }
        else
        {
            // For egg movement, just start the coroutine without tracking
            StartCoroutine(SmoothMoveCoroutine(target, position));
        }
    }

    IEnumerator SmoothMoveCoroutine(Transform target, Vector2 targetPosition)
    {
        Vector2 startPosition = target.position;
        float elapsedTime = 0f;

        while (elapsedTime < moveDuration)
        {
            elapsedTime += Time.deltaTime;
            float progress = elapsedTime / moveDuration;

            // Lerp from start to target position
            Vector2 newPosition = Vector2.Lerp(startPosition, targetPosition, progress);
            target.position = new Vector3(newPosition.x, newPosition.y, target.position.z);

            yield return null;
        }

        // Ensure final position is exact
        target.position = new Vector3(targetPosition.x, targetPosition.y, target.position.z);

        // Reset movement after animation completes
        canMove = true;
        currentMoveCoroutine = null;
        animator.SetBool("isMoving", false);
        
        // Check for level completion after movement is done
        // Redundant with egg.cs but left here for safety
        CheckLevelCompletion();

    }

    // Check if all eggs are on nests
    public void CheckLevelCompletion()
    {
        GameObject[] eggs = GameObject.FindGameObjectsWithTag("Egg");
        GameObject[] nests = GameObject.FindGameObjectsWithTag("Nest");

        if (eggs.Length == 0 || nests.Length == 0)
            return;

        int eggsOnNests = 0;

        foreach (GameObject egg in eggs)
        {
            foreach (GameObject nest in nests)
            {
                if (Vector2.Distance(egg.transform.position, nest.transform.position) < 0.1f)
                {
                    eggsOnNests++;
                    break;
                }
            }
        }

        if (eggsOnNests == eggs.Length && eggsOnNests == nests.Length)
        {
            LevelComplete();
        }
    }

    void LevelComplete()
    {
        LevelManager levelManager = FindAnyObjectByType<LevelManager>();
        if (levelManager != null)
        {
            levelManager.ShowCompletionScreen();
        }
        else
        {
            Debug.LogError("LevelManager not found in scene");
        }
    }
}
