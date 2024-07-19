using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class SlotMachine : MonoBehaviour
{
    public int rows = 3;
    public int columns = 5;
    public Sprite[] slotSymbols;

    // Parâmetros para tamanho e espaçamento
    public Vector2 slotSize = new Vector2(100, 100);
    public Vector2 slotSpacing = new Vector2(10, 10);
    public float initialSpinSpeed = 1000f;
    public float deceleration = 500f;
    public float magnetStrength = 10f; // Força do "ímã"

    private RectTransform[,] slots;
    private bool isSpinning = false;

    public Button spinButton; // Referência ao botão de giro na UI

    void Start()
    {
        InitializeSlots();
        CenterSlots();
    }

    void InitializeSlots()
    {
        slots = new RectTransform[rows, columns];

        for (int row = 0; row < rows; row++)
        {
            for (int column = 0; column < columns; column++)
            {
                GameObject slot = new GameObject("Slot" + row + column);
                slot.transform.SetParent(this.transform);

                Image slotImage = slot.AddComponent<Image>();
                slotImage.sprite = slotSymbols[Random.Range(0, slotSymbols.Length)];

                RectTransform rectTransform = slot.GetComponent<RectTransform>();
                rectTransform.localPosition = new Vector3(column * (slotSize.x + slotSpacing.x), -row * (slotSize.y + slotSpacing.y), 0);
                rectTransform.sizeDelta = slotSize;

                slots[row, column] = rectTransform;
            }
        }
    }

    void CenterSlots()
    {
        RectTransform rectTransform = GetComponent<RectTransform>();
        float totalWidth = columns * slotSize.x + (columns - 1) * slotSpacing.x;
        float totalHeight = rows * slotSize.y + (rows - 1) * slotSpacing.y;
        rectTransform.localPosition = new Vector3(-totalWidth / 2 + slotSize.x / 2, totalHeight / 2 - slotSize.y / 2, 0);
    }

    public void StartSpin()
    {
        if (!isSpinning)
        {
            isSpinning = true;
            spinButton.interactable = false; // Desabilita o botão de giro durante o giro

            for (int column = 0; column < columns; column++)
            {
                StartCoroutine(SpinColumn(column, column * 0.5f));
            }
        }
    }

    IEnumerator SpinColumn(int column, float delay)
    {
        yield return new WaitForSeconds(delay);
        float speed = initialSpinSpeed;

        while (speed > 0)
        {
            for (int row = 0; row < rows; row++)
            {
                Vector3 newPos = slots[row, column].localPosition;
                newPos.y -= speed * Time.deltaTime;
                slots[row, column].localPosition = newPos;

                // Verifica se o símbolo está completamente fora da área visível
                if (slots[row, column].localPosition.y < -rows * (slotSize.y + slotSpacing.y))
                {
                    newPos.y += rows * (slotSize.y + slotSpacing.y);
                    slots[row, column].localPosition = newPos;
                    slots[row, column].GetComponent<Image>().sprite = slotSymbols[Random.Range(0, slotSymbols.Length)];
                }
            }

            speed -= deceleration * Time.deltaTime;
            yield return null;
        }

        // Ativa o efeito de "ímã" para mover suavemente os símbolos para suas posições finais
        StartCoroutine(MagnetizeSymbols(column));

        if (column == columns - 1)
        {
            isSpinning = false;
            spinButton.interactable = true; // Habilita o botão de giro após o término do giro
        }
    }

    IEnumerator MagnetizeSymbols(int column)
    {
        float duration = 0.5f; // Duração da animação de "ímã"
        float elapsedTime = 0f;

        float finalYPosition = -(rows - 1) * (slotSize.y + slotSpacing.y);
        Vector3[] initialPositions = new Vector3[rows];

        for (int row = 0; row < rows; row++)
        {
            initialPositions[row] = slots[row, column].localPosition;
        }

        while (elapsedTime < duration)
        {
            elapsedTime += Time.deltaTime;

            for (int row = 0; row < rows; row++)
            {
                Vector3 targetPos = new Vector3(column * (slotSize.x + slotSpacing.x), finalYPosition + row * (slotSize.y + slotSpacing.y), 0);
                Vector3 currentPos = slots[row, column].localPosition;

                // Utiliza uma interpolação suave para mover os símbolos para suas posições finais
                slots[row, column].localPosition = Vector3.Lerp(initialPositions[row], targetPos, elapsedTime / duration);
            }

            yield return null;
        }

        // Garante que os símbolos estão nas posições finais exatas
        for (int row = 0; row < rows; row++)
        {
            slots[row, column].localPosition = new Vector3(column * (slotSize.x + slotSpacing.x), finalYPosition + row * (slotSize.y + slotSpacing.y), 0);
        }
    }
}
