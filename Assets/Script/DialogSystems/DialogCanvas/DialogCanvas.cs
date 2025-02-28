using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class DialogCanvas : MonoBehaviour
{
    [SerializeField] private GameObject choiceButtonPrefab; // ������ ������ ������
    [SerializeField] private Text characterNameText; // ����� ����� ���������
    [SerializeField] private Transform choicesPanel; // ������ ��� ������ ������
    [SerializeField] private Text dialogueText; // ����� �������
    [SerializeField] private Image characterImage; // ����������� ���������
    [SerializeField] private Image background;// ����������� ����

    private DialogueLine currentDialogueLine; // ������� ������ �������
    private DialogSystem dialogue; // ������� ��������
    private int currentLineIndex = 0; // ������ ������� ������ �������
    private bool isTextFullyDisplayed = false; // ����, �����������, ��� ����� ��������� ���������
    private bool isShowingChoices = false; // ����, �����������, ��� ������������ �������� ������

    private CallingADialog thisCallingADialog;

    public void AddCalingADialog(CallingADialog callingADialog)
    {
        thisCallingADialog = callingADialog;
    }
    // ������ �������
    public void StartDialogue(DialogSystem dialogues)
    {
        if (dialogues != null && dialogues.dialogueLines.Length > 0)
        {
            dialogue = dialogues;
            currentLineIndex = 0; // ���������� ������ �� ������
            currentDialogueLine = dialogue.dialogueLines[currentLineIndex];
            characterNameText.text = currentDialogueLine.character.nameNPC;
            Time.timeScale = 0f;
            ShowDialogueLine();
        }
        else
        {
            Debug.LogError("DialogSystem �� ��� �������� ��� �� �������� ����� �������.");
        }
    }

    private void Update()
    {
        if (Input.GetMouseButtonDown(0)) 
        {
            SwitchingDialogs();
        }
    }

    // ������� � ��������� ������ �������
    private void GoToNextLine()
    {
        if (isTextFullyDisplayed && !isShowingChoices)
        {
            // ���� ����� ��������� ��������� � ��� �������� �������, ��������� � ��������� ������
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
                    ShowDialogueLine(); // ���� ��� ������, ���������� ��������� ������
                }
            }
            else
            {
                EndDialogue(); // ��������� ������
            }
        }
    }

    private void SwitchingDialogs()
    {
        GoToNextLine();
    }

    // ����� ������� ������ �������
    private void ShowDialogueLine()
    {
        foreach (Transform child in choicesPanel)
        {
            Destroy(child.gameObject); // ������� ������ ������ ������
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
                StartCoroutine(DisplayTextWithDelay(currentDialogueLine.textDialogue)); // ��������� �������� ��� ��-���������� ������
            }
            else
            {
                EndDialogue(); // ��������� ������, ���� ������ ���������
            }
        }
        else
        {
            Debug.LogError("DialogSystem �� ��� ��������.");
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
        isTextFullyDisplayed = true; // ����� ��������� ���������
        CheckForChoices(); // �������� ������� ������ ����� ������ ������
    }

    private void CheckForChoices()
    {
        foreach (Transform child in choicesPanel)
        {
            Destroy(child.gameObject); // ������� ������ ������ ������
        }

        // ���� ���� �������� ������
        if (currentDialogueLine.choices != null && currentDialogueLine.choices.Length > 0)
        {
            bool hasValidChoices = false;

            // ��������� ������� �������� ��������� ������
            foreach (DialogueChoice choice in currentDialogueLine.choices)
            {
                // �������, ����� ��� ����� ���������� �������
                if ((choice.nextLineIndex != -1 || !string.IsNullOrEmpty(choice.choiceText)))
                {
                    hasValidChoices = true;
                    break;
                }
            }

            // ���� ���� ���� �� ���� �����
            if (hasValidChoices)
            {
                isShowingChoices = true;

                // ������� ������ ��� ���� ��������� ������
                foreach (DialogueChoice choice in currentDialogueLine.choices)
                {
                    // ������� ������, ���� nextLineIndex �� -1 ��� ���� �����
                    if (choice.nextLineIndex != -1 || !string.IsNullOrEmpty(choice.choiceText))
                    {
                        GameObject buttonObj = Instantiate(choiceButtonPrefab, choicesPanel);
                        Button button = buttonObj.GetComponent<Button>();
                        Text buttonText = buttonObj.GetComponentInChildren<Text>();
                        buttonText.text = string.IsNullOrEmpty(choice.choiceText) ? "�����" : choice.choiceText;

                        // ������������� �������� ��� �������� ������
                        buttonText.horizontalOverflow = HorizontalWrapMode.Wrap;
                        buttonText.verticalOverflow = VerticalWrapMode.Overflow;

                        // ������ ������ ����������
                        RectTransform buttonRect = button.GetComponent<RectTransform>();
                        buttonRect.sizeDelta = new Vector2(buttonRect.sizeDelta.x, buttonText.preferredHeight); // �������� ������ ������ � ����������� �� ������ ������

                        int nextIndex = choice.nextLineIndex;
                        button.onClick.AddListener(() => OnChoiceSelected(nextIndex));
                    }
                }
            }
        }
        else
        {
            // ���� ��� ��������� ������
            isShowingChoices = false;
        }
    }


    // ��������� ������ ��������
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
            Debug.LogError($"������������ ������ nextLineIndex: {nextLineIndex}");
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
