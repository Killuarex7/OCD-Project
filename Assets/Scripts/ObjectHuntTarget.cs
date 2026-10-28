
using UnityEngine;

public class ObjectHuntTarget : MonoBehaviour
{
    public string objectName = "Sofa";

    public void SelectTarget()
    {
        ObjectHuntController controller =
            FindFirstObjectByType<ObjectHuntController>();

        if (controller != null)
        {
            controller.CheckObject(this);
        }
    }

    // Allows mouse testing in the Unity Editor.
    void OnMouseDown()
    {
        SelectTarget();
    }
}