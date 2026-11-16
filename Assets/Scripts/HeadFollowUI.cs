using UnityEngine;

// Keeps a UI panel floating in front of the player's head.
// When a wall or object is too close, the panel fades out (via a CanvasGroup
// added automatically) instead of being disabled, so this script keeps running.
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

    private CanvasGroup canvasGroup;
    private bool isShown = true;

    private void Awake()
    {
        canvasGroup = GetComponent<CanvasGroup>();

        if (canvasGroup == null)
            canvasGroup = gameObject.AddComponent<CanvasGroup>();
    }

    private void OnEnable()
    {
        SetShown(true);
    }

    private void LateUpdate()
    {
        if (vrCamera == null) return;

        float targetDistance = desiredDistance;

        if (Physics.Raycast(vrCamera.position, vrCamera.forward,
            out RaycastHit hit, desiredDistance, obstacleLayers,
            QueryTriggerInteraction.Ignore))
        {
            targetDistance = hit.distance - obstaclePadding;
        }

        SetShown(targetDistance >= minimumDistance);

        targetDistance = Mathf.Clamp(targetDistance, minimumDistance, desiredDistance);

        Vector3 targetPosition = vrCamera.position + vrCamera.forward * targetDistance;
        transform.position = Vector3.Lerp(transform.position, targetPosition, followSpeed * Time.deltaTime);
        transform.rotation = vrCamera.rotation;
    }

    private void SetShown(bool show)
    {
        if (canvasGroup == null || isShown == show)
            return;

        isShown = show;
        canvasGroup.alpha = show ? 1f : 0f;
        canvasGroup.interactable = show;
        canvasGroup.blocksRaycasts = show;
    }
}
