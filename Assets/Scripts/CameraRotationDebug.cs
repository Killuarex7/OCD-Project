
using UnityEngine;
using UnityEngine.UI;

public class CameraRotationDebug : MonoBehaviour
{
    private Vector3 lastRotation;
    private float timer = 0f;
    private bool hasPrinted = false;

    public float requiredTime = 5f;
    public float sensitivity = 0.1f;
    public Slider meter;

    public GameObject Player;

    [Header("UI Follow Camera")]
    public Transform vrCamera;
    public Vector3 cameraOffset = new Vector3(0f, 0f, 2f);
    public float followSpeed = 5f;

    void Start()
    {
        lastRotation = Player.transform.eulerAngles;
    }

    void Update()
    {
        // Make the UI follow the VR camera
        if (vrCamera != null)
        {
            Vector3 targetPosition =
                vrCamera.position + vrCamera.TransformDirection(cameraOffset);

            transform.position = Vector3.Lerp(
                transform.position,
                targetPosition,
                followSpeed * Time.deltaTime
            );

            transform.rotation = Quaternion.LookRotation(
                transform.position - vrCamera.position
            );
        }

        Vector3 currentRotation = Player.transform.eulerAngles;

        // Check if camera rotation has changed
        if (Vector3.Distance(currentRotation, lastRotation) > sensitivity)
        {
            timer = 0f;
            meter.value = 0f;
            hasPrinted = false;

            lastRotation = currentRotation;
        }
        else
        {
            timer += Time.deltaTime;
            meter.value = Mathf.Clamp01(timer / requiredTime);

            if (timer >= requiredTime && !hasPrinted)
            {
                Debug.Log("Camera has not moved for 5 seconds!");
                hasPrinted = true;
            }
        }
    }
}
