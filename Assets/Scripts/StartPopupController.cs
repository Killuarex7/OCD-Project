
using UnityEngine;
using UnityEngine.UI;

public class StartPopupController : MonoBehaviour
{
    [Header("Popup References")]
    public GameObject startPopup;
    public ResetPopupController resetPopupController;

    [Header("Anxiety Meter")]
    public Slider ocdMeter;

    [Range(0f, 1f)]
    public float triggerThreshold = 0.7f;

    private bool startPopupTriggered = false;

    void Start()
    {
        if (startPopup != null)
            startPopup.SetActive(false);
    }

    void Update()
    {
        if (startPopupTriggered ||
            startPopup == null ||
            ocdMeter == null)
            return;

        // Don't show the popup during a mini-game.
        if (resetPopupController != null &&
            resetPopupController.IsMiniGameActive)
            return;

        // Show the popup when the meter reaches the threshold.
        if (ocdMeter.value >= triggerThreshold)
        {
            startPopup.SetActive(true);
            startPopupTriggered = true;

            Debug.Log("Start Popup triggered!");
        }
    }

    public void StartChallenge()
    {
        if (startPopup != null)
            startPopup.SetActive(false);

        if (resetPopupController != null)
        {
            resetPopupController.ShowResetPopup();
        }
        else
        {
            Debug.LogWarning(
                "Reset Popup Controller is not assigned!"
            );
        }
    }
}
