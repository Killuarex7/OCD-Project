
using UnityEngine;
using UnityEngine.UI;

public class ResetPopupController : MonoBehaviour
{
    [Header("Anxiety Meter")]
    public Slider ocdMeter;
    [Range(0f, 1f)]
    public float triggerThreshold = 1f;

    [Header("Popups")]
    public GameObject resetPopup;
    public GameObject endingPopup;

    [Header("Ending Popup Screens")]
    [Tooltip("Shown first: 'Experience Complete!' image and its Continue button.")]
    public GameObject[] endingCompleteScreen;
    [Tooltip("Shown after Continue: summary card and its End Experience button.")]
    public GameObject[] endingSummaryScreen;

    [Header("Mini Games")]
    public GameObject objectHunt;
    public GameObject memoryMatch;

    [Header("Mini Game Buttons")]
    public Button objectHuntButton;
    public Button memoryMatchButton;

    [Header("Game State")]
    public bool IsMiniGameActive { get; private set; } = false;

    private bool objectHuntCompleted = false;
    private bool memoryMatchCompleted = false;

    private bool experienceStarted = false;
    private bool meterArmed = false;

    private void Start()
    {
        // Hide all popups and mini-games at the beginning.
        if (resetPopup != null)
            resetPopup.SetActive(false);

        if (endingPopup != null)
            endingPopup.SetActive(false);

        if (objectHunt != null)
            objectHunt.SetActive(false);

        if (memoryMatch != null)
            memoryMatch.SetActive(false);

        UpdateGameButtons();
    }

    private void Update()
    {
        // Only monitor the meter after the experience has started.
        if (!experienceStarted)
            return;

        // Do not trigger popups while a mini-game is running.
        if (IsMiniGameActive)
            return;

        // Do not trigger anything after both games are completed.
        if (objectHuntCompleted && memoryMatchCompleted)
            return;

        if (ocdMeter == null)
            return;

        // The meter must first drop below the threshold.
        if (ocdMeter.value < triggerThreshold)
        {
            meterArmed = true;
            return;
        }

        // When it fills again, show the Reset Popup.
        if (ocdMeter.value >= triggerThreshold && meterArmed)
        {
            meterArmed = false;
            ShowResetPopup();
        }
    }

    // Called by StartPopupController when the player starts the challenge.
    public void ShowResetPopup()
    {
        if (objectHuntCompleted && memoryMatchCompleted)
        {
            ShowEndingPopup();
            return;
        }

        if (resetPopup != null)
            resetPopup.SetActive(true);

        experienceStarted = true;
        meterArmed = false;

        UpdateGameButtons();
    }

    // Called by the Object Hunt button.
    public void PlayObjectHunt()
    {
        if (objectHuntCompleted)
            return;

        OpenGame(objectHunt);
    }

    // Called by the Memory Match button.
    public void PlayMemoryMatch()
    {
        if (memoryMatchCompleted)
            return;

        OpenGame(memoryMatch);
    }

    private void OpenGame(GameObject selectedGame)
    {
        if (selectedGame == null)
        {
            Debug.LogError("Selected mini-game is not assigned in ResetPopupController.");
            return;
        }

        IsMiniGameActive = true;
        meterArmed = false;

        if (resetPopup != null)
            resetPopup.SetActive(false);

        if (objectHunt != null)
            objectHunt.SetActive(false);

        if (memoryMatch != null)
            memoryMatch.SetActive(false);

        selectedGame.SetActive(true);

        UpdateGameButtons();
    }

    // Called when the player completes Object Hunt or Memory Match.
    private void FinishMiniGame()
    {
        IsMiniGameActive = false;

        if (objectHunt != null)
            objectHunt.SetActive(false);

        if (memoryMatch != null)
            memoryMatch.SetActive(false);

        UpdateGameButtons();

        // If both games are complete, show the final ending popup.
        if (objectHuntCompleted && memoryMatchCompleted)
        {
            ShowEndingPopup();
        }
        else
        {
            // Do NOT immediately show the Reset Popup.
            // Wait until the anxiety meter drops and fills again.
            meterArmed = false;
        }
    }

    // Called by ObjectHuntController after completing all rounds.
    public void CompleteObjectHunt()
    {
        if (objectHuntCompleted)
            return;

        objectHuntCompleted = true;

        Debug.Log("Object Hunt completed!");

        FinishMiniGame();
    }

    // Called by MemoryMatchController after completing all pairs.
    public void CompleteMemoryMatch()
    {
        if (memoryMatchCompleted)
            return;

        memoryMatchCompleted = true;

        Debug.Log("Memory Match completed!");

        FinishMiniGame();
    }

    private void UpdateGameButtons()
    {
        if (objectHuntButton != null)
            objectHuntButton.interactable = !objectHuntCompleted;

        if (memoryMatchButton != null)
            memoryMatchButton.interactable = !memoryMatchCompleted;
    }

    // Called by the Continue button on the Reset Popup.
    public void ContinueExperience()
    {
        if (resetPopup != null)
            resetPopup.SetActive(false);

        meterArmed = false;
    }

    private void ShowEndingPopup()
    {
        IsMiniGameActive = false;
        meterArmed = false;

        if (resetPopup != null)
            resetPopup.SetActive(false);

        if (objectHunt != null)
            objectHunt.SetActive(false);

        if (memoryMatch != null)
            memoryMatch.SetActive(false);

        SetActiveAll(endingCompleteScreen, true);
        SetActiveAll(endingSummaryScreen, false);

        if (endingPopup != null)
        {
            endingPopup.SetActive(true);
            Debug.Log("Both mini-games completed. Experience finished!");
        }
        else
        {
            Debug.LogError("Ending Popup is not assigned in ResetPopupController.");
        }
    }

    // Called by the Continue button on the "Experience Complete!" screen.
    public void ShowEndingSummary()
    {
        PlayClick();

        SetActiveAll(endingCompleteScreen, false);
        SetActiveAll(endingSummaryScreen, true);
    }

    // Called by the "OK, let's end the experience now" button on the summary.
    public void EndExperience()
    {
        PlayClick();

        Debug.Log("Exiting OCD VR Experience.");

#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }

    private void PlayClick()
    {
        AudioManager audio = FindFirstObjectByType<AudioManager>();

        if (audio != null)
            audio.PlayClick();
    }

    private static void SetActiveAll(GameObject[] objects, bool active)
    {
        if (objects == null)
            return;

        foreach (GameObject obj in objects)
        {
            if (obj != null)
                obj.SetActive(active);
        }
    }
}
