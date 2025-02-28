using UnityEngine;

[CreateAssetMenu(fileName = "DialogSystem", menuName = "Dialog System/Creet_Dialog_System")]
public class DialogSystem : ScriptableObject
{
    public DialogueLine[] dialogueLines;
}
[System.Serializable]
public class DialogueLine
{
    [TextArea(3, 10)] public string textDialogue;
    public NameNPC character;
    public int indexSprite;
    public DialogueChoice[] choices;
    public Vector2 nodePosition;
    public Vector2 scrollPosition;
    public Sprite background;
}

[System.Serializable]
public class DialogueChoice
{
    [TextArea(1, 3)] public string choiceText;
    public int nextLineIndex;
}
