using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

public class DialoguePanel : MonoBehaviour
{
    public static DialoguePanel i;
    public static int charPerSecond = 37;

    [FormerlySerializedAs("text")] public Text textDisplay;
    public CanvasGroup canvasGroup;

    public bool typing;
    public bool displayingMessage;
    private WaitForSeconds _waitForSeconds;
    private WaitUntil _waitUntilDismissed;
    private WaitUntil _waitUntilDismissedOrTimeUp;

    private float timeLastCharPrinted;

    private PuzzleGameplayInput _input;
    private GameManager _gameManager;

    // Start is called before the first frame update
    void Start()
    {
        i = this;
        _input = FindObjectOfType<PuzzleGameplayInput>();
        _gameManager = FindObjectOfType<GameManager>();
        _waitForSeconds = new WaitForSeconds(1f / charPerSecond);
        _waitUntilDismissed = new WaitUntil(() => _input.dialogueDismissed);
        _waitUntilDismissedOrTimeUp =
            new WaitUntil(() => _input.dialogueDismissed || (Time.time - timeLastCharPrinted) > (1f / charPerSecond));
        canvasGroup.alpha = 0;
    }

    public void StartConversation()
    {
        _gameManager.gameplayEnabled = false;
    }

    public void EndConversation()
    {
        _gameManager.gameplayEnabled = true;
    }

    public IEnumerator DisplayDialogue(Vector2Int dialogueKey)
    {
        var message = CustomProjectSettings.i.dialogueDatabaseAsset.scenes[dialogueKey.x].dialogues[dialogueKey.y].line;
        textDisplay.text = "";
        displayingMessage = true;
        typing = true;
        canvasGroup.alpha = 1;
        foreach (char letter in message)
        {
            textDisplay.text += letter;
            timeLastCharPrinted = Time.time;
            yield return _waitUntilDismissedOrTimeUp;
            // Display full message if player is obviously trying to skip.
            if (_input.dialogueDismissed)
            {
                break;
            }
        }

        typing = false;
        textDisplay.text = message;
        // Wait one frame before allowing message dismissal.
        yield return null;
        yield return _waitUntilDismissed;
        displayingMessage = false;
        canvasGroup.alpha = 0;
    }

    public IEnumerator DisplayDialogues(params Vector2Int[] sceneKeys)
    {
        StartConversation();
        foreach (var sceneKey in sceneKeys)
        {
            yield return DisplayDialogue(new Vector2Int(sceneKey.x, sceneKey.y));
        }
        EndConversation();
    }

    public IEnumerator DisplayDialoguesFromScene(int sceneKey)
    {
        var sceneLength = CustomProjectSettings.i.dialogueDatabaseAsset.scenes[sceneKey].dialogues.Count;
        StartConversation();
        for (int j = 0; j < sceneLength; j++)
        {
            yield return DisplayDialogue(new Vector2Int(sceneKey, j));
        }

        EndConversation();
    }
}