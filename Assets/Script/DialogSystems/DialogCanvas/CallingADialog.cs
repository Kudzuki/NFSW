using UnityEngine;

public class CallingADialog : MonoBehaviour
{
    [SerializeField] GameObject dialogueManager;
    [SerializeField] private DialogSystem dialogue;
    [SerializeField] private int indexDialog;
    private bool isDialog = true;

    private void Update()
    {
        if (Input.GetKeyUp(KeyCode.E) && isDialog)
        {
            DialogStart();
        }
    }
    private void DialogStart()
    {
        GameObject gameObject = Instantiate(dialogueManager);
        DialogCanvas dialogues = gameObject.GetComponent<DialogCanvas>();
        dialogues.StartDialogue(dialogue);
        dialogues.AddCalingADialog(this);
        isDialog = false;
    }
    public bool IsDialog()
    {
        isDialog = !isDialog;
        return isDialog;
    }
}
