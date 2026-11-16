
using UnityEngine;
using TMPro;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;
using UnityEngine.XR.Interaction.Toolkit.Interactors;

public class ObjectHuntController : MonoBehaviour
{
    [Header("UI Elements")]
    public TMP_Text targetText;
    public TMP_Text feedbackText;

    [Header("Intro Screen")]
    public GameObject introImage;
    public GameObject introButton;

    [Header("Objects to Find")]
    public ObjectHuntTarget[] huntTargets;

    [Header("Hiding Locations")]
    public Transform[] hideSpots;

    [Header("Game Settings")]
    public float memoryDuration = 5f;
    public float searchDuration = 20f;
    public float nextRoundDelay = 2f;
    public int roundsToWin = 3;

    [Header("Game Completion")]
    public ResetPopupController resetPopupController;

    private ObjectHuntTarget currentTarget;

    private List<ObjectHuntTarget> remainingTargets =
        new List<ObjectHuntTarget>();

    private Vector3 originalPosition;
    private Quaternion originalRotation;
    private Transform originalParent;

    private bool isSearching = false;
    private bool roundWon = false;
    private bool gameStarted = false;

    private AudioManager audioManager;

    // Every XR interactable in the scene we listen to for ray clicks.
    private readonly List<XRBaseInteractable> listenedInteractables =
        new List<XRBaseInteractable>();

    private void OnEnable()
    {
        gameStarted = false;
        isSearching = false;

        audioManager = FindFirstObjectByType<AudioManager>();

        if (introImage != null)
            introImage.SetActive(true);

        if (introButton != null)
            introButton.SetActive(true);

        if (targetText != null)
            targetText.text = "Ready to play?";

        if (feedbackText != null)
            feedbackText.text = "Press Start Game when you are ready.";
    }

    // Listen to every interactable in the scene, not just the hunt targets.
    // Some colliders are owned by a different interactable than the object
    // they belong to, so we work out the real object from what was clicked.
    private void SubscribeToInteractables()
    {
        UnsubscribeFromInteractables();

        XRBaseInteractable[] all =
            FindObjectsByType<XRBaseInteractable>(
                FindObjectsInactive.Include,
                FindObjectsSortMode.None);

        foreach (XRBaseInteractable interactable in all)
        {
            interactable.selectEntered.AddListener(OnObjectSelected);
            listenedInteractables.Add(interactable);
        }
    }

    private void UnsubscribeFromInteractables()
    {
        foreach (XRBaseInteractable interactable in listenedInteractables)
        {
            if (interactable != null)
                interactable.selectEntered.RemoveListener(OnObjectSelected);
        }

        listenedInteractables.Clear();
    }

    // Called when the XR ray is pointed at an object and the trigger is pressed.
    private void OnObjectSelected(SelectEnterEventArgs args)
    {
        if (!isSearching || roundWon || currentTarget == null)
            return;

        ObjectHuntTarget clicked = ResolveClickedTarget(args);

        if (clicked == null)
            return; // Clicked something that is not part of the hunt.

        if (clicked == currentTarget)
        {
            roundWon = true;

            if (audioManager != null)
                audioManager.PlayObjectFound();

            feedbackText.text = "Correct! You found it!";
        }
        else
        {
            feedbackText.text = "Not that one! Keep looking.";
        }
    }

    private ObjectHuntTarget ResolveClickedTarget(SelectEnterEventArgs args)
    {
        // 1. Best: the exact collider the ray hit.
        if (args.interactorObject is XRRayInteractor ray &&
            ray.TryGetCurrent3DRaycastHit(out RaycastHit hit) &&
            hit.collider != null)
        {
            ObjectHuntTarget fromHit =
                hit.collider.GetComponentInParent<ObjectHuntTarget>();

            if (fromHit != null)
                return fromHit;
        }

        XRBaseInteractable interactable =
            args.interactableObject as XRBaseInteractable;

        // 2. The collider the interactable uses.
        if (interactable != null)
        {
            foreach (Collider col in interactable.colliders)
            {
                if (col == null)
                    continue;

                ObjectHuntTarget fromCollider =
                    col.GetComponentInParent<ObjectHuntTarget>();

                if (fromCollider != null)
                    return fromCollider;
            }
        }

        // 3. The interactable's own object.
        return args.interactableObject.transform
            .GetComponentInParent<ObjectHuntTarget>();
    }

