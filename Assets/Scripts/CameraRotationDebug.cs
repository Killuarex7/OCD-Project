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

    void Start()
    {
        lastRotation = transform.eulerAngles;
    }

    void Update()
    {
        Vector3 currentRotation = transform.eulerAngles;

        // Check if camera rotation has changed
        if (Vector3.Distance(currentRotation, lastRotation) > sensitivity)
        {
            // Camera moved, reset timer
            timer = 0f;
            meter.value = 0f;
            hasPrinted = false;

            lastRotation = currentRotation;
        }
        else
        {
            // Camera has not moved
            timer += Time.deltaTime;
            meter.value = timer / requiredTime;

            if (timer >= requiredTime && !hasPrinted)
            {
                Debug.Log("Camera has not moved for 5 seconds!");
                hasPrinted = true;
            }
        }
    }
}