using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class DialogCanvas : MonoBehaviour
{
    [SerializeField] private GameObject choiceButtonPrefab; // Префаб кнопки выбора
    [SerializeField] private Text characterNameText; // Текст имени персонажа
    [SerializeField] private Transform choicesPanel; // Панель для кнопок выбора
    [SerializeField] private Text dialogueText; // Текст диалога
    [SerializeField] private Image characterImage; // Изображение персонажа
    [SerializeField] private Image background;// Изображение фона

    private DialogueLine currentDialogueLine; // Текущая строка диалога
    private DialogSystem dialogue; // Система диалогов
    private int currentLineIndex = 0; // Индекс текущей строки диалога
    private bool isTextFullyDisplayed = false; // Флаг, указывающий, что текст полностью отображен
    private bool isShowingChoices = false; // Флаг, указывающий, что отображаются варианты выбора

    private CallingADialog thisCallingADialog;

    public void AddCalingADialog(CallingADialog callingADialog)
    {
        thisCallingADialog = callingADialog;
    }
    // Начало диалога
    public void StartDialogue(DialogSystem dialogues)
    {
        if (dialogues != null && dialogues.dialogueLines.Length > 0)
        {
            dialogue = dialogues;
            currentLineIndex = 0; // Сбрасываем индекс на начало
            currentDialogueLine = dialogue.dialogueLines[currentLineIndex];
            characterNameText.text = currentDialogueLine.character.nameNPC;
            Time.timeScale = 0f;
            ShowDialogueLine();
        }
        else
        {
            Debug.LogError("DialogSystem не был назначен или не содержит строк диалога.");
        }
    }

    private void Update()
    {
        if (Input.GetMouseButtonDown(0)) 
        {
            SwitchingDialogs();
        }
    }

    // Переход к следующей строке диалога
    private void GoToNextLine()
    {
        if (isTextFullyDisplayed && !isShowingChoices)
        {
            // Если текст полностью отображен и нет активных выборов, переходим к следующей строке
            currentLineIndex++;

            if (currentLineIndex < dialogue.dialogueLines.Length)
            {
                currentDialogueLine = dialogue.dialogueLines[currentLineIndex];
                if (currentDialogueLine.choices != null && currentDialogueLine.choices.Length > 0)
                {
                    CheckForChoices();
                }
                else
                {
                    ShowDialogueLine(); // Если нет выбора, показываем следующий диалог
                }
            }
            else
            {
                EndDialogue(); // Завершаем диалог
            }
        }
    }

    private void SwitchingDialogs()
    {
        GoToNextLine();
    }

    // Показ текущей строки диалога
    private void ShowDialogueLine()
    {
        foreach (Transform child in choicesPanel)
        {
            Destroy(child.gameObject); // Очистка старых кнопок выбора
        }

        isShowingChoices = false;
        isTextFullyDisplayed = false;

        if (dialogue != null)
        {
            if (currentLineIndex < dialogue.dialogueLines.Length)
            {
                currentDialogueLine = dialogue.dialogueLines[currentLineIndex];
                characterNameText.text = currentDialogueLine.character.nameNPC;
                characterImage.sprite = currentDialogueLine.character.sprites[currentDialogueLine.indexSprite];
                background.sprite = currentDialogueLine.background;
                StartCoroutine(DisplayTextWithDelay(currentDialogueLine.textDialogue)); // Запускаем корутину для по-буквенного вывода
            }
            else
            {
                EndDialogue(); // Завершаем диалог, если строки кончились
            }
        }
        else
        {
            Debug.LogError("DialogSystem не был назначен.");
        }
    }

    private IEnumerator DisplayTextWithDelay(string text)
    {
        isTextFullyDisplayed = false;
        dialogueText.text = "";
        foreach (char letter in text)
        {
            dialogueText.text += letter;
            yield return new WaitForSecondsRealtime(0.05f);
        }
        isTextFullyDisplayed = true; // Текст полностью отображен
        CheckForChoices(); // Проверка наличия выбора после вывода текста
    }

    private void CheckForChoices()
    {
        foreach (Transform child in choicesPanel)
        {
            Destroy(child.gameObject); // Очистка старых кнопок выбора
        }

        // Если есть варианты выбора
        if (currentDialogueLine.choices != null && currentDialogueLine.choices.Length > 0)
        {
            bool hasValidChoices = false;

            // Проверяем наличие валидных вариантов выбора
            foreach (DialogueChoice choice in currentDialogueLine.choices)
            {
                // Условие, когда нам нужно отобразить вариант
                if ((choice.nextLineIndex != -1 || !string.IsNullOrEmpty(choice.choiceText)))
                {
                    hasValidChoices = true;
                    break;
                }
            }

            // Если есть хотя бы один выбор
            if (hasValidChoices)
            {
                isShowingChoices = true;

                // Создаем кнопки для всех вариантов выбора
                foreach (DialogueChoice choice in currentDialogueLine.choices)
                {
                    // Создаем кнопки, если nextLineIndex не -1 или есть текст
                    if (choice.nextLineIndex != -1 || !string.IsNullOrEmpty(choice.choiceText))
                    {
                        GameObject buttonObj = Instantiate(choiceButtonPrefab, choicesPanel);
                        Button button = buttonObj.GetComponent<Button>();
                        Text buttonText = buttonObj.GetComponentInChildren<Text>();
                        buttonText.text = string.IsNullOrEmpty(choice.choiceText) ? "Далее" : choice.choiceText;

                        // Устанавливаем свойство для переноса текста
                        buttonText.horizontalOverflow = HorizontalWrapMode.Wrap;
                        buttonText.verticalOverflow = VerticalWrapMode.Overflow;

                        // Делаем кнопку адаптивной
                        RectTransform buttonRect = button.GetComponent<RectTransform>();
                        buttonRect.sizeDelta = new Vector2(buttonRect.sizeDelta.x, buttonText.preferredHeight); // Изменяем высоту кнопки в зависимости от высоты текста

                        int nextIndex = choice.nextLineIndex;
                        button.onClick.AddListener(() => OnChoiceSelected(nextIndex));
                    }
                }
            }
        }
        else
        {
            // Если нет вариантов выбора
            isShowingChoices = false;
        }
    }


    // Обработка выбора варианта
    private void OnChoiceSelected(int nextLineIndex)
    {
        isShowingChoices = false;
        isTextFullyDisplayed = false;

        if (nextLineIndex == -1)
        {
            EndDialogue(); 
        }
        else if (nextLineIndex >= 0 && nextLineIndex < dialogue.dialogueLines.Length)
        {
            currentLineIndex = nextLineIndex;
            ShowDialogueLine(); 
        }
        else
        {
            Debug.LogError($"Некорректный индекс nextLineIndex: {nextLineIndex}");
            EndDialogue();
        }
    }
    public void EndDialogue()
    {
        Time.timeScale = 1f;
        thisCallingADialog.IsDialog();
        Destroy(gameObject);
    }
}