    public void StartGame()
    {
        if (gameStarted)
            return;

        StopAllCoroutines();
        RestoreCurrentTarget();

        remainingTargets.Clear();

        if (huntTargets != null)
        {
            foreach (ObjectHuntTarget target in huntTargets)
            {
                if (target != null)
                    remainingTargets.Add(target);
            }
        }

        if (targetText == null || feedbackText == null)
        {
            Debug.LogError("Object Hunt: Assign Target Text and Feedback Text!");
            return;
        }

        if (hideSpots == null || hideSpots.Length == 0)
        {
            Debug.LogError("Object Hunt: Assign your hiding spots!");
            return;
        }

        if (remainingTargets.Count < roundsToWin)
        {
            Debug.LogError("Object Hunt: Not enough hunt targets!");
            return;
        }

        foreach (Transform spot in hideSpots)
        {
            if (spot == null)
            {
                Debug.LogError("Object Hunt: A hiding spot is missing!");
                return;
            }
        }

        gameStarted = true;

        if (introImage != null)
            introImage.SetActive(false);

        if (introButton != null)
            introButton.SetActive(false);

        targetText.text = "Get Ready!";
        feedbackText.text = "The game is starting...";

        if (audioManager == null)
            audioManager = FindFirstObjectByType<AudioManager>();

        SubscribeToInteractables();

        Debug.Log("Object Hunt started successfully!");

        StartCoroutine(GameRoutine());
    }

    private IEnumerator GameRoutine()
    {
        for (int round = 0; round < roundsToWin; round++)
        {
            // MEMORY PHASE
            feedbackText.text = "Remember where everything is!";

            float memoryTimer = memoryDuration;

            while (memoryTimer > 0)
            {
                targetText.text =
                    "Memorise the room: " +
                    Mathf.CeilToInt(memoryTimer);

                memoryTimer -= Time.deltaTime;
                yield return null;
            }

            // SELECT A RANDOM OBJECT
            int targetIndex =
                Random.Range(0, remainingTargets.Count);

            currentTarget = remainingTargets[targetIndex];
            remainingTargets.RemoveAt(targetIndex);

            // SAVE ORIGINAL POSITION
            originalPosition = currentTarget.transform.position;
            originalRotation = currentTarget.transform.rotation;
            originalParent = currentTarget.transform.parent;

            // SELECT A RANDOM HIDING SPOT
            Transform hideSpot =
                hideSpots[Random.Range(0, hideSpots.Length)];

            // MOVE THE OBJECT
            currentTarget.transform.position = hideSpot.position;
            currentTarget.transform.rotation = hideSpot.rotation;

            // PLAY DISAPPEAR SOUND
            if (audioManager != null)
                audioManager.PlayObjectDisappear();

            // SEARCH PHASE
            roundWon = false;
            isSearching = true;

            feedbackText.text =
                "Find what is missing, point at it and press the trigger!";

            float searchTimer = searchDuration;

            while (searchTimer > 0 && !roundWon)
            {
                targetText.text =
                    "Find the missing object: " +
                    Mathf.CeilToInt(searchTimer) + "s";

                searchTimer -= Time.deltaTime;
                yield return null;
            }

            isSearching = false;

            if (roundWon)
            {
                feedbackText.text = "Correct! You found it!";
            }
            else
            {
                feedbackText.text = "Time's up! Keep practising.";
            }

            yield return new WaitForSeconds(nextRoundDelay);

            RestoreCurrentTarget();
        }

        // OBJECT HUNT COMPLETED
        UnsubscribeFromInteractables();

        targetText.text = "Game Complete!";
        feedbackText.text = "You completed the Object Hunt!";

        // PLAY ACHIEVEMENT SOUND
        if (audioManager != null)
            audioManager.PlayWin();

        yield return new WaitForSeconds(3f);

        // REPORT COMPLETION
        if (resetPopupController != null)
        {
            resetPopupController.CompleteObjectHunt();
        }
        else
        {
            Debug.LogError(
                "Object Hunt: Reset Popup Controller is not assigned!"
            );

            gameObject.SetActive(false);
        }
    }

    private void RestoreCurrentTarget()
    {
        if (currentTarget == null)
            return;

        currentTarget.transform.SetParent(originalParent, true);
        currentTarget.transform.position = originalPosition;
        currentTarget.transform.rotation = originalRotation;

        currentTarget = null;
    }

    private void OnDisable()
    {
        isSearching = false;
        gameStarted = false;

        StopAllCoroutines();

        UnsubscribeFromInteractables();

        RestoreCurrentTarget();
    }
}
