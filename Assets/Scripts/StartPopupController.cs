
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

    [Header("VR Stillness Settings")]
    public Transform vrCamera;
    public float stillnessDuration = 5f;
    public float positionMovementThreshold = 0.02f;
    public float rotationMovementThreshold = 1f;

    private Vector3 lastPosition;
    private Quaternion lastRotation;
    private float stillnessTimer;
    private bool startPopupTriggered;

    private void Start()
    {
        // The Start Popup must begin hidden.
        if (startPopup != null)
            startPopup.SetActive(false);

        // Use the Main Camera if no camera was assigned.
        if (vrCamera == null && Camera.main != null)
            vrCamera = Camera.main.transform;

        if (vrCamera != null)
        {
            lastPosition = vrCamera.position;
            lastRotation = vrCamera.rotation;
        }
        else
        {
            Debug.LogWarning(
                "StartPopupController: VR Camera is not assigned."
            );
        }

        if (ocdMeter == null)
        {
            Debug.LogWarning(
                "StartPopupController: OCD Meter is not assigned."
            );
        }
    }

    private void Update()
    {
        if (startPopupTriggered ||
            startPopup == null ||
            ocdMeter == null ||
            vrCamera == null)
        {
            return;
        }

        // Don't trigger the Start Popup during a mini-game.
        if (resetPopupController != null &&
            resetPopupController.IsMiniGameActive)
        {
            stillnessTimer = 0f;
            UpdateCameraPose();
            return;
        }

        bool cameraMoved =
            Vector3.Distance(vrCamera.position, lastPosition)
                > positionMovementThreshold
            ||
            Quaternion.Angle(vrCamera.rotation, lastRotation)
                > rotationMovementThreshold;

        UpdateCameraPose();

        if (cameraMoved)
        {
            // Any meaningful head/camera movement resets the timer.
            stillnessTimer = 0f;
            return;
        }

        stillnessTimer += Time.deltaTime;

        // Both conditions must be satisfied.
        if (stillnessTimer >= stillnessDuration &&
            ocdMeter.value >= triggerThreshold)
        {
            startPopup.SetActive(true);
            startPopupTriggered = true;

            Debug.Log("Start Popup triggered by stillness and meter threshold.");
        }
    }

    private void UpdateCameraPose()
    {
        lastPosition = vrCamera.position;
        lastRotation = vrCamera.rotation;
    }

    // Connect this method to the Start Popup's Okay button.
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
                "StartPopupController: Reset Popup Controller is not assigned."
            );
        }
    }
}
