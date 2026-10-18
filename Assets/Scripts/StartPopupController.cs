using UnityEngine;

public class StartPopupController : MonoBehaviour
{
    public GameObject startPopup;

    public void StartChallenge()
    {
        startPopup.SetActive(false);
    }
}
