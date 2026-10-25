
using UnityEngine;
using UnityEngine.UI;

public class ResetPopupController : MonoBehaviour
{
    [Header("Meter")]
    public Slider ocdMeter;

    [Header("Popup")]
    public GameObject resetPopup;

    [Header("Game Options")]
    public GameObject objectHunt;
    public GameObject memoryMatch;

    // Tracks whether a mini-game is currently running.
    public bool IsMiniGameActive { get; private set; } = false;

    private void Start()
    {
        // The Reset Popup should not appear automatically.
        if (resetPopup != null)
            resetPopup.SetActive(false);

        if (objectHunt != null)
            objectHunt.SetActive(false);

        if (memoryMatch != null)
            memoryMatch.SetActive(false);
    }

    // Called by StartPopupController after Okay is clicked.
    public void ShowResetPopup()
    {
        if (IsMiniGameActive)
            return;

        if (resetPopup != null)
            resetPopup.SetActive(true);
    }

    public void PlayObjectHunt()
    {
        OpenGame(objectHunt, "Object Hunt");
    }

    public void PlayMemoryMatch()
    {
        OpenGame(memoryMatch, "Memory Match");
    }

    public void ContinueExperience()
    {
        if (resetPopup != null)
            resetPopup.SetActive(false);
    }

    private void OpenGame(GameObject game, string gameName)
    {
        if (game == null)
        {
            Debug.LogWarning(gameName + " is not assigned!");
            return;
        }

        // Prevent the Reset Popup from reopening during gameplay.
        IsMiniGameActive = true;

        if (resetPopup != null)
            resetPopup.SetActive(false);

        if (objectHunt != null)
            objectHunt.SetActive(false);

        if (memoryMatch != null)
            memoryMatch.SetActive(false);

        game.SetActive(true);

        Debug.Log("Selected game: " + gameName);
    }

    // Call this when a mini-game is completed or exited.
    public void EndMiniGame()
    {
        if (objectHunt != null)
            objectHunt.SetActive(false);

        if (memoryMatch != null)
            memoryMatch.SetActive(false);

        IsMiniGameActive = false;
    }
}
