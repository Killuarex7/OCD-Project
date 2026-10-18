
using UnityEngine;
using UnityEngine.UI;

public class ResetPopupController : MonoBehaviour
{
    [Header("Meter")]
    public Slider ocdMeter;

    [Header("Popup")]
    public GameObject resetPopup;

    [Header("Game Options")]
    public GameObject objectHunt;
    public GameObject memoryMatch;

    [Header("Thresholds")]
    [Range(0f, 1f)]
    public float triggerThreshold = 0.7f;

    [Range(0f, 1f)]
    public float resetThreshold = 0.4f;

    private bool popupShown = false;

    void Start()
    {
        if (resetPopup != null)
            resetPopup.SetActive(false);

        if (objectHunt != null)
            objectHunt.SetActive(false);

        if (memoryMatch != null)
            memoryMatch.SetActive(false);
    }

    void Update()
    {
        if (ocdMeter == null || resetPopup == null)
            return;

        if (!popupShown &&
            ocdMeter.value >= triggerThreshold)
        {
            resetPopup.SetActive(true);
            popupShown = true;
        }

        if (popupShown &&
            ocdMeter.value <= resetThreshold)
        {
            popupShown = false;
        }
    }

    public void PlayObjectHunt()
    {
        OpenGame(objectHunt, "Object Hunt");
    }

    public void PlayMemoryMatch()
    {
        OpenGame(memoryMatch, "Memory Match");
    }

    public void ContinueExperience()
    {
        if (resetPopup != null)
            resetPopup.SetActive(false);
    }

    void OpenGame(GameObject game, string gameName)
    {
        if (resetPopup != null)
            resetPopup.SetActive(false);

        if (game != null)
            game.SetActive(true);

        Debug.Log("Selected game: " + gameName);
    }
}