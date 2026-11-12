using UnityEngine;

public class ObjectHuntTarget : MonoBehaviour
{
    public string objectName;

    public void SelectTarget()
    {
        ObjectHuntController controller =
            FindFirstObjectByType<ObjectHuntController>();

        if (controller != null)
        {
            controller.CheckObject(this);
        }
    }

    private void OnMouseDown()
    {
        SelectTarget();
    }
}