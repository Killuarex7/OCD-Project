using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

public class MemoryMatchController : MonoBehaviour
{
    [Header("Assign Card1 to Card8")]
    public Button[] cardButtons;

    [Header("Assign Instruction Text")]
    public TMP_Text statusText;

    private string[] symbols =
    {
        "★", "●", "▲", "♥",
        "★", "●", "▲", "♥"
    };

    private TMP_Text[] labels;
    private bool[] matched;

    private int firstCard = -1;
    private int secondCard = -1;
    private int matchesFound = 0;
    private bool waiting = false;

    void Start()
    {
        if (cardButtons == null || cardButtons.Length != 8)
        {
            Debug.LogError("Please assign all 8 card buttons.");
            return;
        }

        labels = new TMP_Text[8];
        matched = new bool[8];

        ShuffleSymbols();

        for (int i = 0; i < 8; i++)
        {
            int index = i;

            // Create a symbol label on each card.
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

            labels[i].text = "?";

            cardButtons[i].onClick.RemoveAllListeners();
            cardButtons[i].onClick.AddListener(
                () => FlipCard(index));
        }

        if (statusText != null)
            statusText.text = "Find all 4 matching pairs!";
    }

    void ShuffleSymbols()
    {
        for (int i = symbols.Length - 1; i > 0; i--)
        {
            int j = Random.Range(0, i + 1);

            string temp = symbols[i];
            symbols[i] = symbols[j];
            symbols[j] = temp;
        }
    }

    void FlipCard(int index)
    {
        if (waiting || matched[index])
            return;

        if (index == firstCard)
            return;

        labels[index].text = symbols[index];

        if (firstCard == -1)
        {
            firstCard = index;
            return;
        }

        secondCard = index;

        if (symbols[firstCard] == symbols[secondCard])
        {
            matched[firstCard] = true;
            matched[secondCard] = true;
            matchesFound++;

            firstCard = -1;
            secondCard = -1;

            if (matchesFound == 4)
            {
                if (statusText != null)
                    statusText.text =
                        "Well done! You found all the pairs!";
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

        labels[firstCard].text = "?";
        labels[secondCard].text = "?";

        firstCard = -1;
        secondCard = -1;
        waiting = false;

        if (statusText != null)
            statusText.text = "Find all 4 matching pairs!";
    }
}