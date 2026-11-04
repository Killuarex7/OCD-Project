
using UnityEngine;
using UnityEngine.UI;

public class CameraRotationDebug : MonoBehaviour
{
    [Header("Anxiety Meter")]
    public Slider meter;
    public float requiredTime = 5f;

    [Header("VR Movement Detection")]
    public Transform vrCamera;
    public float positionMovementThreshold = 0.02f;
    public float rotationMovementThreshold = 1f;

    [Header("UI Follow Camera")]
    public Vector3 cameraOffset = new Vector3(0f, 0f, 2f);
    public float followSpeed = 5f;

    private Vector3 lastPosition;
    private Quaternion lastRotation;
    private float timer = 0f;

    void Start()
    {
        if (vrCamera == null && Camera.main != null)
            vrCamera = Camera.main.transform;

        if (vrCamera != null)
        {
            lastPosition = vrCamera.position;
            lastRotation = vrCamera.rotation;
        }

        if (meter != null)
            meter.value = 0f;
    }

    void Update()
    {
        if (vrCamera == null)
            return;

        // Make the UI follow the VR camera.
        Vector3 targetPosition =
            vrCamera.position +
            vrCamera.TransformDirection(cameraOffset);

        transform.position = Vector3.Lerp(
            transform.position,
            targetPosition,
            followSpeed * Time.deltaTime
        );

        transform.rotation = Quaternion.LookRotation(
            transform.position - vrCamera.position
        );

        // Detect headset movement.
        bool cameraMoved =
            Vector3.Distance(vrCamera.position, lastPosition)
                > positionMovementThreshold
            ||
            Quaternion.Angle(vrCamera.rotation, lastRotation)
                > rotationMovementThreshold;

        lastPosition = vrCamera.position;
        lastRotation = vrCamera.rotation;

        if (cameraMoved)
        {
            timer = 0f;

            if (meter != null)
                meter.value = 0f;
        }
        else
        {
            timer += Time.deltaTime;

            if (meter != null)
            {
                meter.value = Mathf.Clamp01(
                    timer / Mathf.Max(requiredTime, 0.01f)
                );
            }
        }
    }
}
