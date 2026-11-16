
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

public class MemoryMatchController : MonoBehaviour
{
    [Header("Assign Card1 to Card8")]
    public Button[] cardButtons;

    [Header("Card Pictures (4 images, each used for one pair)")]
    public Sprite[] cardFaces;
    [Tooltip("Gap between the picture and the edge of the card.")]
    public float facePadding = 4f;

    [Header("Assign Instruction Text")]
    public TMP_Text statusText;

    [Header("End Game")]
    public GameObject congratulationsText;
    public float endScreenDuration = 3f;

    [Header("Game Completion")]
    public ResetPopupController resetPopupController;

    // Which picture (0-3) is under each card. Two cards share each number.
    private int[] pairIds = { 0, 0, 1, 1, 2, 2, 3, 3 };

    private TMP_Text[] labels;   // the "?" shown on the back of each card
    private Image[] faces;       // the picture shown when a card is flipped
    private bool[] matched;

    private int firstCard = -1;
    private int secondCard = -1;
    private int matchesFound = 0;

    private bool waiting = false;
    private bool gameFinished = false;

    // Runs every time the Memory Match panel is opened.
    private void OnEnable()
    {
        if (cardButtons == null || cardButtons.Length != 8)
        {
            Debug.LogError("Please assign all 8 card buttons.");
            return;
        }

        if (cardFaces == null || cardFaces.Length < 4)
        {
            Debug.LogError("Memory Match: assign 4 images in Card Faces.");
            return;
        }

        CreateCardLabels();
        ResetGame();
        ShowPlayScreen();
    }

    private void OnDisable()
    {
        StopAllCoroutines();
    }

    // The card labels are created once and reused.
    private void CreateCardLabels()
    {
        if (labels != null)
            return;

        labels = new TMP_Text[8];
        faces = new Image[8];

        for (int i = 0; i < 8; i++)
        {
            int index = i;

            // PICTURE (hidden until the card is flipped)
            GameObject faceObject = new GameObject(
                "CardPicture",
                typeof(RectTransform),
                typeof(CanvasRenderer),
                typeof(Image)
            );

            faceObject.transform.SetParent(
                cardButtons[i].transform, false);

            RectTransform faceRect =
                faceObject.GetComponent<RectTransform>();

            faceRect.anchorMin = Vector2.zero;
            faceRect.anchorMax = Vector2.one;
            faceRect.offsetMin = new Vector2(facePadding, facePadding);
            faceRect.offsetMax = new Vector2(-facePadding, -facePadding);

            faces[i] = faceObject.GetComponent<Image>();
            faces[i].preserveAspect = true;
            faces[i].raycastTarget = false;
            faces[i].enabled = false;

            // "?" ON THE BACK OF THE CARD
            GameObject labelObject = new GameObject(
                "CardSymbol",
                typeof(RectTransform),
                typeof(CanvasRenderer),
                typeof(TextMeshProUGUI)
            );

            labelObject.transform.SetParent(
                cardButtons[i].transform, false);

            RectTransform rect =
                labelObject.GetComponent<RectTransform>();

            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;

            labels[i] = labelObject.GetComponent<TextMeshProUGUI>();
            labels[i].font = TMP_Settings.defaultFontAsset;
            labels[i].fontSize = 48;
            labels[i].alignment = TextAlignmentOptions.Center;
            labels[i].color = Color.black;
            labels[i].raycastTarget = false;

            cardButtons[i].onClick.RemoveAllListeners();

            cardButtons[i].onClick.AddListener(
                () => FlipCard(index));
        }
    }

    private void ResetGame()
    {
        matched = new bool[8];
        firstCard = -1;
        secondCard = -1;
        matchesFound = 0;
        waiting = false;
        gameFinished = false;

        ShufflePairs();

        for (int i = 0; i < 8; i++)
            ShowCardBack(i);

        if (statusText != null)
            statusText.text = "Find all 4 matching pairs!";
    }

    // Everything inside the panel (heading, instructions, cards) is shown
    // on top of the blue panel; only the congratulations text is hidden.
    private void ShowPlayScreen()
    {
        foreach (Transform child in transform)
            child.gameObject.SetActive(child.gameObject != congratulationsText);

        foreach (Button card in cardButtons)
        {
            if (card != null)
                card.gameObject.SetActive(true);
        }

        if (statusText != null)
            statusText.gameObject.SetActive(true);

        if (congratulationsText != null)
            congratulationsText.SetActive(false);
    }

    // When the game is won, only the congratulations text stays on the panel.
    private void ShowCongratulationsScreen()
    {
        foreach (Transform child in transform)
            child.gameObject.SetActive(child.gameObject == congratulationsText);

        if (congratulationsText != null)
        {
            congratulationsText.SetActive(true);
            congratulationsText.transform.SetAsLastSibling();
        }
    }

    void ShufflePairs()
    {
        for (int i = pairIds.Length - 1; i > 0; i--)
        {
            int j = Random.Range(0, i + 1);

            int temp = pairIds[i];
            pairIds[i] = pairIds[j];
            pairIds[j] = temp;
        }
    }

    void ShowCardFace(int index)
    {
        labels[index].text = "";
        faces[index].sprite = cardFaces[pairIds[index]];
        faces[index].enabled = true;
    }

    void ShowCardBack(int index)
    {
        labels[index].text = "?";
        faces[index].enabled = false;
    }

    void FlipCard(int index)
    {
        if (waiting || gameFinished || matched[index])
            return;

        if (index == firstCard)
            return;

        AudioManager audio =
            FindFirstObjectByType<AudioManager>();

        if (audio != null)
            audio.PlayClick();

        ShowCardFace(index);

        if (firstCard == -1)
        {
            firstCard = index;
            return;
        }

        secondCard = index;

        if (pairIds[firstCard] == pairIds[secondCard])
        {
            matched[firstCard] = true;
            matched[secondCard] = true;

            matchesFound++;

            firstCard = -1;
            secondCard = -1;

            if (matchesFound == 4)
            {
                StartCoroutine(EndGame());
            }
            else if (statusText != null)
            {
                statusText.text =
                    "Pairs found: " + matchesFound + "/4";
            }
        }
        else
        {
            StartCoroutine(HideCards());
        }
    }

    IEnumerator HideCards()
    {
        waiting = true;

        if (statusText != null)
            statusText.text = "Try again!";

        yield return new WaitForSeconds(1f);

        ShowCardBack(firstCard);
        ShowCardBack(secondCard);

        firstCard = -1;
        secondCard = -1;

        waiting = false;

        if (statusText != null)
            statusText.text = "Find all 4 matching pairs!";
    }

    IEnumerator EndGame()
    {
        gameFinished = true;

        AudioManager audio =
            FindFirstObjectByType<AudioManager>();

        if (audio != null)
            audio.PlayWin();

        ShowCongratulationsScreen();

        yield return new WaitForSeconds(endScreenDuration);

        // REPORT COMPLETION TO RESET POPUP CONTROLLER
        if (resetPopupController != null)
        {
            resetPopupController.CompleteMemoryMatch();
        }
        else
        {
            Debug.LogError(
                "Reset Popup Controller is not assigned! " +
                "Memory Match cannot report its completion."
            );

            gameObject.SetActive(false);
        }
    }
}
