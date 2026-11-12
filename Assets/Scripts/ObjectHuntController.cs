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

    private ObjectHuntTarget currentTarget;

    private List<ObjectHuntTarget> remainingTargets =
        new List<ObjectHuntTarget>();

    private int roundsCompleted = 0;
    private int roundsToWin = 3;
    private bool canSelect = true;

    private void OnEnable()
    {
        StartGame();
    }

    public void StartGame()
    {
        StopAllCoroutines();

        roundsCompleted = 0;
        canSelect = true;

        remainingTargets.Clear();

        foreach (ObjectHuntTarget target in huntTargets)
        {
            if (target != null)
            {
                remainingTargets.Add(target);
            }
        }

        if (remainingTargets.Count < roundsToWin)
        {
            Debug.LogError("Add at least 3 hunt targets!");
            return;
        }

        feedbackText.text = "";
        ChooseNextTarget();
    }

    private void ChooseNextTarget()
    {
        if (remainingTargets.Count == 0)
        {
            FinishGame();
            return;
        }

        int randomIndex = Random.Range(0, remainingTargets.Count);

        currentTarget = remainingTargets[randomIndex];

        remainingTargets.RemoveAt(randomIndex);

        targetText.text = "Find: " + currentTarget.objectName;
        feedbackText.text = "Look around the room!";

        canSelect = true;
    }

    public void CheckObject(ObjectHuntTarget selectedObject)
    {
        if (!canSelect || currentTarget == null)
            return;

        AudioManager audio =
            FindFirstObjectByType<AudioManager>();

        if (audio != null)
            audio.PlayClick();

        if (selectedObject == currentTarget)
        {
            canSelect = false;

            roundsCompleted++;

            feedbackText.text = "Correct! Well done!";

            StartCoroutine(NextRound());
        }
        else
        {
            feedbackText.text = "Not quite! Keep looking.";
        }
    }

    private IEnumerator NextRound()
    {
        yield return new WaitForSeconds(1.5f);

        if (roundsCompleted >= roundsToWin)
        {
            FinishGame();
        }
        else
        {
            ChooseNextTarget();
        }
    }

    private void FinishGame()
    {
        canSelect = false;

        targetText.text = "Game Complete!";
        feedbackText.text =
            "Congratulations! You found all the objects!";

        AudioManager audio =
            FindFirstObjectByType<AudioManager>();

        if (audio != null)
            audio.PlayWin();

        StartCoroutine(CloseGame());
    }

    private IEnumerator CloseGame()
    {
        yield return new WaitForSeconds(3f);

        gameObject.SetActive(false);
    }
}