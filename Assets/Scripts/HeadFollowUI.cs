
using UnityEngine;

public class HeadFollowUI : MonoBehaviour
{
    [Header("VR Camera")]
    public Transform vrCamera;

    [Header("UI Position")]
    public float desiredDistance = 2f;
    public float minimumDistance = 0.6f;
    public float obstaclePadding = 0.15f;

    [Header("Movement")]
    public float followSpeed = 8f;

    [Header("Obstacle Detection")]
    public LayerMask obstacleLayers = ~0;

    private void LateUpdate()
    {
        if (vrCamera == null)
            return;

        float targetDistance = desiredDistance;

        // Detect walls and objects in front of the camera.
        if (Physics.Raycast(
            vrCamera.position,
            vrCamera.forward,
            out RaycastHit hit,
            desiredDistance,
            obstacleLayers))
        {
            targetDistance = hit.distance - obstaclePadding;
        }

        // Hide the UI if an obstacle is too close.
        bool shouldShow = targetDistance >= minimumDistance;

        if (gameObject.activeSelf != shouldShow)
            gameObject.SetActive(shouldShow);

        if (!shouldShow)
            return;

        targetDistance = Mathf.Min(targetDistance, desiredDistance);

        Vector3 targetPosition =
            vrCamera.position + vrCamera.forward * targetDistance;

        transform.position = Vector3.Lerp(
            transform.position,
            targetPosition,
            followSpeed * Time.deltaTime
        );

        transform.rotation = vrCamera.rotation;
    }
}
