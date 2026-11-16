
using UnityEngine;
using TMPro;
using System.Collections;
using System.Collections.Generic;

public class ObjectHuntController : MonoBehaviour
{
    [Header("UI Elements")]
    public TMP_Text targetText;
    public TMP_Text feedbackText;

    [Header("Objects to Find")]
    public ObjectHuntTarget[] huntTargets;

    [Header("Hiding Locations")]
    public Transform[] hideSpots;

    [Header("VR Gaze Detection")]
    public Transform vrCamera;
    public float gazeDistance = 10f;

    [Header("Game Settings")]
    public float memoryDuration = 5f;
    public float searchDuration = 20f;
    public float nextRoundDelay = 2f;
    public int roundsToWin = 3;

    private ObjectHuntTarget currentTarget;
    private List<ObjectHuntTarget> remainingTargets =
        new List<ObjectHuntTarget>();

    private Vector3 originalPosition;
    private Quaternion originalRotation;
    private Transform originalParent;

    private bool isSearching;
    private bool lookedAtTarget;
    private bool roundWon;

    private void OnEnable()
    {
        StartGame();
    }

    private void Update()
    {
        if (!isSearching || currentTarget == null || vrCamera == null)
            return;

        Ray gazeRay = new Ray(vrCamera.position, vrCamera.forward);

        if (Physics.Raycast(
            gazeRay,
            out RaycastHit hit,
            gazeDistance,
            Physics.DefaultRaycastLayers,
            QueryTriggerInteraction.Ignore))
        {
            ObjectHuntTarget seenTarget =
                hit.collider.GetComponentInParent<ObjectHuntTarget>();

            if (seenTarget == currentTarget)
            {
                lookedAtTarget = true;
            }
        }
    }

    public void StartGame()
    {
        StopAllCoroutines();
        RestoreCurrentTarget();

        remainingTargets.Clear();

        foreach (ObjectHuntTarget target in huntTargets)
        {
            if (target != null)
                remainingTargets.Add(target);
        }

        if (targetText == null || feedbackText == null)
        {
            Debug.LogError("Assign the Object Hunt UI Text fields!");
            return;
        }

        if (vrCamera == null)
        {
            Debug.LogError("Assign the VR Main Camera!");
            return;
        }

        if (hideSpots == null || hideSpots.Length == 0)
        {
            Debug.LogError("Assign your hiding spots!");
            return;
        }

        if (remainingTargets.Count < roundsToWin)
        {
            Debug.LogError("Not enough hunt targets!");
            return;
        }

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

            if (hideSpot == null)
            {
                Debug.LogError("A hiding spot is missing!");
                yield break;
            }

            // MOVE THE OBJECT
            currentTarget.transform.position = hideSpot.position;
            currentTarget.transform.rotation = hideSpot.rotation;

            // SEARCH PHASE
            lookedAtTarget = false;
            roundWon = false;
            isSearching = true;

            feedbackText.text = "Look around and find what is missing!";

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

        targetText.text = "Game Complete!";
        feedbackText.text = "You completed the Object Hunt!";

        AudioManager audio =
            FindFirstObjectByType<AudioManager>();

        if (audio != null)
            audio.PlayWin();

        yield return new WaitForSeconds(3f);

        gameObject.SetActive(false);
    }

    // Connect this method to the I Found It! UI button.
    public void ConfirmFound()
    {
        if (!isSearching)
            return;

        if (lookedAtTarget)
        {
            roundWon = true;

            AudioManager audio =
                FindFirstObjectByType<AudioManager>();

            if (audio != null)
                audio.PlayClick();
        }
        else
        {
            feedbackText.text =
                "Not yet! Look directly at the missing object first.";
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
        StopAllCoroutines();
        RestoreCurrentTarget();
    }
}
